using Mirror;
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
    public Image progressImage;
    public GameObject RewardIcon;
    public Button RewardIconButton;
    public GameDB gameDB;
    public Button dashButton;


    [Header("Chat Objects")]
    public GameObject ChatBox;
    public GameObject messagePrefab;
    public Transform messageContainer;
    public InputField chatInputField;
    public Button chatButton;
    public GameObject chatCloudPrefab;
    // Start is called before the first frame update

    public void Start()
    {
        gameDB = FindObjectOfType<GameDB>();
    }


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


    public void RewardButton()
    {
        Debug.Log("Clicked UI Package");
        gameDB.SwitchToRewardCamera();
        gameDB.rewardRoomScript.ResetRoom();
        gameDB.mainUI.bottomPanel.SetActive(false);
        gameDB.mainUI.ChatBox.SetActive(false);
        gameDB.mainUI.dashButton.gameObject.SetActive(false);
        gameDB.mainUI.expPanelImage.transform.parent.gameObject.SetActive(false);
        RewardIcon.SetActive(false);
    }
}
