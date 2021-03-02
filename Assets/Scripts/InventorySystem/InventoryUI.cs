using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System;

public class InventoryUI : MonoBehaviour
{
    public Transform itemContainer;
    public GameObject invItemPrefab;
    public ItemDB itemDB;
    public InfoPanel infoPanel;
    public Sprite selectedImage;
    public Sprite normalImage;
    



    public void RefreshInventory()
    {
        foreach (Transform t in itemContainer)
        {
            Destroy(t.gameObject);
        }
        if (NetworkClient.connection.identity != null && NetworkClient.connection.identity.GetComponent<Player>() != null)
        {
            int slot = 0;
            foreach (int invItem in NetworkClient.connection.identity.GetComponent<Player>().info.inventory)
            {
                GameObject i = Instantiate(invItemPrefab, itemContainer);
                InventorySlot item = i.GetComponent<InventorySlot>();
                item.currentItem = itemDB.itemList[invItem];
                if (invItem > 0)
                {
                    item.itemIcon.sprite = itemDB.itemList[invItem].itemIcon;
                    // Add button event to show item details
                    item.itemButton.onClick.RemoveAllListeners();
                    item.itemButton.onClick.AddListener(() => {
                        ShowItemDesc(invItem);
                        DeselectAll();
                        item.itemFrame.sprite = selectedImage;
                        infoPanel.selectedSlot = slot;
                    });
                } else
                {
                    item.itemButton.enabled = false;
                    
                }
                slot++;
            }
        }
    }

    public void ShowItemDesc(int id)
    {
        infoPanel.PopulateInfo(itemDB.itemList[id]);
    }

    public void DeselectAll()
    {
        InventorySlot[] slots = itemContainer.GetComponentsInChildren<InventorySlot>();
        foreach(InventorySlot slot in slots)
        {
            slot.itemFrame.sprite = normalImage;
        }
    }


}
