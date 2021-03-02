using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEngine.AI;
using Firebase;
using Firebase.Auth;
using System.Collections;
using Firebase.Database;
using System.Collections.Generic;
using System.IO;

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
        public PlayerInfo info;
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

    public struct OutfitAddStart : NetworkMessage
    {
        public int id;
        public PlayerInfo info;
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

    public struct TimeUpdate : NetworkMessage
    {
        public float time;
    }

    public struct BoostRequest : NetworkMessage
    {
        public string type;
        public float amount;
        public float time;
    }

    public struct BoostResponse : NetworkMessage
    {
    }

    public struct PlayerInventoryRequest : NetworkMessage
    {

    }
    
    public struct PlayerInventoryResponse : NetworkMessage
    {
        public int[] invList;
    }

    public struct EquipItemRequest : NetworkMessage
    {
        public int itemId;
    }
    public struct EquipItemResponse : NetworkMessage
    {
        public int response;
    }

    public struct DestroyItemRequest : NetworkMessage
    {
        public int slotId;
    }

    public struct DestroyItemResponse : NetworkMessage
    {
        public int response;
    }



    public FirebaseManager DBManager;

    public NetworkStartPosition defaultStart;

    private GameObject tempObj;

    public PlayerInfo playerInfo;

    private GameDB gameDB;
    public float loadingSpeed = 2f;

    [Range(0,24)] public float TimeOfDay;
    public float timeSpeed;

    [Header("Settings")]
    public int defaultPlayerModelId = 15;

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
#if UNITY_SERVER
        string dataPath = Application.dataPath + "/StreamingAssets/";
        Debug.Log("Data path: " + dataPath);
        StreamReader sr = new StreamReader(dataPath + "serverIp.cfg");
        string fileIp = sr.ReadToEnd();
        sr.Close();
        Debug.Log("Loaded server ip " + fileIp + " from serverIp.cfg");
        if (fileIp.Length > 0)
        {
            this.networkAddress = fileIp;
        }
#endif
    }

