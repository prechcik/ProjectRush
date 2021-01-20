using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    public GameObject LoginScreen;
    public GameObject RegisterScreen;
    public GameObject NicknameScreen;
    public GameObject packageScreen;

    public static UIManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else if (instance != null)
        {
            Destroy(this);
        }
    }

    public static void HideAll()
    {
        instance.LoginScreen.SetActive(false);
        instance.RegisterScreen.SetActive(false);
        instance.NicknameScreen.SetActive(false);
        instance.packageScreen.SetActive(false);
    }

    public static void ShowLogin()
    {
        HideAll();
        instance.LoginScreen.SetActive(true);
    }

    public static void ShowRegister()
    {
        HideAll();
        instance.RegisterScreen.SetActive(true);
    }

    public static void ShowNick()
    {
        HideAll();
        instance.NicknameScreen.SetActive(true);
    }

    public static void ShowPackage()
    {
        HideAll();
        instance.packageScreen.SetActive(true);
    }


}
