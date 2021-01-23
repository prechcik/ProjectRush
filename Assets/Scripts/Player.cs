using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

    private GameDB gameDB;
    private MainUI mainUI;

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
        }
        else
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, playerRotation, Time.deltaTime * agent.angularSpeed);
            transform.position = Vector3.Lerp(transform.position, playerPosition, Time.deltaTime * agent.speed);
        }
    }

    public void ChangeOutfit(int newOufit)
    {
        foreach(Transform t in modelContainer.transform)
        {
            Destroy(t.gameObject);
        }
        foreach (GameObject o in gameDB.outfitList) {
            Outfit ou = o.GetComponent<Outfit>();
            if (ou != null && ou.id == newOufit)
            {
                GameObject playerModel = Instantiate(o, Vector3.zero, transform.rotation, modelContainer.transform);
                playerModel.transform.localPosition = Vector3.zero;
                activeOutfit = currentOutfit;
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
}
