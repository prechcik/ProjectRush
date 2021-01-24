using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEngine.AI;
using Firebase;
using Firebase.Auth;
using System.Collections;
using Firebase.Database;
using System.Collections.Generic;

/*
	Documentation: https://mirror-networking.com/docs/Components/NetworkManager.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

public class NetworkManagement : NetworkManager
{
    public struct PlayerMessage : NetworkMessage
    {
        public string userId;
        public string nickname;
        public int currentOutfit;
        public string outfits;
        public Vector3 pos;
    }

    public struct LoginRequest : NetworkMessage
    {
        public string email;
        public string password;
    }

    public struct LoginResponse : NetworkMessage
    {
        public string result;
        public PlayerInfo info;
    }

    public struct PlayerRequest : NetworkMessage
    {
        public string result;
    }

    public struct PlayerOutfitRequest : NetworkMessage { }

    public struct PlayerOutfitResult : NetworkMessage
    {
        public int[] outfits;
    }

    public struct ClientReadyForPlayer : NetworkMessage
    {

    }

    public struct NicknameRequest : NetworkMessage
    {
        public string nick;
    }

    public struct NicknameResponse : NetworkMessage
    {
        public string response;
        public string nickname;
    }

    public struct OutfitAdd : NetworkMessage
    {
        public int id;
    }

    public struct OutfitAddResult : NetworkMessage { }

    public struct PlayerUpdateRequest : NetworkMessage { }

    public struct PlayerUpdateResponse : NetworkMessage
    {
        public PlayerInfo info;
    }
    public struct PackagePacket : NetworkMessage { }

    public struct OutfitRequest : NetworkMessage
    {
        public int outfitid;
    }

    public FirebaseManager DBManager;

    public NetworkStartPosition defaultStart;

    private GameObject tempObj;

    public PlayerInfo playerInfo;

    private GameDB gameDB;

    #region Unity Callbacks

public override void OnValidate()
{
    base.OnValidate();
}

/// <summary>
/// Runs on both Server and Client
/// Networking is NOT initialized when this fires
/// </summary>
public override void Awake()
{
    base.Awake();
        DBManager = FindObjectOfType<FirebaseManager>();
        gameDB = FindObjectOfType<GameDB>();
}

/// <summary>
/// Runs on both Server and Client
/// Networking is NOT initialized when this fires
/// </summary>
public override void Start()
{
    base.Start();
        StartClient();
        DBManager = FindObjectOfType<FirebaseManager>();
        gameDB = FindObjectOfType<GameDB>();
    }

    



    /// <summary>
    /// Runs on both Server and Client
    /// </summary>
    public override void LateUpdate()
{
    base.LateUpdate();
}

/// <summary>
/// Runs on both Server and Client
/// </summary>
public override void OnDestroy()
{
    base.OnDestroy();
}

#endregion

#region Start & Stop

/// <summary>
/// Set the frame rate for a headless server.
/// <para>Override if you wish to disable the behavior or set your own tick rate.</para>
/// </summary>
public override void ConfigureServerFrameRate()
{
    base.ConfigureServerFrameRate();
}

/// <summary>
/// called when quitting the application by closing the window / pressing stop in the editor
/// </summary>
public override void OnApplicationQuit()
{
    base.OnApplicationQuit();
}

#endregion

#region Scene Management

/// <summary>
/// This causes the server to switch scenes and sets the networkSceneName.
/// <para>Clients that connect to this server will automatically switch to this scene. This is called autmatically if onlineScene or offlineScene are set, but it can be called from user code to switch scenes again while the game is in progress. This automatically sets clients to be not-ready. The clients must call NetworkClient.Ready() again to participate in the new scene.</para>
/// </summary>
/// <param name="newSceneName"></param>
public override void ServerChangeScene(string newSceneName)
{
    base.ServerChangeScene(newSceneName);
}

/// <summary>
/// Called from ServerChangeScene immediately before SceneManager.LoadSceneAsync is executed
/// <para>This allows server to do work / cleanup / prep before the scene changes.</para>
/// </summary>
/// <param name="newSceneName">Name of the scene that's about to be loaded</param>
public override void OnServerChangeScene(string newSceneName) { }

/// <summary>
/// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
/// </summary>
/// <param name="sceneName">The name of the new scene.</param>
public override void OnServerSceneChanged(string sceneName) { }

/// <summary>
/// Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
/// <para>This allows client to do work / cleanup / prep before the scene changes.</para>
/// </summary>
/// <param name="newSceneName">Name of the scene that's about to be loaded</param>
/// <param name="sceneOperation">Scene operation that's about to happen</param>
/// <param name="customHandling">true to indicate that scene loading will be handled through overrides</param>
public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling) { }

/// <summary>
/// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
/// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
/// </summary>
/// <param name="conn">The network connection that the scene change message arrived on.</param>
public override void OnClientSceneChanged(NetworkConnection conn)
{
    base.OnClientSceneChanged(conn);
}

#endregion

#region Server System Callbacks

/// <summary>
/// Called on the server when a new client connects.
/// <para>Unity calls this on the Server when a Client connects to the Server. Use an override to tell the NetworkManager what to do when a client connects to the server.</para>
/// </summary>
/// <param name="conn">Connection from client.</param>
public override void OnServerConnect(NetworkConnection conn) {
        Debug.Log("Player connected " + conn.address);
    }

/// <summary>
/// Called on the server when a client is ready.
/// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
/// </summary>
/// <param name="conn">Connection from client.</param>
public override void OnServerReady(NetworkConnection conn)
{
    base.OnServerReady(conn);
}

/// <summary>
/// Called on the server when a client adds a new player with ClientScene.AddPlayer.
/// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
/// </summary>
/// <param name="conn">Connection from client.</param>
public override void OnServerAddPlayer(NetworkConnection conn)
{
    base.OnServerAddPlayer(conn);
}

/// <summary>
/// Called on the server when a client disconnects.
/// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
/// </summary>
/// <param name="conn">Connection from client.</param>
public override void OnServerDisconnect(NetworkConnection conn)
{
        Player p = conn.identity.GetComponent<Player>();
        StartCoroutine(DBManager.UpdatePlayerInfo(p));
        Debug.Log("Player " + p.nickname + " disconnected " + conn.address);
        base.OnServerDisconnect(conn);
        

}

/// <summary>
/// Called on the server when a network error occurs for a client connection.
/// </summary>
/// <param name="conn">Connection from client.</param>
/// <param name="errorCode">Error code.</param>
public override void OnServerError(NetworkConnection conn, int errorCode) { }

#endregion

#region Client System Callbacks

/// <summary>
/// Called on the client when connected to a server.
/// <para>The default implementation of this function sets the client as ready and adds a player. Override the function to dictate what happens when the client connects.</para>
/// </summary>
/// <param name="conn">Connection to the server.</param>
public override void OnClientConnect(NetworkConnection conn)
{
        base.OnClientConnect(conn);
    }

/// <summary>
/// Called on clients when disconnected from a server.
/// <para>This is called on the client when it disconnects from the server. Override this function to decide what happens when the client disconnects.</para>
/// </summary>
/// <param name="conn">Connection to the server.</param>
public override void OnClientDisconnect(NetworkConnection conn)
{
        Destroy(DBManager.gameObject);
        Destroy(gameDB.gameObject);

        StartCoroutine(DisconnectLog("Server has disconnected"));
        StartClient();
        base.OnClientDisconnect(conn);
        Destroy(this.gameObject);
}

    private IEnumerator DisconnectLog(string msg)
    {
        yield return SceneManager.LoadSceneAsync("MainMenu");
        DBManager.loginStatusText.text = msg;
    }

/// <summary>
/// Called on clients when a network error occurs.
/// </summary>
/// <param name="conn">Connection to a server.</param>
/// <param name="errorCode">Error code.</param>
public override void OnClientError(NetworkConnection conn, int errorCode) {
        Debug.Log("Error: " + errorCode);
    }

/// <summary>
/// Called on clients when a servers tells the client it is no longer ready.
/// <para>This is commonly used when switching scenes.</para>
/// </summary>
/// <param name="conn">Connection to the server.</param>
public override void OnClientNotReady(NetworkConnection conn) { }

#endregion

#region Start & Stop Callbacks

// Since there are multiple versions of StartServer, StartClient and StartHost, to reliably customize
// their functionality, users would need override all the versions. Instead these callbacks are invoked
// from all versions, so users only need to implement this one case.

/// <summary>
/// This is invoked when a host is started.
/// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
/// </summary>
public override void OnStartHost() { }

/// <summary>
/// This is invoked when a server is started - including when a host is started.
/// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
/// </summary>
public override void OnStartServer() {
        base.OnStartServer();
        NetworkServer.RegisterHandler<PlayerMessage>(OnPlayerMessage);
        NetworkServer.RegisterHandler<LoginRequest>(OnLoginRequest);
        NetworkServer.RegisterHandler<PlayerOutfitRequest>(OnOutfitRequest);
        NetworkServer.RegisterHandler<OutfitRequest>(OnPlayerChangeOutfit);
        NetworkServer.RegisterHandler<ClientReadyForPlayer>(OnClientReadyToSpawn);
        NetworkServer.RegisterHandler<NicknameRequest>(OnNicknameRequest);
        NetworkServer.RegisterHandler<OutfitAdd>(OnOutfitAdd);
        NetworkServer.RegisterHandler<PlayerUpdateRequest>(OnPlayerUpdateRequest);
        Debug.Log("Initializing server map");
        SceneManager.LoadSceneAsync("MainGame");
    }

/// <summary>
/// This is invoked when the client is started.
/// </summary>
public override void OnStartClient() {
        NetworkClient.RegisterHandler<LoginResponse>(OnLoginResponse);
        NetworkClient.RegisterHandler<PlayerOutfitResult>(OnOutfitResult);
        NetworkClient.RegisterHandler<NicknameResponse>(OnNicknameResponse);
        NetworkClient.RegisterHandler<OutfitAddResult>(OnOutfitAddResult);
        NetworkClient.RegisterHandler<PackagePacket>(OnPackagePacket);
        NetworkClient.RegisterHandler<PlayerUpdateResponse>(OnPlayerUpdateResponse);
    }

/// <summary>
/// This is called when a host is stopped.
/// </summary>
public override void OnStopHost() { }

/// <summary>
/// This is called when a server is stopped - including when a host is stopped.
/// </summary>
public override void OnStopServer() { }

/// <summary>
/// This is called when a client is stopped.
/// </summary>
public override void OnStopClient() { }

    #endregion



    void OnPlayerMessage(NetworkConnection conn, PlayerMessage message)
    {
        
        GameObject obj = GameObject.Instantiate(playerPrefab, message.pos, Quaternion.identity) ;
        obj.transform.name = message.nickname;
        Player p = obj.GetComponent<Player>();

        p.nickname = message.nickname;
        p.currentOutfit = message.currentOutfit;
        p.userId = message.userId;

        p.info = new PlayerInfo
        {
            nickname = message.nickname,
            userId = message.userId,
            currentOutfit = message.currentOutfit,
            x = message.pos.x,
            y = message.pos.y,
            z = message.pos.z
        };

        NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
        agent.enabled = false;
        agent.Warp(message.pos);
        agent.enabled = true;
        NetworkServer.AddPlayerForConnection(conn, obj);
        Debug.Log("Player connected: " + p.nickname);
    }

    

    void OnLoginRequest(NetworkConnection conn, LoginRequest message)
    {
        Debug.Log("Connection " + conn.address + " is requesting login for username " + message.email);
        StartCoroutine(LoginEnum(message, conn));
    }


    IEnumerator LoginEnum(LoginRequest message, NetworkConnection conn)
    {
        var LoginTask = DBManager.auth.SignInWithEmailAndPasswordAsync(message.email, message.password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string emessage = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    emessage = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    emessage = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    emessage = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    emessage = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    emessage = "Account does not exist";
                    break;
            }
            LoginResponse r = new LoginResponse
            {
                result = emessage
            };
            conn.Send(r);
            //loginStatusText.text = message;
        }
        else
        {
            //User is now logged in
            //Now get the result
            DBManager.User = LoginTask.Result;
            Debug.LogFormat("Firebase: User signed in successfully: {0} ({1})", DBManager.User.DisplayName, DBManager.User.Email);

            var DBTask = DBManager.DBRefrence.Child("users").Child(DBManager.User.UserId).GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
            

            if (DBTask.Exception != null)
            {
                Debug.Log(DBTask.Exception);
            }
            else if (DBTask.Result.Value == null)
            {
                Debug.Log("User DB is empty, creating sample data");
                var DBTask2 = DBManager.DBRefrence.Child("users").Child(DBManager.User.UserId).Child("nickname").SetValueAsync("");
                yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
                DBTask2 = DBManager.DBRefrence.Child("users").Child(DBManager.User.UserId).Child("currentOutfit").SetValueAsync(0);
                yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
                DBTask2 = DBManager.DBRefrence.Child("users").Child(DBManager.User.UserId).Child("outfits").SetValueAsync("0");
                yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
                DBTask2 = DBManager.DBRefrence.Child("users").Child(DBManager.User.UserId).Child("x").SetValueAsync(0);
                yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
                DBTask2 = DBManager.DBRefrence.Child("users").Child(DBManager.User.UserId).Child("y").SetValueAsync(0);
                yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
                DBTask2 = DBManager.DBRefrence.Child("users").Child(DBManager.User.UserId).Child("z").SetValueAsync(0);
                yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
                DBTask2 = DBManager.DBRefrence.Child("users").Child(DBManager.User.UserId).Child("experience").SetValueAsync(0);
                yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
                DBTask2 = DBManager.DBRefrence.Child("users").Child(DBManager.User.UserId).Child("level").SetValueAsync(1);
                yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
                PlayerInfo pInfo = new PlayerInfo
                {
                    nickname = "",
                    outfits = "0",
                    currentOutfit = 0,
                    username = DBManager.User.Email,
                    userId = DBManager.User.UserId,
                    x = 0,
                    y = 0,
                    z = 0
                };
                DBManager.pInfo = pInfo;
                OnPlayerInfo(conn, pInfo);
            }
            else
            {
                
                DataSnapshot snapshot = DBTask.Result;
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
                    username = DBManager.User.Email,
                    userId = DBManager.User.UserId
                };
                Debug.Log("Loaded user data - " + pInfo.nickname);
                DBManager.pInfo = pInfo;
                OnPlayerInfo(conn, pInfo);
            }

            

            

        }
    }


    void OnLoginResponse(NetworkConnection conn, LoginResponse message)
    {
        Debug.Log("Received login response from server: " + message.result);
        if (message.result == "OK")
        {
            playerInfo = message.info;
            if (message.info.nickname.Length < 1)
            {
                UIManager.ShowNick();
            }
            else if (message.info.currentOutfit == 0)
            {
                UIManager.ShowPackage();
            }
            else
            {
                SceneManager.LoadSceneAsync("MainGame");
                conn.Send(new ClientReadyForPlayer());
            }
        } else
        {
            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                SceneManager.LoadSceneAsync("MainMenu");
                
            }
            DBManager.loginStatusText.text = message.result;
        }
    }


    void OnPlayerInfo(NetworkConnection conn, PlayerInfo info)
    {
        LoginResponse r = new LoginResponse
        {
            result = "OK",
            info = info
        };
        conn.Send(r);

        GameObject obj = GameObject.Instantiate(playerPrefab, new Vector3(info.x,info.y,info.z), Quaternion.identity);
        Debug.Log("Spawning " + info.nickname + " at (" + info.x + "," + info.y + "," + info.z + ")");
        NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
        agent.Warp(new Vector3(info.x, info.y, info.z));
        agent.enabled = false;
        obj.name = info.nickname;
        Player p = obj.GetComponent<Player>();

        p.nickname = info.nickname;
        p.currentOutfit = info.currentOutfit;
        p.userId = info.userId;
        p.info = info;
        p.outfits = new List<int>(System.Array.ConvertAll(info.outfits.Split(','), int.Parse));

        
        agent.enabled = true;

        tempObj = obj;
        playerInfo = info;
        

        
        Debug.Log("Player connected: " + p.nickname);
    }

    public void OnClientReadyToSpawn(NetworkConnection conn, ClientReadyForPlayer message)
    {
        NetworkServer.AddPlayerForConnection(conn, tempObj);
    }

    

    public void LoginBtn()
    {
        if (DBManager.loginUserField.text.Length < 1 || DBManager.loginPasswordField.text.Length < 1)
        {
            DBManager.loginStatusText.text = "Please enter both email and password!";
            return;
        }
        Debug.Log("Sending login request");
        
        LoginRequest r = new LoginRequest
        {
            email = DBManager.loginUserField.text,
            password = DBManager.loginPasswordField.text
        };
        NetworkClient.Send(r);
        
    }


    public void OnOutfitRequest(NetworkConnection conn, PlayerOutfitRequest message)
    {
        StartCoroutine(GetOutfits(conn));
    }

    public void OnOutfitResult(NetworkConnection conn, PlayerOutfitResult message)
    {
        Debug.Log("Player outfits: ");
        foreach (int outfit in message.outfits)
        {
            Debug.Log("Outfit id: " + outfit);
        }
        OutfitPanel panel = FindObjectOfType<OutfitPanel>();
        panel.PopulateIcons(new List<int>(message.outfits));
    }


    IEnumerator GetOutfits(NetworkConnection conn)
    {
        Player p = conn.identity.GetComponent<Player>();
        Debug.Log("Received outfit list request for player " + p.nickname);
        var DBTask = DBManager.DBRefrence.Child("users").Child(DBManager.User.UserId).Child("outfits").GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        if (DBTask.Exception != null)
        {
            Debug.Log(DBTask.Exception);
        }
        else
        {
            string res = DBTask.Result.Value.ToString();
            int[] outfits = System.Array.ConvertAll(res.Split(','), int.Parse);
            p.outfits = new List<int>(outfits);
            PlayerOutfitResult msg = new PlayerOutfitResult
            {
                outfits = outfits
            };
            conn.Send(msg);

        }
    }

    public void OnPlayerChangeOutfit(NetworkConnection conn, OutfitRequest message)
    {
        conn.identity.GetComponent<Player>().currentOutfit = message.outfitid;
        Debug.Log("Player " + conn.identity.GetComponent<Player>().nickname + " has changed outfit to id " + message.outfitid);
    }

    public void NicknameBtn()
    {
        NetworkClient.Send(new NicknameRequest { nick = DBManager.nicknameField.text });
    }

    public void CheckAndSendNickname()
    {

    }

    public void OnNicknameRequest(NetworkConnection conn, NicknameRequest message)
    {
        var DBTask = DBManager.DBRefrence.Child("users").OrderByChild("nickname").EqualTo(message.nick).GetValueAsync().ContinueWith(task =>
        {
            NicknameResponse response = new NicknameResponse { };
            DataSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                response.response = "Username already exists";
                response.nickname = "";
            } else
            {
                response.response = "OK";
                response.nickname = message.nick;
                StartCoroutine(NicknameEnum(conn, message.nick));
            }
            conn.Send(response);
        });
    }

    public void OnNicknameResponse(NetworkConnection conn, NicknameResponse message)
    {
        if (message.response == "OK")
        {
            Debug.Log("Username was free");
            UIManager.ShowPackage();
        } else
        {
            DBManager.nicknameStatus.text = message.response;
        }
    }

    IEnumerator NicknameEnum(NetworkConnection conn, string nick)
    {
        var DBTask = DBManager.DBRefrence.Child("users").Child(DBManager.User.UserId).Child("nickname").SetValueAsync(nick);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.Log(DBTask.Exception);
        }
        else
        {
            
        }
    }

    public void OnOutfitAdd(NetworkConnection conn, OutfitAdd message)
    {
        string playerOutfits = playerInfo.outfits;
        string newPlayerOutfits = playerOutfits + "," + message.id;
        StartCoroutine(OutfitAddEnum(newPlayerOutfits, message.id, conn));
    }

    IEnumerator OutfitAddEnum(string newoutfits, int newOutfit, NetworkConnection conn)
    {
        conn.identity.GetComponent<Player>().info.outfits = newoutfits;
        var DBTask = DBManager.DBRefrence.Child("users").Child(DBManager.User.UserId).Child("outfits").SetValueAsync(newoutfits);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.Log(DBTask.Exception);
        }
        else
        {
            if (playerInfo.currentOutfit == 0)
            {
                DBTask = DBManager.DBRefrence.Child("users").Child(DBManager.User.UserId).Child("currentOutfit").SetValueAsync(newOutfit);
                yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
                if (DBTask.Exception != null)
                {
                    Debug.Log(DBTask.Exception);
                }
                playerInfo.currentOutfit = newOutfit;
                conn.identity.GetComponent<Player>().currentOutfit = newOutfit;
                conn.identity.GetComponent<Player>().info.currentOutfit = newOutfit;
            }
            conn.identity.GetComponent<Player>().info.outfits = newoutfits;
            conn.identity.GetComponent<Player>().outfits.Add(newOutfit);
            playerInfo.currentOutfit = newOutfit;
            playerInfo.outfits = newoutfits;
            conn.Send(new OutfitAddResult { });
        }
    }

    public void OnOutfitAddResult(NetworkConnection conn, OutfitAddResult message)
    {

    }

    public void AddPlayerOutfitStart(int id)
    {
        NetworkClient.Send(new OutfitAdd { id = id });
        SceneManager.LoadSceneAsync("MainGame");
        NetworkClient.Send(new NetworkManagement.ClientReadyForPlayer());
    }

    public void AddPlayerOutfit(int id)
    {
        NetworkClient.Send(new OutfitAdd { id = id });
        
    }

    public void OnPackagePacket(NetworkConnection conn, PackagePacket message)
    {
        FindObjectOfType<MainUI>().ShowPackage();
    }

    public void OnPlayerUpdateRequest(NetworkConnection conn, PlayerUpdateRequest message)
    {

        PlayerInfo info = conn.identity.GetComponent<Player>().info;
        DBManager.UpdatePlayerData(info);
        conn.Send(new PlayerUpdateResponse { });
    }

    public void OnPlayerUpdateResponse(NetworkConnection conn, PlayerUpdateResponse message)
    {
        Debug.Log("Saved player data");
    }


}
