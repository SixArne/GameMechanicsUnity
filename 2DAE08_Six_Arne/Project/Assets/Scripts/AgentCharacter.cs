using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class AgentCharacter : BasicNavMeshAgent
{
    [SerializeField] private float _wanderDistance = 5f;
    [SerializeField] private float _reachedDestination = 5f;

    private bool _hasReachedDestination = false;
    private Vector3 _wanderDestination = Vector3.zero;
    private GameObject _player;

    private bool _isFollowing = false;

    private AwarenessBehavior _awarenessBehavior = null;

    protected override void Awake()
    {
        base.Awake();

        _player = GameObject.FindGameObjectWithTag("Friendly");
        _awarenessBehavior = GetComponent<AwarenessBehavior>();
    }

    public bool IsFollowing
    {
        get => _isFollowing;
        set
        {
            _isFollowing = value;
            _awarenessBehavior.CanFollow = !value;
        }
    }

    void Start()
    {
        CalculateWanderDestination();
    }

    public void Update()
    {
        if (_isFollowing)
        {
            Target = _player.transform.position;
            Seek();

            return;
        }
        
        Wander();
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

        _target = _wanderDestination;
        base.Seek();
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

    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 1, 0.4f);
        Gizmos.DrawSphere(transform.position, _reachedDestination);

        Gizmos.DrawSphere(_wanderDestination, 2f);
    }
}
