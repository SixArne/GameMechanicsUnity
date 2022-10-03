using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SeekBehavior : SteeringBehavior
{
    private NavMeshAgent _agent = null;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        _agent.SetDestination(_target.transform.position);

        if (Vector2.Distance(_target.transform.position, transform.position) <= .2f)
        {
            _agent.isStopped = true;
            Debug.Log("stop");
        }
        else
        {
            _agent.isStopped = false;
            Debug.Log("go");
        }
    }
}
