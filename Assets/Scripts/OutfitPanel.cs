using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class OutfitPanel : MonoBehaviour
{

    public GameObject iconContainer;
    public GameObject iconPrefab;
    public GameObject CardPrefab;

    private FirebaseManager DBManager;
    private NetworkManagement network;
    private GameDB gameDB;


    private void Start()
    {
        DBManager = FindObjectOfType<FirebaseManager>();
        network = FindObjectOfType<NetworkManagement>();
        gameDB = FindObjectOfType<GameDB>();
    }





    public void OnEnable()
    {
        NetworkManagement.PlayerOutfitRequest req = new NetworkManagement.PlayerOutfitRequest();
        NetworkClient.Send(req);
    }

    public void PopulateIcons(List<int> newids)
    {
        foreach(Transform t in iconContainer.transform)
        {
            Destroy(t.gameObject);
        }
        foreach(int id in newids)
        {
            if (id != 0)
            {
                Outfit newOutfit = gameDB.GetOutfit(id);
                GameObject CardObj = Instantiate(CardPrefab, iconContainer.transform);
                CardObj.GetComponent<Image>().fillAmount = 1f;
                Destroy(CardObj.GetComponent<Animator>());
                OutfitCard card = CardObj.GetComponent<OutfitCard>();
                card.cardButton.onClick.RemoveAllListeners();
                card.cardButton.onClick.AddListener(delegate { card.cardButton.enabled = false; NetworkClient.Send(new NetworkManagement.OutfitRequest { outfitid = id }); Debug.Log("Sending outfit change request [" + id + "] for player " + NetworkClient.connection.identity.GetComponent<Player>().nickname); TogglePanel(); });
                card.cardOutfit.sprite = newOutfit.icon;
                
                card.cardRarity.sprite = gameDB.GetRaritySprite(newOutfit.rarity);
                float outfitRarity = newOutfit.rarity * 1f;
                float totalRarities = gameDB.packageRarities.Count * 1f;
                float rarityPercent = outfitRarity / totalRarities;
                card.cardRarity.color = gameDB.rarityColors.Evaluate(rarityPercent);
                card.cardButton.enabled = true;
            }

        }
    }


    public void TogglePanel()
    {
        if (this.gameObject.activeSelf)
        {

            this.gameObject.SetActive(false);
        } else
        {
            this.gameObject.SetActive(true);
        }
    }
}
