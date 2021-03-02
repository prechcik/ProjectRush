using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class InfoPanel : MonoBehaviour
{
    public Text itemName;
    public Image itemImage;
    public Text itemDescription;
    public Text itemAttack;
    public Button equipBtn;
    public Button destroyBtn;

    public int selectedSlot;


    public void PopulateInfo(InventoryItem item)
    {
        if (item.id > 0)
        {
            itemName.text = item.itemName;
            itemAttack.text = "Attack: " + item.damage;
            itemImage.sprite = item.itemIcon;
            itemDescription.text = item.desc;
            equipBtn.onClick.AddListener(() =>
            {
                // Send message to server to equip item and update inventory
                NetworkClient.Send(new NetworkManagement.EquipItemRequest
                {
                    itemId = item.id
                });
            });
            destroyBtn.onClick.AddListener(() =>
            {
                // Send message to server to destroy item and update inventory
                NetworkClient.Send(new NetworkManagement.DestroyItemRequest
                {
                    slotId = selectedSlot
                });
            });
        }
        else
        {
            itemName.text = "";
            itemAttack.text = "";
            itemImage.sprite = item.itemIcon;
            itemDescription.text = "";
            equipBtn.enabled = false;
            destroyBtn.enabled = false;
        }
    }
}
