using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{

    public InputField email;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetString("SavedEmail", "") != "")
        {
            email.text = PlayerPrefs.GetString("SavedEmail");
        }
    }

    public void SaveEmail()
    {
        PlayerPrefs.SetString("SavedEmail", email.text);
    }

    
}
