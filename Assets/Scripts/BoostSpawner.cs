using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BoostSpawner : MonoBehaviour
{

    public List<GameObject> boostList = new List<GameObject>();

    public int maxBoostAmount;
    public Transform boostContainer;

    public BoxCollider spawnableArea;

    public float spawnRate = 5f;
    private float timer = 0f;


    
    void Start()
    { 
        foreach(GameObject boostPrefab in boostList)
        {
            ClientScene.RegisterPrefab(boostPrefab);
        }
    }

    

    [ServerCallback]
    void Update()
    {
        if (boostContainer.childCount < maxBoostAmount)
        {
            if (timer >= spawnRate)
            {
                Vector3 newRawPos = GameDB.RandomPointInBounds(spawnableArea.bounds);
                NavMeshHit hit;
                Vector3 newMeshPos = newRawPos;
                int random = Random.Range(0, boostList.Count);

                if (NavMesh.SamplePosition(newMeshPos, out hit, Mathf.Infinity, -1))
                {
                    newMeshPos = hit.position;
                    GameObject boostObj = Instantiate(boostList[random], newMeshPos, Quaternion.identity, this.transform);
                    NetworkServer.Spawn(boostObj);
                    timer = 0f;
                }
            }
            timer += Time.deltaTime;
        }
    }
}
