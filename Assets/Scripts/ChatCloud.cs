using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatCloud : MonoBehaviour
{

    public Text cloudText;
    public Transform target;

    public void Start()
    {
        Invoke("AutoDestroy", 5f);
    }

    private void Update()
    {
        this.transform.position = Camera.main.WorldToScreenPoint(target.position) + new Vector3(0,00f,0);
    }

    public void AutoDestroy()
    {
        Destroy(this.gameObject);
    }

}