/// <summary>
/// Runs on both Server and Client
/// Networking is NOT initialized when this fires
/// </summary>
public override void Start()
{
    base.Start();
        //StartClient();
        DBManager = FindObjectOfType<FirebaseManager>();
        gameDB = FindObjectOfType<GameDB>();
        
    }

    [ServerCallback]
    private void Update()
    {
        TimeOfDay += Time.deltaTime / timeSpeed;
        TimeOfDay %= 24;
        if (NetworkServer.active)
        NetworkServer.SendToAll(new TimeUpdate { time = TimeOfDay / 24f });
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
        DBManager.auth.SignOut();
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
        //Debug.Log("Player connected " + conn.address);
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
        
    //base.OnServerAddPlayer(conn);
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
        
        //Player p = conn.identity.GetComponent<Player>();
        //StartCoroutine(DBManager.UpdatePlayerInfo(p));
        StartCoroutine(DisconnectLog("Server has disconnected"));
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
        NetworkServer.RegisterHandler<PlayerOutfitRequest>(OnOutfitRequest);
        NetworkServer.RegisterHandler<OutfitRequest>(OnPlayerChangeOutfit);
        NetworkServer.RegisterHandler<ClientReadyForPlayer>(OnClientReadyToSpawn);
        NetworkServer.RegisterHandler<NicknameRequest>(OnNicknameRequest);
        NetworkServer.RegisterHandler<BoostRequest>(OnBoostRequest);
        NetworkServer.RegisterHandler<OutfitAdd>(OnOutfitAdd);
        NetworkServer.RegisterHandler<PlayerUpdateRequest>(OnPlayerUpdateRequest);
        NetworkServer.RegisterHandler<PlayerInventoryRequest>(OnPlayerInventoryRequest);
        NetworkServer.RegisterHandler<EquipItemRequest>(OnEquipItem);
        NetworkServer.RegisterHandler<DestroyItemRequest>(OnDestroyItem);
        Debug.Log("[Server] Initializing server map");
        StartCoroutine(LoadServerScene());
    }

    IEnumerator LoadServerScene()
    {
        yield return SceneManager.LoadSceneAsync("MainGame");
        Debug.Log("[Server] Finished loading main scene");
        NetworkStartPosition startPos = FindObjectOfType<NetworkStartPosition>();
        if (startPos != null)
        {
            startPositions[0] = startPos.transform;
            defaultStart = startPos;
            Debug.Log("[Server] Added server start pos: (" + startPos.transform.position.x + "," + startPos.transform.position.y + "," + startPos.transform.position.z + ")");
        }
        EnemySpawner enemySpawner = FindObjectOfType<EnemySpawner>();
        if (enemySpawner != null)
        {
            enemySpawner.Spawn();
        }
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
        NetworkClient.RegisterHandler<BoostResponse>(OnBoostResponse);
        NetworkClient.RegisterHandler<TimeUpdate>(OnTimeUpdate, false);
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




    void OnLoginResponse(NetworkConnection conn, LoginResponse message)
    {
        Debug.Log("Received login response from server: " + message.result);
        if (message.result == "OK")
        {
            
            playerInfo = message.info;
            StartCoroutine(WaitForScene("MainGame", conn, message.info));
        } else
        {
            if (message.result == "NICKNAME")
            {
                if (message.info.nickname.Length < 1)
                {
                    UIManager.ShowNick();
                }
            }
            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                SceneManager.LoadSceneAsync("MainMenu");
                
            }
            DBManager.loginStatusText.text = message.result;
        }
    }

    public static IEnumerator WaitForScene(string sceneName, NetworkConnection conn, PlayerInfo info)
    {
        
        Debug.Log("Loading main game scene");
        
        yield return SceneManager.LoadSceneAsync(sceneName).isDone;
        yield return new WaitForSeconds(2f);
        GameDB.mainUI.progressImage.fillAmount = 0f;
        ClientScene.Ready(conn);
        
        float elapsed = 0f;
        while (elapsed < 2f)
        {
            GameDB.mainUI.progressImage.fillAmount = Mathf.Lerp(GameDB.mainUI.progressImage.fillAmount, 1f, Time.deltaTime * 2f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        GameDB.mainUI.progressImage.fillAmount = 1f;

        Debug.Log("Finished loading, sending ready packet");
        conn.Send(new ClientReadyForPlayer());

        
        
    }



    public void OnClientReadyToSpawn(NetworkConnection conn, ClientReadyForPlayer message)
    {
        Transform startPos = GetStartPosition();
        PlayerInfo pi = (PlayerInfo)conn.authenticationData;
        Vector3 spawnPoint = new Vector3(pi.x,pi.y,pi.z);
        
        bool isSpawnOnMesh = IsAgentOnNavMesh(spawnPoint);
        NavMeshHit hit;
        // Check for nearest point on navmesh to agent, within onMeshThreshold
        if (NavMesh.SamplePosition(spawnPoint, out hit, Mathf.Infinity, NavMesh.AllAreas))
        {
            if (hit.hit)
            {
                spawnPoint = hit.position;
            }
        }

        // Initialize player prefab
        GameObject obj = GameObject.Instantiate(playerPrefab, startPos.position, Quaternion.identity);
        NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();

        // Set player prefab options
        agent.Warp(spawnPoint);
        obj.gameObject.name = pi.nickname;
        Player p = obj.GetComponent<Player>();
        p.nickname = pi.nickname;
        p.currentOutfit = pi.currentOutfit;
        p.userId = pi.userId;
        p.info = pi;
        p.lastPos = spawnPoint;
        p.lastLocalPos = spawnPoint;
        p.currentExp = pi.experience;
        p.currentLevel = pi.level;
        p.level = pi.level;
        tempObj = obj;
        playerInfo = pi;
        p.currentHealth = pi.currentHealth;
        agent.enabled = false;
        

        // Load player outfits
        // Check if player has multiple outfits
        if (pi.outfits.Contains(","))
        {
            p.outfits = new List<int>(System.Array.ConvertAll(pi.outfits.Split(','), int.Parse));
        } else
        {
            p.outfits = new List<int>();
            p.outfits.Add(int.Parse("" + pi.outfits));
        }

        // Spawn player on server

        Player[] players = FindObjectsOfType<Player>();
        if (players.Length > 0)
        {
            bool alreadySpawned = false, s = false;
            foreach (Player play in players)
            {
                if (play.spawnedAndReady && p.nickname == play.nickname)
                {
                    alreadySpawned = true;
                    s = true;
                    NetworkServer.ReplacePlayerForConnection(p.netIdentity.connectionToServer, obj);
                    Debug.Log("[Server] Replacing connection for player - " + p.nickname);
                    conn.identity.GetComponent<NavMeshAgent>().enabled = true;
                    conn.identity.GetComponent<Player>().spawnedAndReady = true;
                }
            }
            if (!alreadySpawned && !s)
            {
                NetworkServer.AddPlayerForConnection(conn, obj);
                Debug.Log("[Server] Player connected - " + p.nickname);
                conn.identity.GetComponent<NavMeshAgent>().enabled = true;
                conn.identity.GetComponent<Player>().spawnedAndReady = true;
                s = true;
                alreadySpawned = true;
            }
        }


        
        
    }

    

    public void LoginBtn()
    {
        FindObjectOfType<GameDB>().loginBtn.enabled = false;
        StartClient();
        if (DBManager.loginUserField.text.Length < 1 || DBManager.loginPasswordField.text.Length < 1)
        {
            DBManager.loginStatusText.text = "Please enter both email and password!";
            return;
        }
        StartCoroutine(WaitForConnection());
        
        
    }

    public IEnumerator WaitForConnection()
    {
        yield return NetworkClient.isConnected;
        Debug.Log("Sending login request");

        WebAuthenticator.AuthRequestMessage r = new WebAuthenticator.AuthRequestMessage
        {
            username = DBManager.loginUserField.text,
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
        //Debug.Log("Player outfits: ");
        //foreach (int outfit in message.outfits)
        //{
            //Debug.Log("Outfit id: " + outfit);
        //}
        OutfitPanel panel = FindObjectOfType<OutfitPanel>();
        panel.PopulateIcons(new List<int>(message.outfits));
    }


    IEnumerator GetOutfits(NetworkConnection conn)
    {
        Player p = conn.identity.GetComponent<Player>();
        //Debug.Log("Received outfit list request for player " + p.nickname);
        var DBTask = DBManager.DBRefrence.Child("users").Child(p.userId).Child("outfits").GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        if (DBTask.Exception != null)
        {
            Debug.Log(DBTask.Exception);
        }
        else
        {
            string res = DBTask.Result.Value.ToString();
            int[] outfits = System.Array.ConvertAll(res.Split(','), int.Parse);
            conn.identity.GetComponent<Player>().outfits = new List<int>(outfits);
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
        Debug.Log("[Server] Player [" + conn.identity.GetComponent<Player>().nickname + "] has changed outfit to id " + message.outfitid);
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

    public bool CheckNickname(string nick)
    {
        bool res = false;
        var DBTask = DBManager.DBRefrence.Child("users").OrderByChild("nickname").EqualTo(nick).GetValueAsync().ContinueWith(task =>
        {
            DataSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                res = false;
            }
            else
            {
                res = true;
            }
        });
        return res;
    }

    public void OnNicknameResponse(NetworkConnection conn, NicknameResponse message)
    {
        if (message.response == "OK")
        {
            OnLoginResponse(conn, new LoginResponse { info = playerInfo, result = "OK" });
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
        string[] pO = playerOutfits.Split(',');
        bool found = false;
        if (pO.Length > 0)
        {
            foreach (string o in pO)
            {
                if (o == message.id.ToString())
                {
                    found = true;
                }
            }
        } 
        if (!found)
        {
            string newPlayerOutfits = playerOutfits + "," + message.id;
            StartCoroutine(OutfitAddEnum(newPlayerOutfits, message.id, conn));
        }
        
    }

    public void OnOutfitAddStart(NetworkConnection conn, OutfitAddStart message)
    {

    }

    IEnumerator OutfitAddEnum(string newoutfits, int newOutfit, NetworkConnection conn)
    {

        conn.identity.GetComponent<Player>().info.outfits = newoutfits;
        var DBTask = DBManager.DBRefrence.Child("users").Child(conn.identity.GetComponent<Player>().userId).Child("outfits").SetValueAsync(newoutfits);
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
            playerInfo.outfits = newoutfits;
            conn.Send(new OutfitAddResult { });
        }
    }

    public void OnOutfitAddResult(NetworkConnection conn, OutfitAddResult message)
    {

    }

    public void AddPlayerOutfitStart(int id)
    {

        StartCoroutine(StartOutfit(id));
    }

    IEnumerator StartOutfit(int id)
    {
        yield return SceneManager.LoadSceneAsync("MainGame");
        NetworkClient.Send(new NetworkManagement.ClientReadyForPlayer());
        NetworkClient.Send(new OutfitAddStart { id = id, info = playerInfo });
    }

    public void AddPlayerOutfit(int id)
    {
        NetworkClient.Send(new OutfitAdd { id = id });
        
    }

    public void OnPackagePacket(NetworkConnection conn, PackagePacket message)
    {
        gameDB.SwitchToRewardCamera();
        gameDB.rewardRoomScript.ResetRoom();
        GameDB.mainUI.bottomPanel.SetActive(false);
        GameDB.mainUI.ChatBox.SetActive(false);
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


    public bool IsAgentOnNavMesh(Vector3 agentObject)
    {
        Vector3 agentPosition = agentObject;
        NavMeshHit hit;

        // Check for nearest point on navmesh to agent, within onMeshThreshold
        if (NavMesh.SamplePosition(agentPosition, out hit, 2f, NavMesh.AllAreas))
        {
            // Check if the positions are vertically aligned
            if (Mathf.Approximately(agentPosition.x, hit.position.x)
                && Mathf.Approximately(agentPosition.z, hit.position.z))
            {
                // Lastly, check if object is below navmesh
                return agentPosition.y >= hit.position.y;
            }
        }

        return false;
    }

    public void OnBoostRequest(NetworkConnection conn, BoostRequest message)
    {
        NavMeshAgent agent = conn.identity.GetComponent<NavMeshAgent>();
        //Debug.Log("Boost request for player " + conn.identity.GetComponent<Player>().name + ", " + message.type + " for " + message.time + "s, power " + message.amount);
        
    }

    public void OnBoostResponse(NetworkConnection conn, BoostResponse message)
    {

    }

    public void OnTimeUpdate(NetworkConnection conn, TimeUpdate time)
    {
        if (NetworkClient.isConnected && NetworkClient.connection.identity != null)
        {
            if (NetworkClient.connection.identity.GetComponent<DayTimeManager>() != null)
            {
                NetworkClient.connection.identity.GetComponent<DayTimeManager>().UpdateLightning(time.time);
            }
        }
    }

    public void OnPlayerInventoryRequest(NetworkConnection conn, PlayerInventoryRequest message)
    {
        DBManager.RefreshPlayerInventory(conn);
    }

    public void OnEquipItem(NetworkConnection conn, EquipItemRequest message)
    {
        EquipItemResponse response = new EquipItemResponse
        {
            
        };
    }


    public void OnDestroyItem(NetworkConnection conn, DestroyItemRequest message)
    {

    }

}
