using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class TemporaryBoost : NetworkBehaviour
{
    public string type;
    public float amount;
    public float time = 10f;
    public float aliveTime = 20f;
    public Vector3 startPos;
    public Vector3 startPosHigh;
    public float posBounce;
    public float floatSpeed = 1f;
    private bool bounce = false;

    public BoxCollider col;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("DestroyRoutine", aliveTime);
        startPos = this.transform.localPosition;
        startPos.y += 1f;
        startPosHigh = new Vector3(startPos.x, startPos.y + posBounce, startPos.z);
    }

    private void Update()
    {
        if (!bounce)
        {
            float dist = Vector3.Distance(this.transform.localPosition, startPosHigh);
            if (dist < 0.01f)
            {
                bounce = true;
            }
            this.transform.localPosition = Vector3.Lerp(this.transform.position, startPosHigh, Time.deltaTime * floatSpeed);
        } else
        {
            float dist = Vector3.Distance(this.transform.localPosition, startPos);
            if (dist < 0.01f)
            {
                bounce = false;
            }
            this.transform.localPosition = Vector3.Lerp(this.transform.position, startPos, Time.deltaTime * floatSpeed);
        }
    }

    
    void DestroyRoutine()
    {
        Destroy(this.gameObject);
    }

    
    
}
