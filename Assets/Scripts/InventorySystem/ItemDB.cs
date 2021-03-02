using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryDB", menuName = "Scriptables/Inventory DB")]
public class ItemDB : ScriptableObject
{
    public List<InventoryItem> itemList = new List<InventoryItem>();
}
