using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Collections;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Collections.Generic;

/*
    Authenticators: https://mirror-networking.com/docs/Components/Authenticators/
    Documentation: https://mirror-networking.com/docs/Guides/Authentication.html
    API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkAuthenticator.html
*/

public class WebAuthenticator : NetworkAuthenticator
{
    #region Messages

    public struct AuthRequestMessage : NetworkMessage {
        public string username;
        public string password;
    }

    public struct AuthResponseMessage : NetworkMessage {
        public int response;
        public string responseTxt;
        public PlayerInfo playerData;
    }

    public InputField usernameField;
    public InputField passwordField;

    public Text loginResponseText;

    public FirebaseManager firebase;

    #endregion

    #region Server

    /// <summary>
    /// Called on server from StartServer to initialize the Authenticator
    /// <para>Server message handlers should be registered in this method.</para>
    /// </summary>
    public override void OnStartServer()
    {
        // register a handler for the authentication request we expect from client
        NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
    }

    /// <summary>
    /// Called on server from OnServerAuthenticateInternal when a client needs to authenticate
    /// </summary>
    /// <param name="conn">Connection to client.</param>
    public override void OnServerAuthenticate(NetworkConnection conn) {
        
    }

    /// <summary>
    /// Called on server when the client's AuthRequestMessage arrives
    /// </summary>
    /// <param name="conn">Connection to client.</param>
    /// <param name="msg">The message payload</param>
    public void OnAuthRequestMessage(NetworkConnection conn, AuthRequestMessage msg)
    {
        Debug.Log("[Authenticator] Client " + conn.address + " is trying to authenticate for user " + msg.username);
        StartCoroutine(AuthenticateEnum(conn, msg.username, msg.password));


       
    }


    public IEnumerator AuthenticateEnum(NetworkConnection conn, string username, string password)
    {
        var LoginTask = firebase.auth.SignInWithEmailAndPasswordAsync(username, password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            //Debug.LogWarning(message: $"[Authenticator] Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            AuthResponseMessage authResponseMessage = new AuthResponseMessage()
            {
                response = 1,
                responseTxt = message
            };

            conn.Send(authResponseMessage);
        }
        else
        {
            firebase.User = LoginTask.Result;


            var DBTask = firebase.DBRefrence.Child("users").Child(firebase.User.UserId).GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);



            if (DBTask.Exception != null)
            {
                Debug.Log(DBTask.Exception);
            }
            else if (DBTask.Result.Value == null)
            {
                Debug.Log("[Authenticator] User DBTask for account info is empty!");

            }
            else
            {

                DataSnapshot snapshot = DBTask.Result;

                string[] invString = snapshot.Child("inventory").Value.ToString().Split(',');
                List<int> invList = new List<int>();
                foreach(string s in invString)
                {
                    invList.Add(int.Parse(s));
                }

                PlayerInfo pInfo = new PlayerInfo
                {
                    nickname = snapshot.Child("nickname").Value.ToString(),
                    currentOutfit = int.Parse(snapshot.Child("currentOutfit").Value.ToString()),
                    outfits = snapshot.Child("outfits").Value.ToString(),
                    x = float.Parse(snapshot.Child("x").Value.ToString()),
                    y = float.Parse(snapshot.Child("y").Value.ToString()),
                    z = float.Parse(snapshot.Child("z").Value.ToString()),
                    experience = int.Parse(snapshot.Child("experience").Value.ToString()),
                    level = int.Parse(snapshot.Child("level").Value.ToString()),
                    username = firebase.User.Email,
                    userId = firebase.User.UserId,
                    inventory = invList,
                    heldItem = int.Parse(snapshot.Child("heldItem").Value.ToString()),
                    maxHealth = int.Parse(snapshot.Child("maxHealth").Value.ToString()),
                    currentHealth = int.Parse(snapshot.Child("currentHealth").Value.ToString())

                };
                //Debug.Log("Loaded user data - " + pInfo.nickname);


                AuthResponseMessage authResponseMessage = new AuthResponseMessage()
                {
                    response = 2,
                    responseTxt = "OK",
                    playerData = pInfo
                };
                conn.authenticationData = pInfo;

                conn.Send(authResponseMessage);
                // Accept the successful authentication
                ServerAccept(conn);


                Debug.LogFormat("[Authenticator] User {0} ({1}) signed in successfully", firebase.User.DisplayName, firebase.User.Email);
                yield return new WaitForSeconds(2f);
            }

        }
    }


    #endregion

    #region Client

    /// <summary>
    /// Called on client from StartClient to initialize the Authenticator
    /// <para>Client message handlers should be registered in this method.</para>
    /// </summary>
    public override void OnStartClient()
    {
        // register a handler for the authentication response we expect from server
        NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
    }

    /// <summary>
    /// Called on client from OnClientAuthenticateInternal when a client needs to authenticate
    /// </summary>
    /// <param name="conn">Connection of the client.</param>
    public override void OnClientAuthenticate(NetworkConnection conn)
    {
        Debug.Log("[Authenticator] Sending authentication request");
        AuthRequestMessage authRequestMessage = new AuthRequestMessage
        {
            username = usernameField.text,
            password = passwordField.text
        };
        NetworkClient.Send(authRequestMessage);
    }

    /// <summary>
    /// Called on client when the server's AuthResponseMessage arrives
    /// </summary>
    /// <param name="conn">Connection to client.</param>
    /// <param name="msg">The message payload</param>
    public void OnAuthResponseMessage(NetworkConnection conn, AuthResponseMessage msg)
    {
        Debug.Log("[Authenticator] Authentication response: " + msg.response + ": " + msg.responseTxt);
        switch(msg.response)
        {
            case 1: // Wrong credentials
                loginResponseText.text = msg.responseTxt;
                StartCoroutine(DelayedDisconnect(conn, 1f));
                FindObjectOfType<GameDB>().loginBtn.enabled = true;
                break;
            case 2: // Accept the connection
                // Authentication has been accepted
                ClientAccept(conn);
                StartCoroutine(NetworkManagement.WaitForScene("MainGame", conn, msg.playerData));
                break;
            default:
                break;
        }

    }


    public IEnumerator DelayedDisconnect(NetworkConnection conn, float time) {
        yield return new WaitForSeconds(time);
        conn.Disconnect();
    }

    #endregion
}
