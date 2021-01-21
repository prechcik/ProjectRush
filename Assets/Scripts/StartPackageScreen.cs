using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartPackageScreen : MonoBehaviour
{

    public GameObject Card1, Card2, Card3, Card4, Card5;
    public Image Card1OImage, Card2OImage, Card3OImage, Card4OImage, Card5OImage;
    public Image Card1Ring, Card2Ring, Card3Ring, Card4Ring, Card5Ring;
    private GameDB gameDB;
    public float animationSpeed = 2f;
    public GameObject CardPrefab;

    public GameObject PackageScreen, ContentScreen, ContentBG;

    private void Start()
    {
        gameDB = FindObjectOfType<GameDB>();
    }


    public void GetRandomPackages()
    {
        List<int> randomOutfits = gameDB.GetRandomOutfits(5);
        Debug.Log("Package rewards: " + randomOutfits[0] + "," + randomOutfits[1] + "," + randomOutfits[2] + "," + randomOutfits[3] + "," + randomOutfits[4]);

        StartCoroutine(RevealCards(randomOutfits.ToArray()));


    }


    IEnumerator RevealCards(int[] ids)
    {
        PackageScreen.SetActive(false);
        ContentBG.SetActive(true);
        List<Outfit> outfitList = new List<Outfit>();
        for(int i = 0; i < ids.Length; i++)
        {
            outfitList.Add(gameDB.GetOutfit(ids[i]));
            GameObject CardObj = Instantiate(CardPrefab, ContentScreen.transform);
            OutfitCard card = CardObj.GetComponent<OutfitCard>();
            int outfitId = ids[i];
            card.cardButton.onClick.AddListener(delegate { gameDB.DBManager.AddOutfit(outfitId); Debug.Log("Sending outfit id request [" + outfitId + "] for player " + gameDB.DBManager.GetPlayerInfo().nickname); });
            card.cardOutfit.sprite = outfitList[i].icon;
            card.cardRarity.sprite = gameDB.GetRaritySprite(outfitList[i].rarity);
            card.cardAnimator.SetTrigger("Reveal");
            yield return new WaitForSeconds(1f);
        }
        yield return null;

        


    }


    public void ShowPackageScreen()
    {
        UIManager.ShowPackage();
        PackageScreen.SetActive(true);
        ContentBG.SetActive(false);
    }


    

}
