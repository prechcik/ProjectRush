using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Outfit : MonoBehaviour
{
    public string outfitName;
    public int id;
    public Sprite icon;
    public int rarity = 0;
    public float lootPercent;

    public Outfit(int id, string name)
    {
        this.id = id;
        this.outfitName = name;
    }

    

}
