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
    public ParticleSystem levelUpParticle;
    public ParticleSystem levelUpParticle2;
    public TextMesh usernameMesh;
    public TextMesh levelMesh;
    public int activeOutfit = 0;
    public string userId;
    [SyncVar]
    public Quaternion playerRotation;
    [SyncVar]
    public Vector3 playerPosition;
    public NavMeshAgent agent;
    [SyncVar]
    public List<int> outfits;

    [SyncVar]
    public int currentExp;
    [SyncVar]
    public int currentLevel;
    public int level;

    public ParticleSystem boostParticles;
    

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

    public Vector3 lastLocalPos;

    private MainUI ui;

    private PlayerMovement movementScript;

    [SyncVar]
    public bool spawnedAndReady = false;

    private GameObject destinationMarker;

    public OutfitDatabase oDB;

    [SyncVar]
    public float playerSpeed = 2f;

    private float dashCounter;

    public DayTimeManager timeManager;
    public Light flashLightObject;

    [SyncVar]
    public bool inCombat = false;
    [SyncVar]
    public float combatTimer = 0f;

    public Transform HpBarParent;
    public RectTransform HpBackground;
    public Image HealthImage;
    public Text HealthText;

    [SyncVar]
    public float currentHealth;
    public float hpPercent;



    private void Awake()
    {
        NavMeshHit hit;
        // Check for nearest point on navmesh to agent, within onMeshThreshold
        if (NavMesh.SamplePosition(this.transform.position, out hit, Mathf.Infinity, NavMesh.AllAreas))
        {
            if (hit.hit)
            {
                this.transform.position = hit.position;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        gameDB = FindObjectOfType<GameDB>();
        agent = this.GetComponent<NavMeshAgent>();
        ui = FindObjectOfType<MainUI>();
        timeManager = this.GetComponent<DayTimeManager>();
        if (isLocalPlayer)
        {
            Camera.main.GetComponent<CameraMovement>().target = this.transform;
            mainUI = FindObjectOfType<MainUI>();
            mainUI.HideBlank();
            movementScript = this.GetComponent<PlayerMovement>();
            mainUI.dashButton.onClick.AddListener(delegate
            {
                float newSpeed = this.playerSpeed * 3f;
                SetBoostSpeed(newSpeed, 1f);
                mainUI.dashButton.enabled = false;
                dashCounter = 0f;
                Invoke(nameof(ResetDash), 5f);
            });
        }
        spawnedAndReady = false;
        lastPos = new Vector3(info.x,info.y,info.z);
        agent.enabled = true;
        spawnedAndReady = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameDB == null)
        {
            gameDB = FindObjectOfType<GameDB>();
        }
        if (playerAnimator == null)
        {
            playerAnimator = this.GetComponentInChildren<Animator>();
        }
        if (usernameMesh.text != nickname) usernameMesh.text = nickname;
        levelMesh.text = "Level " + currentLevel;
        if (currentHealth > 0)
        {
            hpPercent = (float)(currentHealth / info.maxHealth);
            if (playerAnimator != null) playerAnimator.SetBool("Alive", true);
        } else
        {
            hpPercent = 0;
            if (playerAnimator != null) playerAnimator.SetBool("Alive", false);
        }
        HpBackground.transform.position = Camera.main.WorldToScreenPoint(HpBarParent.transform.position);
        HealthImage.fillAmount = hpPercent;
        HealthText.text = currentHealth + "/" + info.maxHealth;

        if (activeOutfit != currentOutfit)
        {
            ChangeOutfit(currentOutfit);
            
        }
        if (isLocalPlayer && isClient)
        {
            UpdateRot(transform.rotation);
            UpdatePos(transform.position);
            currentLevel = CalculateLevel(currentExp);
            if (currentLevel > level)
            {
                if (level > 0)
                {
                    levelUpParticle.Play();
                    levelUpParticle2.Play();
                    ShowRewardIcon();
                }
                HandleLevel(currentLevel);
                level = currentLevel;
            }
            
            if (spawnedAndReady)
            {
                HandleExperience();
            }
            UpdateExpBar();
            if (destinationMarker == null)
            {
                destinationMarker = Instantiate(gameDB.destinationMarker);
                destinationMarker.SetActive(false);
            }
            if (agent.hasPath && destinationMarker != null)
            {
                destinationMarker.SetActive(true);
                destinationMarker.transform.position = agent.destination;

            }
            else
            {
                destinationMarker.SetActive(false);
            }

            if (!mainUI.dashButton.enabled)
            {
                mainUI.dashButton.image.fillAmount = dashCounter / 5f;
                dashCounter += Time.deltaTime;
            } else
            {
                mainUI.dashButton.image.fillAmount = 1f;
            }
        }
        else if (!isLocalPlayer && isClient)
        {
            float tempSpeed = Mathf.Clamp(Time.deltaTime * agent.speed, 0f, 0.99f);
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
        if (agent.speed != playerSpeed)
        {
            agent.speed = playerSpeed;
        }
        if (playerSpeed > 2f)
        {
            if (!boostParticles.isPlaying)
            {
                boostParticles.Play();
            }
        } else
        {
            boostParticles.Stop();
        }
        if (timeManager.TimeOfDay < 6f || timeManager.TimeOfDay > 18f)
        {
            flashLightObject.enabled = true;
        } else
        {
            flashLightObject.enabled = false;
        }
        combatTimer -= Time.deltaTime;
        if (combatTimer <= 0)
        {
            inCombat = false;
        } else
        {
            inCombat = true;
        }
        if (inCombat)
        {
            HpBackground.gameObject.SetActive(true);
        } else
        {
            HpBackground.gameObject.SetActive(false);
        }


    }

    public void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.GetComponent<TemporaryBoost>() != null)
        {
            
            TemporaryBoost boost = collision.gameObject.GetComponent<TemporaryBoost>();
            //Debug.Log("Player " + info.nickname + " touched the boost " + boost.type + " * " + boost.time + "s");
            NetworkClient.Send(new NetworkManagement.BoostRequest
            {
                type = boost.type,
                amount = boost.amount,
                time = boost.time
            });
            SetBoostSpeed(agent.speed * boost.amount, boost.time);
            DestroyBoost(collision.gameObject);
        }
    }

    [Command]
    public void DestroyBoost(GameObject obj)
    {
        Destroy(obj);
    }

    public void ChangeOutfit(int newOufit)
    {
        foreach(Transform t in netIdentity.GetComponent<Player>().modelContainer.transform)
        {
            Destroy(t.gameObject);
        }
        foreach (OutfitDatabase.DBOutfit o in oDB.outfitList) {
            Outfit ou = o.outfitPrefab.GetComponent<Outfit>();
            if (ou != null && ou.id == newOufit)
            {
                GameObject playerModel = Instantiate(o.outfitPrefab, o.outfitPrefab.transform.localPosition, netIdentity.GetComponent<Player>().transform.rotation, netIdentity.GetComponent<Player>().modelContainer.transform);
                playerModel.transform.localPosition = o.outfitPrefab.transform.localPosition;
                activeOutfit = currentOutfit;
                playerAnimator = playerModel.GetComponent<Animator>();
            }
        }
    }

    [Command]
    public void UpdateRot(Quaternion rot)
    {
        this.playerRotation = rot;
        connectionToClient.identity.GetComponent<Player>().playerRotation = rot;
    }

    [Command]
    public void UpdatePos(Vector3 pos)
    {
        this.playerPosition = pos;
        connectionToClient.identity.GetComponent<Player>().playerPosition = pos;
    }

    [Command]
    public void HandleLevel(int level)
    {
        this.info.level = level;
        connectionToClient.identity.GetComponent<Player>().info = this.info;
        this.level = level;
        //Debug.LogFormat("[Server] Player [{0}] has leveled up to level {1}", this.nickname, this.level);
        gameDB.network.OnPlayerUpdateRequest(connectionToClient, new NetworkManagement.PlayerUpdateRequest { });
    }

    [Command]
    public void ShowPackageScreen()
    {
        
        GameDB.mainUI.RewardIcon.SetActive(false);
    }

    [Command]
    public void SetBoostSpeed(float newSpeed, float time)
    {
        this.playerSpeed = newSpeed;
        this.agent.speed = newSpeed;
        Invoke(nameof(ResetSpeed), time);

    }

    public void ResetSpeed()
    {
        this.agent.speed = 2f;
        this.playerSpeed = 2f;
    }

    public void ToggleSpeedBoostParticles(bool play)
    {
        if (!play)
        {
            this.boostParticles.Stop();
        }
        else
        {
            this.boostParticles.Play();
        }
    }

    public void ShowRewardIcon()
    {
        GameDB.mainUI.RewardIcon.SetActive(true);
    }

    

    [Command]
    public void HandleExperience()
    {
        distanceTravelled += (Vector3.Distance(lastPos, playerPosition) * gameDB.expRate );
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
        Collider[] nearbyPlayers = Physics.OverlapSphere(netIdentity.transform.position, gameDB.chatProximity);
        List<NetworkIdentity> playersSent = new List<NetworkIdentity>();
        playersSent.Add(netIdentity);
        NetworkIdentity[] idents = FindObjectsOfType<NetworkIdentity>();
        foreach (NetworkIdentity i in idents)
        {
            if (Vector3.Distance(i.transform.position, this.netIdentity.transform.position) < gameDB.chatProximity)
            {
                if (i.GetComponent<Player>() != null)
                {
                    i.GetComponent<Player>().CmdSendMessage(new ChatBox.ChatMsg { sender = i.GetComponent<Player>().nickname, message = i.GetComponent<Player>().ui.chatInputField.text });
                }
            }
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


    public void ResetDash()
    {
        mainUI.dashButton.enabled = true;
    }

    [Command]
    public void ReceiveDamage(int damage)
    {
        //Debug.Log(nickname + " received " + damage + " damage");
        if (!inCombat) inCombat = true; 
        if (currentHealth >= damage)
        {
            currentHealth -= damage;
        }
        else
        {
            currentHealth = 0;
            // DIE!
        }
        combatTimer = 5f;
    }


    [Command]
    public void ToggleCombat(bool a)
    {
        inCombat = a;
    }


}
