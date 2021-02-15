using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardRoomOutfit : MonoBehaviour
{
    public int outfitId;
    public float rotateSpeed = 1f;

    public OutfitDatabase outfitDB;
    private GameDB gameDB;


    private void Start()
    {
        gameDB = FindObjectOfType<GameDB>();
    }

    [ClientCallback]
    private void Update()
    {
        // Outfit rotation
        this.transform.Rotate(0, Time.deltaTime * rotateSpeed, 0, Space.Self);

        if (Input.GetMouseButtonDown(0) && Camera.main.name == "RewardCamera")
        {
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(r, out hit))
            {
                if (hit.transform == this.transform)
                {
                    Debug.Log("Clicked " + this.transform.name + ", adding outfit with id " + this.outfitId);
                    gameDB.network.AddPlayerOutfit(this.outfitId);
                    gameDB.SwitchToMainCamera();
                    gameDB.mainUI.bottomPanel.SetActive(true);
                    gameDB.mainUI.ChatBox.SetActive(true);
                    gameDB.mainUI.dashButton.gameObject.SetActive(true);
                    gameDB.mainUI.expPanelImage.transform.parent.gameObject.SetActive(true);
                }
            }
        }
    }



    public void ClearContainer()
    {
        Transform cont = this.transform;
        foreach(Transform t in cont)
        {
            Destroy(t.gameObject);
        }
    }

    public void InsertOutfit(int id)
    {
        foreach(OutfitDatabase.DBOutfit o in outfitDB.outfitList)
        {
            if (o.outfitId == id)
            {
                this.outfitId = id;
                Instantiate(o.outfitPrefab, this.transform);
            }
        }
    }

}
