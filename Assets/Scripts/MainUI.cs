using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUI : MonoBehaviour
{

    public GameObject blankObj;
    public GameObject bottomPanel;
    public GameObject expPanelImage;
    public GameObject expPanelText;
    // Start is called before the first frame update
    


    public void HideBlank()
    {
        blankObj.SetActive(false);
        bottomPanel.SetActive(true);
    }
}
