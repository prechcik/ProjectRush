using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

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

    private bool nicknameTaken = false;

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
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });

        DontDestroyOnLoad(this.gameObject);
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
        DBRefrence = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void LoginButton()
    {
        StartCoroutine(Login(loginUserField.text, loginPasswordField.text));
    }

    public void RegisterButton()
    {
        StartCoroutine(Register(registerUserField.text, registerPasswordField.text, registerPassword2Field.text));
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
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            loginStatusText.text = "Logged In";
            StartCoroutine(LoadUserData());
            yield return new WaitForSeconds(2f);
            if (pInfo != null)
            {
                if (pInfo.nickname == "") // Show 'Enter nickname' screen
                {
                    UIManager.ShowNick();
                } else if (pInfo.outfits.Length <= 1) // Get first outfit package
                {
                    UIManager.ShowPackage();
                } else // Just log in
                {
                    SceneManager.LoadSceneAsync("MainGame");
                    network.StartClient();
                }
                
            }

            
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
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
                loginStatusText.text = "Created account, You can log in now!";
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
                        //Now return to login screen
                        UIManager.ShowLogin();
                        registerStatusText.text = "";
                    }
                }
            }
        }
    }

    public void NicknameBtn()
    {
        string u = nicknameField.text;
        DBRefrence.Child("users").OrderByChild("nickname").EqualTo(u).ValueChanged += HandleNicknameChanged;
        
        
        
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
                network.StartClient();
            }
        }
    }

    private IEnumerator LoadUserData()
    {
        var DBTask = DBRefrence.Child("users").Child(User.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null) { 
            Debug.Log(DBTask.Exception); 
        } else if (DBTask.Result.Value == null)
        {
            Debug.Log("User DB is empty, creating sample data");
            var DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("nickname").SetValueAsync("");
            yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
            DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("currentOutfit").SetValueAsync(0);
            yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
            DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("outfits").SetValueAsync("0");
            yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
            DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("x").SetValueAsync(0);
            yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
            DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("y").SetValueAsync(0);
            yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
            DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("z").SetValueAsync(0);
            yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
            pInfo = new PlayerInfo();
            pInfo.nickname = "";
            pInfo.outfits = "0";
            pInfo.currentOutfit = 0;
            pInfo.username = User.Email;
            pInfo.userId = User.UserId;
        } else
        {
            Debug.Log("Loaded user data");
            DataSnapshot snapshot = DBTask.Result;
            pInfo = new PlayerInfo();
            pInfo.nickname = snapshot.Child("nickname").Value.ToString();
            pInfo.currentOutfit = int.Parse(snapshot.Child("currentOutfit").Value.ToString());
            pInfo.outfits = snapshot.Child("outfits").Value.ToString();
            pInfo.x = float.Parse(snapshot.Child("x").Value.ToString());
            pInfo.y = float.Parse(snapshot.Child("y").Value.ToString());
            pInfo.z = float.Parse(snapshot.Child("z").Value.ToString());
            pInfo.username = User.Email;
            pInfo.userId = User.UserId;
        }

    }

    public PlayerInfo GetPlayerInfo()
    {
        StartCoroutine(LoadUserData());
        return pInfo;
    }

    public void AddOutfit(int id)
    {
        StartCoroutine(AddPlayerOutfit(id));
    }

    public IEnumerator AddPlayerOutfit(int id)
    {
        SceneManager.LoadSceneAsync("MainGame");
        Debug.Log("Add " + id);
        string outfitS = pInfo.outfits;
        outfitS += "," + id;
        Debug.Log("Player outfits: " + outfitS);
        var DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("outfits").SetValueAsync(outfitS);
        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
        Debug.Log("Player " + pInfo.nickname + " retrieved outfit with id " + id);
        DBTask2 = DBRefrence.Child("users").Child(User.UserId).Child("currentOutfit").SetValueAsync(id);
        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
        ReloadData();
        
        network.StartClient();
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

    void HandleNicknameChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.Log("Database error: " + args.DatabaseError.Message);
        } else
        {
            DataSnapshot snapshot = args.Snapshot;
            if (!snapshot.Exists)
            {
                Debug.Log("Nickname free");
                nicknameTaken = false;
            } else
            {
                Debug.Log("Nickname taken");
                nicknameTaken = true;
            }

            if (!nicknameTaken)
            {
                StartCoroutine(UpdateNickname(nicknameField.text));
            }
            else
            {
                nicknameStatus.text = "Nickname is already taken! Please try another!";
            }
        }


        ReloadData();

    }

    public void ReloadData()
    {
        StartCoroutine(LoadUserData());
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
    }
}
