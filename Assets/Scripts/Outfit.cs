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

    public Outfit(int id, string name)
    {
        this.outfitName = name;
        this.id = id;
    }
}
