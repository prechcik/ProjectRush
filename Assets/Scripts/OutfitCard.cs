using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutfitCard : MonoBehaviour
{

    public Button cardButton;
    public Image cardOutfit;
    public Image cardRarity;
    public Animator cardAnimator;

    public OutfitCard(Button btn, Image outfit, Image rarity) {
        this.cardButton = btn;
        this.cardOutfit = outfit;
        this.cardRarity = rarity;
    }
}
