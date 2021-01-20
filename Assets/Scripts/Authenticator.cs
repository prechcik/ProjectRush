using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEngine.Networking;
using System.Collections;

/*
    Authenticators: https://mirror-networking.com/docs/Components/Authenticators/
    Documentation: https://mirror-networking.com/docs/Guides/Authentication.html
    API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkAuthenticator.html
*/

public class Authenticator : NetworkAuthenticator
{
    #region Messages

public struct AuthRequestMessage : NetworkMessage {
        public string username;
        public string password;
    }

public struct AuthResponseMessage : NetworkMessage {
        public string response;
        public PlayerInfo player;
    }

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
public override void OnServerAuthenticate(NetworkConnection conn) { }

/// <summary>
/// Called on server when the client's AuthRequestMessage arrives
/// </summary>
/// <param name="conn">Connection to client.</param>
/// <param name="msg">The message payload</param>
public void OnAuthRequestMessage(NetworkConnection conn, AuthRequestMessage msg)
{
    AuthResponseMessage authResponseMessage = new AuthResponseMessage();


    StartCoroutine(SendPostCoroutine(msg.username, msg.password));
    
    conn.Send(authResponseMessage);

    // Accept the successful authentication
    ServerAccept(conn);
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
    AuthRequestMessage authRequestMessage = new AuthRequestMessage();

    NetworkClient.Send(authRequestMessage);
}

/// <summary>
/// Called on client when the server's AuthResponseMessage arrives
/// </summary>
/// <param name="conn">Connection to client.</param>
/// <param name="msg">The message payload</param>
public void OnAuthResponseMessage(NetworkConnection conn, AuthResponseMessage msg)
{
    // Authentication has been accepted
    ClientAccept(conn);
}

    #endregion

    IEnumerator SendPostCoroutine(string u, string p)
    {
        string url = "http://192.168.0.110/ProjectRush/api/logininfo.php";
        WWWForm f = new WWWForm();
        f.AddField("username", u);
        f.AddField("password", p);
        UnityWebRequest www = UnityWebRequest.Post(url, f);
        Debug.Log("Sending authentication request");
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
        } else
        {
            Debug.Log("Response: '" + www.downloadHandler.text + "'");
            PlayerInfo pInfo = JsonUtility.FromJson<PlayerInfo>(www.downloadHandler.text);
            if (pInfo != null)
            {
                Debug.Log("Retrieved user data for account " + pInfo.username);
            }
        }

    }

}



