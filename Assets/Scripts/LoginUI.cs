using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{

    public InputField email;

    private NetworkManagement network;
    private FirebaseManager DBManager;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetString("SavedEmail", "") != "")
        {
            email.text = PlayerPrefs.GetString("SavedEmail");
        }
        network = FindObjectOfType<NetworkManagement>();
        DBManager = FindObjectOfType<FirebaseManager>();
    }

    public void SaveEmail()
    {
        PlayerPrefs.SetString("SavedEmail", email.text);
    }

    public void SendLogin()
    {
        network.LoginBtn();
    }

    
}
