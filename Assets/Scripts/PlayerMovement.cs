using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : NetworkBehaviour
{
    private NavMeshAgent agent;

    // Start is called before the first frame update
    [Client]
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    [Client]
    void Update()
    {
        //if (!hasAuthority) { return; }
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Ended)
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(t.position), out hit, 100))
                {
                    agent.SetDestination(hit.point);
                }
            }
        }
    }
}
