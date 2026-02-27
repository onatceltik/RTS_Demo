using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NMUnitMovement : MonoBehaviour
{
    private NavMeshAgent unitNMAgent;

    void Awake()
    {
        unitNMAgent = GetComponent<NavMeshAgent>();
        unitNMAgent.updateRotation = false; // maintain 2d
        unitNMAgent.updateUpAxis = false;   // maintain 2d
    }

    public void moveToPosition(Vector3 mousePosition)
    {
        unitNMAgent.SetDestination(mousePosition);
    }
}
