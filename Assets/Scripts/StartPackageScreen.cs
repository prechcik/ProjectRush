using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        
        StartCoroutine(RevealCards(randomOutfits.ToArray()));


    }



    IEnumerator RevealCards(int[] ids)
    {
        PackageScreen.SetActive(false);
        ContentBG.SetActive(true);
        List<Outfit> outfitList = new List<Outfit>();
        for (int i = 0; i < ids.Length; i++)
        {
            outfitList.Add(gameDB.GetOutfit(ids[i]));
            GameObject CardObj = Instantiate(CardPrefab, ContentScreen.transform);
            OutfitCard card = CardObj.GetComponent<OutfitCard>();
            int outfitId = ids[i];
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                card.cardButton.onClick.AddListener(delegate { gameDB.network.AddPlayerOutfitStart(outfitId); Debug.Log("Sending outfit id request [" + outfitId + "] for player "); });
            } else if (SceneManager.GetActiveScene().name == "MainGame")
            {
                card.cardButton.onClick.AddListener(delegate {  gameDB.network.AddPlayerOutfit(outfitId); Debug.Log("Sending outfit id request [" + outfitId + "] for player "); this.gameObject.SetActive(false); });
            }
            card.cardOutfit.sprite = outfitList[i].icon;
            card.cardRarity.sprite = gameDB.GetRaritySprite(outfitList[i].rarity);
            card.cardAnimator.SetTrigger("Reveal");
            yield return new WaitForSeconds(0.2f);
        }
        yield return null;

        


    }




    

}
