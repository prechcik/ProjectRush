using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    [SyncVar]
    public string nickname;
    [SyncVar]
    public int currentOutfit;
    [SyncVar]
    public PlayerInfo info;
    public GameObject modelContainer;
    public int activeOutfit = 0;
    public string userId;
    [SyncVar]
    public Quaternion playerRotation;
    [SyncVar]
    public Vector3 playerPosition;
    private NavMeshAgent agent;
    [SyncVar]
    public List<int> outfits;

    [SyncVar]
    public int currentExp;
    [SyncVar]
    public int currentLevel;

    private GameDB gameDB;
    private MainUI mainUI;

    [SyncVar]
    public Vector3 lastPos;
    [SyncVar]
    public float distanceTravelled = 0f;
    public int expLeft;
    public int expNext, currentPercentInt;
    public float currentPercent;
    public float speed = 0f;
    public float speedMagnitude;

    public Animator playerAnimator;

    private Vector3 lastLocalPos;

    private MainUI ui;

    

    // Start is called before the first frame update
    void Start()
    {
        if (isLocalPlayer)
        {
            Camera.main.GetComponent<CameraMovement>().target = this.transform;
            mainUI = FindObjectOfType<MainUI>();
            mainUI.HideBlank();
        }
        gameDB = FindObjectOfType<GameDB>();
        agent = this.GetComponent<NavMeshAgent>();
        currentExp = info.experience;
        currentLevel = info.level;
        lastPos = new Vector3(info.x,info.y,info.z);
        ui = FindObjectOfType<MainUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (this.GetComponentInChildren<TextMesh>().text != nickname) this.GetComponentInChildren<TextMesh>().text = nickname;
        if (activeOutfit != currentOutfit)
        {
            ChangeOutfit(currentOutfit);
            
        }
        if (isLocalPlayer)
        {
            UpdateRot(transform.rotation);
            UpdatePos(transform.position);
            HandleLevel();
            HandleExperience();
            UpdateExpBar();
        }
        else
        {
            float tempSpeed = Mathf.Clamp(Time.deltaTime * agent.angularSpeed, 0f, 0.99f);
            transform.rotation = Quaternion.Lerp(transform.rotation, playerRotation, tempSpeed);
            transform.position = Vector3.Lerp(transform.position, playerPosition, Time.deltaTime * agent.speed);
        }
        speedMagnitude = (float)(this.transform.position - lastLocalPos).magnitude;
        speed = (float)((speedMagnitude / Time.deltaTime) / this.agent.speed);
        if (playerAnimator != null)
        {
            playerAnimator.SetFloat("speed", this.speed);
        }
        lastLocalPos = this.transform.position;
    }

    public void ChangeOutfit(int newOufit)
    {
        foreach(Transform t in netIdentity.GetComponent<Player>().modelContainer.transform)
        {
            Destroy(t.gameObject);
        }
        foreach (GameObject o in gameDB.outfitList) {
            Outfit ou = o.GetComponent<Outfit>();
            if (ou != null && ou.id == newOufit)
            {
                GameObject playerModel = Instantiate(o, o.transform.localPosition, netIdentity.GetComponent<Player>().transform.rotation, netIdentity.GetComponent<Player>().modelContainer.transform);
                playerModel.transform.localPosition = o.transform.localPosition;
                netIdentity.GetComponent<Player>().activeOutfit = currentOutfit;
                netIdentity.GetComponent<Player>().playerAnimator = playerModel.GetComponent<Animator>();
            }
        }
    }

    [Command]
    public void UpdateRot(Quaternion rot)
    {
        playerRotation = rot;
    }

    [Command]
    public void UpdatePos(Vector3 pos)
    {
        playerPosition = pos;
    }

    [Command]
    public void HandleLevel()
    {
        currentLevel = CalculateLevel(currentExp);
        if (currentLevel != info.level)
        {
            Debug.Log("Updated player level to " + currentLevel + ", experience: " + currentExp);
            info.level = currentLevel;
            gameDB.DBManager.UpdatePlayerInfo(connectionToClient.identity.GetComponent<Player>());
            connectionToClient.Send(new NetworkManagement.PackagePacket { });
        }
    }

    

    [Command]
    public void HandleExperience()
    {
        distanceTravelled += Vector3.Distance(lastPos, playerPosition);
        lastPos = playerPosition;
        if (distanceTravelled > 1f)
        {
            currentExp += Mathf.FloorToInt(distanceTravelled);
            info.experience = currentExp;
            distanceTravelled = 0f;
        }
    }

    public int CalculateLevel(int experience)
    {
        return Mathf.FloorToInt((1 + Mathf.Sqrt(1 + 8 * experience / 50)) / 2);
    }

    public int CalculateExpForLevel(int level)
    {
        int expForLevel = (int)(((Mathf.Pow(level, 2) - level) * 50f) / 2);
        return expForLevel;
    }

  


    public void UpdateExpBar()
    {
        int nextLevelExp = CalculateExpForLevel(CalculateLevel(this.currentExp) + 1);
        int thisLevelExp = CalculateExpForLevel(CalculateLevel(this.currentExp));
        int difference = nextLevelExp - thisLevelExp;
        int remaining = nextLevelExp - this.currentExp;
        expNext = nextLevelExp;
        expLeft = thisLevelExp;
        float percent = (float)(difference - remaining) / (float)difference;
        currentPercent = percent;
        float lerpPercent = Mathf.Lerp(mainUI.expPanelImage.GetComponent<Image>().fillAmount, percent, Time.deltaTime * 5f);
        mainUI.expPanelImage.GetComponent<Image>().fillAmount = lerpPercent;
        mainUI.expPanelText.GetComponent<Text>().text = lerpPercent.ToString("0.00%");
    }


    [Client]
    public void Send()
    {
        if (string.IsNullOrWhiteSpace(netIdentity.GetComponent<Player>().ui.chatInputField.text)) { return; }
        if (netIdentity.GetComponent<Player>().ui.chatInputField.text.Length > 50)
        {
            netIdentity.GetComponent<Player>().ui.chatInputField.text = netIdentity.GetComponent<Player>().ui.chatInputField.text.Substring(0, 50);
        }
        SendToNearby();
    }

    public void SendToNearby()
    {
        Collider[] nearbyPlayers = Physics.OverlapSphere(netIdentity.transform.position, 3f);
        List<NetworkIdentity> playersSent = new List<NetworkIdentity>();
        playersSent.Add(netIdentity);
        foreach (Collider hitPlayer in nearbyPlayers)
        {
                //Debug.Log("Found player " + hitPlayer.GetComponent<Player>().nickname + ", MyIdentity: " + netIdentity.GetComponent<Player>().nickname);
                if (hitPlayer.GetComponent<NetworkIdentity>() != null && !hitPlayer.GetComponent<NetworkIdentity>() != netIdentity)
                {
                    if (!playersSent.Contains(hitPlayer.GetComponent<NetworkIdentity>()))
                    {
                        netIdentity.GetComponent<Player>().CmdSendMessage(new ChatBox.ChatMsg { sender = netIdentity.GetComponent<Player>().nickname, message = netIdentity.GetComponent<Player>().ui.chatInputField.text });
                        playersSent.Add(hitPlayer.GetComponent<NetworkIdentity>());
                    }
                }
        }
        if (playersSent.Count < 2)
        {
            netIdentity.GetComponent<ChatBox>().HandleNewMessage(new ChatBox.ChatMsg { sender = netIdentity.GetComponent<Player>().nickname, message = netIdentity.GetComponent<Player>().ui.chatInputField.text });

        }
    }

    [Command]
    private void CmdSendMessage(ChatBox.ChatMsg msg)
    {
        RpcHandleMessage(msg);
    }

    [ClientRpc]
    private void RpcHandleMessage(ChatBox.ChatMsg msg)
    {
        netIdentity.GetComponent<ChatBox>().HandleNewMessage(msg);
    }




}
