using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Mirror;
using System.Threading.Tasks;

public class FirebaseManager : MonoBehaviour
{
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;
    public DatabaseReference DBRefrence;
    public NetworkManagement network;
    [Header("Login")]
    public InputField loginUserField;
    public InputField loginPasswordField;
    public Text loginStatusText;
    [Header("Register")]
    public InputField registerUserField;
    public InputField registerPasswordField;
    public InputField registerPassword2Field;
    public Text registerStatusText;
    [Header("Nickname Selection")]
    public InputField nicknameField;
    public Text nicknameStatus;

    private int tempId;

    public string userId;

    public PlayerInfo pInfo;

    private bool usernameTaken = false;



    private int[] tempOutfits;

    void Awake()
    {
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //If they are avalible Initialize Firebase
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("[Firebase] Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });


        DontDestroyOnLoad(this.gameObject);
    }

    private void OnApplicationQuit()
    {
        
    }

    private void InitializeFirebase()
    {
        Debug.Log("[Firebase] Setting up Firebase Auth");
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
        //FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
        DBRefrence = FirebaseDatabase.DefaultInstance.RootReference;
        
    }

    public void LoginButton()
    {
        StartCoroutine(Login(loginUserField.text, loginPasswordField.text));
    }

    public void RegisterButton()
    {
        StartCoroutine(Register(registerUserField.text, registerPasswordField.text, registerPassword2Field.text, nicknameField.text));
    }

    

    private IEnumerator Login(string _email, string _password)
    {
        loginStatusText.text = "Logging in...";
        //Call the Firebase auth signin function passing the email and password
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
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
            loginStatusText.text = message;
        }
        else
        {
            //User is now logged in
            //Now get the result
            User = LoginTask.Result;
            Debug.LogFormat("[Firebase] User {0} ({1}) signed in successfully", User.DisplayName, User.Email);
            loginStatusText.text = "Logged In";
            yield return new WaitForSeconds(2f);
            

            
        }
    }

    private IEnumerator Register(string _email, string _password, string _username, string _nickname)
    {
        StartCoroutine(CheckNickname(_nickname));
        if (usernameTaken)
        {
            registerStatusText.text = "Nickname already taken!";
        } else if (_username == "")
        {
            //If the username field is blank show a warning
            registerStatusText.text = "Missing Username";
        }
        else if (registerPasswordField.text != registerPassword2Field.text)
        {
            //If the password does not match show a warning
            registerStatusText.text = "Password Does Not Match!";
        }
        else
        {
            //Call the Firebase auth signin function passing the email and password
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
                registerStatusText.text = message;
                loginStatusText.text = message;
            }
            else
            {
                //User has now been created
                //Now get the result
                User = RegisterTask.Result;

                if (User != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    //Call the Firebase auth update user profile function passing the profile with the username
                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);


                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        registerStatusText.text = "Username Set Failed!";
                    }
                    else
                    {
                        //Username is now set

                        // Create initial data
                        var DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("nickname").SetValueAsync(_nickname);
                        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
                        DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("currentOutfit").SetValueAsync(network.defaultPlayerModelId);
                        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
                        DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("outfits").SetValueAsync("" + network.defaultPlayerModelId);
                        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
                        DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("x").SetValueAsync(0);
                        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
                        DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("y").SetValueAsync(0);
                        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
                        DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("z").SetValueAsync(0);
                        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
                        DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("experience").SetValueAsync(0);
                        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
                        DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("level").SetValueAsync(1);
                        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);

                        //Now return to login screen
                        loginUserField.text = _email;
                        UIManager.ShowLogin();
                        loginStatusText.text = "Account created successfully! You can log in now!";
                    }
                }
            }
        }
    }

    


    public IEnumerator UpdateNickname(string nick)
    {
        

        var DBTask = DBRefrence.Child("users").Child(User.UserId).Child("nickname").SetValueAsync(nick);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.Log(DBTask.Exception);
        }
        else
        {
            if (pInfo.currentOutfit == 0 && pInfo.outfits.Length <= 1) // Get first outfit package
            {
                UIManager.ShowPackage();
            }
            else // Just log in
            {
                SceneManager.LoadSceneAsync("MainGame");
                NetworkClient.Send(new NetworkManagement.ClientReadyForPlayer());
            }
        }
    }


    public IEnumerator UpdatePlayerInfo(Player p)
    {
        var DBTask = DBRefrence.Child("users").Child(p.userId).Child("x").SetValueAsync(p.playerPosition.x);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        DBTask = DBRefrence.Child("users").Child(p.userId).Child("y").SetValueAsync(p.playerPosition.y);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        DBTask = DBRefrence.Child("users").Child(p.userId).Child("z").SetValueAsync(p.playerPosition.z);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        DBTask = DBRefrence.Child("users").Child(p.userId).Child("currentOutfit").SetValueAsync(p.currentOutfit);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        DBTask = DBRefrence.Child("users").Child(p.userId).Child("experience").SetValueAsync(p.currentExp);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        DBTask = DBRefrence.Child("users").Child(p.userId).Child("level").SetValueAsync(p.info.level);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
    }

    

    

    public void AddOutfit(int id)
    {
        StartCoroutine(AddPlayerOutfit(id));
    }

    public IEnumerator AddPlayerOutfit(int id)
    {
        string outfitS = network.playerInfo.outfits;
        outfitS += "," + id;
        network.playerInfo.outfits = outfitS;
        var DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("outfits").SetValueAsync(outfitS);
        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
        //Debug.Log("Player " + pInfo.nickname + " retrieved outfit with id " + id);
        DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("currentOutfit").SetValueAsync(id);
        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
        SceneManager.LoadSceneAsync("MainGame");
        NetworkClient.Send(new NetworkManagement.ClientReadyForPlayer());
    }

    public string GetPlayerNick()
    {
        return pInfo.nickname;
    }

    public int GetPlayerOutfit(string id)
    {
        StartCoroutine(PlayerOutfitRoutine(id));
        return tempId;
    }

    public IEnumerator PlayerOutfitRoutine(string id) {
        var DBTask2 = DBRefrence.Child("users").Child(id).Child("currentOutfit").GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
        tempId = int.Parse(DBTask2.Result.Value.ToString());
    }

    

    public void UpdatePlayerData(PlayerInfo p)
    {
        StartCoroutine(UPlayerData(p));   
    }

    public IEnumerator UPlayerData(PlayerInfo p)
    {
        var DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("nickname").SetValueAsync(p.nickname);
        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
        DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("currentOutfit").SetValueAsync(p.currentOutfit);
        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
        DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("x").SetValueAsync(p.x);
        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
        DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("y").SetValueAsync(p.y);
        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
        DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("z").SetValueAsync(p.z);
        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
        DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("outfits").SetValueAsync(p.outfits);
        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
        DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("level").SetValueAsync(p.level);
        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
        DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("experience").SetValueAsync(p.experience);
        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
        Debug.Log("[Firebase] Saved data for player [" + p.nickname + "]");

    }

    public IEnumerator CheckNickname(string nick)
    {
        bool res = true;

        var DBTask = DBRefrence.Child("users").OrderByChild("nickname").EqualTo(nick).GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        if (DBTask.Exception != null)
        {
            Debug.Log(DBTask.Exception);
        } else
        {
            DataSnapshot snapshot = DBTask.Result;
            if (snapshot.Exists)
            {
                res = true;
            } else
            {
                res = false;
            }
        }
        usernameTaken = res;
    }

}
