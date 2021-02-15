using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class PlayerMovement : NetworkBehaviour
{
    private NavMeshAgent agent;
    public LayerMask mask;

    public bool walking;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasAuthority) { return; }
        if (Input.GetMouseButtonDown(0) && Camera.main.name == "Main Camera")
        {
            int touchIndex = -1;
            if (Input.touchCount > 0)
            {
                touchIndex = Input.GetTouch(0).fingerId;
            } else
            {
                touchIndex = -1;
            }
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, mask))
            {
                if (EventSystem.current.IsPointerOverGameObject(touchIndex)) {
                    if (!hit.collider.CompareTag("UIClickableThrough"))
                    {
                        return;
                    }
                }
                walking = true;
                agent.SetDestination(hit.point);
            }
        }
        if (agent.hasPath && agent.remainingDistance < 0.1f)
        {
            walking = false;
            agent.ResetPath();
        }
    }
}
