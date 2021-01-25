using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{

    public GameObject blankObj;
    public GameObject bottomPanel;
    public GameObject expPanelImage;
    public GameObject expPanelText;
    public GameObject packageUI;


    [Header("Chat Objects")]
    public GameObject ChatBox;
    public GameObject messagePrefab;
    public Transform messageContainer;
    public InputField chatInputField;
    public Button chatButton;
    public GameObject chatCloudPrefab;
    // Start is called before the first frame update



    public void HideBlank()
    {
        blankObj.SetActive(false);
        bottomPanel.SetActive(true);
    }

    public void HideAll()
    {
        packageUI.SetActive(false);

    }

    public void ShowPackage()
    {
        HideAll();
        packageUI.SetActive(true);
    }

    public void ToggleChat()
    {
        ChatBox.SetActive(!ChatBox.activeSelf);
    }
}
