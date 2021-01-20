using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartPackageScreen : MonoBehaviour
{

    public GameObject Card1, Card2, Card3, Card4, Card5;
    GameDB gameDB;

    private void Start()
    {
        gameDB = FindObjectOfType<GameDB>();
    }


    public void GetRandomPackages()
    {
        List<int> randomOutfits = gameDB.GetRandomOutfits(5);
        Debug.Log("Package rewards: " + randomOutfits.ToString());

        Outfit Card1Outfit = gameDB.GetOutfit(randomOutfits[0]);
        Button card1Button = Card1.GetComponent<Button>();
        Image card1Image = Card1.GetComponent<Image>();
        card1Image.sprite = Card1Outfit.icon;

        Outfit Card2Outfit = gameDB.GetOutfit(randomOutfits[1]);
        Button card2Button = Card2.GetComponent<Button>();
        Image card2Image = Card2.GetComponent<Image>();
        card2Image.sprite = Card2Outfit.icon;

        Outfit Card3Outfit = gameDB.GetOutfit(randomOutfits[2]);
        Button card3Button = Card3.GetComponent<Button>();
        Image card3Image = Card3.GetComponent<Image>();
        card3Image.sprite = Card3Outfit.icon;

        Outfit Card4Outfit = gameDB.GetOutfit(randomOutfits[3]);
        Button card4Button = Card4.GetComponent<Button>();
        Image card4Image = Card4.GetComponent<Image>();
        card4Image.sprite = Card4Outfit.icon;

        Outfit Card5Outfit = gameDB.GetOutfit(randomOutfits[4]);
        Button card5Button = Card5.GetComponent<Button>();
        Image card5Image = Card5.GetComponent<Image>();
        card5Image.sprite = Card5Outfit.icon;


    }

}
