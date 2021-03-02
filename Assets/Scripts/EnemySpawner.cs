using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{

    public List<GameObject> enemyList = new List<GameObject>();
    public BoxCollider spawnableArea;
    public int maxEnemies;
    public int spawnedEnemies = 0;

    
    public void Spawn()
    {
        for(int i = 0; i < maxEnemies; i++)
        {
            Vector3 newRawPos = GameDB.RandomPointInBounds(spawnableArea.bounds);
            NavMeshHit hit;
            Vector3 newMeshPos = newRawPos;
            int r = Random.Range(0, enemyList.Count);
            if (NavMesh.SamplePosition(newMeshPos, out hit, Mathf.Infinity, -1))
            {
                newMeshPos = hit.position;
                GameObject enemyObj = Instantiate(enemyList[r], newMeshPos, Quaternion.Euler(0,Random.Range(0,360), 0), this.transform);
                NetworkServer.Spawn(enemyObj);
                spawnedEnemies++;
            }
        }
        Debug.Log("Spawned " + spawnedEnemies + " monsters");
    }


}
