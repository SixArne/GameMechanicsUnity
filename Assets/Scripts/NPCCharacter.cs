using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCCharacter : MonoBehaviour
{
    protected MovementBehavior _movementBehavior;
    protected NavMeshAgent _agent;

    protected virtual void Awake()
    {
        _movementBehavior = GetComponent<MovementBehavior>();
        _agent = GetComponent<NavMeshAgent>();
    }
}
