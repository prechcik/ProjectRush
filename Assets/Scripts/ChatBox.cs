using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatBox : MonoBehaviour
{

    

    public string playerName = "";

    private static MainUI ui;

    public struct ChatMsg
    {
        public string sender;
        public string message;
    }

    // Start is called before the first frame update
    void Start()
    {
        ui = FindObjectOfType<MainUI>();
        ui.chatButton.onClick.AddListener(delegate { this.gameObject.GetComponent<Player>().Send(); ui.chatInputField.text = ""; });
    }

    private void Update()
    {
        if (playerName == "")
        {
            playerName = this.gameObject.GetComponent<Player>().nickname;
        }
    }



    public static void HandleNewMessage(ChatMsg msg)
    {
        if (ui.messageContainer.childCount > 6)
        {
            Destroy(ui.messageContainer.GetChild(0).gameObject);
        }
        GameObject message = Instantiate(ui.messagePrefab, ui.messageContainer);
        ChatMessage sc = message.GetComponent<ChatMessage>();
        sc.SenderText.text = msg.sender + ": ";
        sc.MessageText.text = msg.message;
    }

    
}
