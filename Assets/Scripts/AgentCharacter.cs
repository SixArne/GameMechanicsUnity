using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class AgentCharacter : NPCCharacter
{
    [SerializeField] private float _detectionRadius;
    [SerializeField] private LayerMask _layerMask;

    enum AgentState
    {
        Idle,
        Flee,
        Follow,
        Wander
    }

    [SerializeField] private AgentState _agentState = AgentState.Wander;
    [SerializeField] private float _wanderDistance = 5f;
    [SerializeField] private float _reachedDestination = 5f;
    [SerializeField] private bool _isScared = false;

    private bool _hasReachedDestination = false;
    private Vector3 _wanderDestination = Vector3.zero;

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        CalculateWanderDestination();
    }

    public void Update()
    {
        var player = Physics.OverlapSphere(transform.position, _detectionRadius, _layerMask);
        
        if (player.Length > 0 && _isScared)
        {
            _agentState = AgentState.Flee;
        }
        else if (player.Length > 0)
        {
            _agentState = AgentState.Follow;
        }
        else
        {
            _agentState = AgentState.Wander;
        }

        switch (_agentState)
        {
            case AgentState.Flee:
                Flee(player[0].gameObject.transform.position);

                // Calculate different wander point
                CalculateWanderDestination();
                break;
            case AgentState.Follow:
                Follow(player[0].gameObject.transform.position);
                break;

            case AgentState.Wander:
                Wander();
                break;

        }
    }

    public void Follow(Vector3 position)
    {
        if (Vector3.Distance(position, transform.position) <= _reachedDestination)
        {
            _agent.isStopped = true;
        }
        else
        {
            _agent.isStopped = false;
            _agent.SetDestination(position);
        }
    }

    private void Wander()
    {
        if (_hasReachedDestination)
        {
            _hasReachedDestination = false;
            CalculateWanderDestination();
        }

        if (Vector3.Distance(_wanderDestination, transform.position) <= _reachedDestination)
        {
            _hasReachedDestination = true;
        }

        _agent.SetDestination(_wanderDestination);
    }

    private void CalculateWanderDestination()
    {
        Vector2 randomDirection = Random.insideUnitCircle;
        Vector3 randomWanderLocation = new Vector3(
            transform.position.x + randomDirection.x * _wanderDistance, 
            transform.position.y,
            transform.position.z + randomDirection.y * _wanderDistance
            );

        NavMeshHit navMeshHit;
        if (NavMesh.SamplePosition(randomWanderLocation, out navMeshHit, 100f, NavMesh.AllAreas))
        {
            _wanderDestination = navMeshHit.position;
        }
    }

    private void Flee(Vector3 position)
    {
        Vector3 transformPosition = position - transform.position;
        _agent.SetDestination(transform.position - transformPosition);
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);

        Gizmos.color = Color.magenta.WithAlpha(0.4f);
        Gizmos.DrawSphere(transform.position, _reachedDestination);

        Gizmos.DrawSphere(_wanderDestination, 2f);
    }
}
