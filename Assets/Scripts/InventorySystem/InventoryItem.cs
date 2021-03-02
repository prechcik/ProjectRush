using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class InventoryItem
{
    public int id;
    public string itemName;
    public string desc;
    public int damage;
    public Sprite itemIcon;
    public GameObject prefab;
    public int rarity;
}
