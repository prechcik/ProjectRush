using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SyncVar]
    public string nickname;
    [SyncVar]
    public int currentOutfit;
    public PlayerInfo info;
    public GameObject modelContainer;
    public int activeOutfit = 0;
    public string userId;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (this.GetComponentInChildren<TextMesh>().text != nickname) this.GetComponentInChildren<TextMesh>().text = nickname;
        if (activeOutfit != currentOutfit)
        {
            ChangeOutfit(currentOutfit);
            
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
}
