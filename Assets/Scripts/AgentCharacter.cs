using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class AgentCharacter : BasicNavMeshAgent
{
    [SerializeField] private float _wanderDistance = 5f;
    [SerializeField] private float _reachedDestination = 5f;
    [SerializeField] private ParticleSystem _DeathParticle;
    [SerializeField] private GameObject _Visuals;
    [SerializeField] private Material _DeathMaterial;
    [SerializeField] readonly private float _wanderMaxCooldown = 5f;
    [SerializeField] private float _blindEyeCooldown = 15f;
    [SerializeField] AgentState _state = AgentState.Wander;

    private bool _hasReachedDestination = false;
    private Vector3 _wanderDestination = Vector3.zero;
    private float _wanderCooldown = 0f;
    private float _blindEyeTimer = 0f;
    private GameObject _player;

    private bool _isFollowing = false;
    private bool _isMarkedForKilling = false;
    private bool _isDead = false;
    private bool _hasSeenCrime = false;
    private bool _isReaped = false;
    private bool _isTurningBlindEye = false;

    private AwarenessBehavior _awarenessBehavior = null;

    public enum AgentState
    {
        Wander,
        Idle,
        Follow,
        Dead,
    }

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
            _awarenessBehavior.IsFollowing = value;
        }
    }

    public bool IsMarkedForKilling
    {
        get => _isMarkedForKilling;
        set
        {
            _isMarkedForKilling = value;

            // Makes sure we can't make a corpse follow us or kill it again
            _awarenessBehavior.IsFollowing = false;
            _awarenessBehavior.CanKill = false;

            // FUCKING DIE ALREADY
            _awarenessBehavior.IsDead = true;
        }
    }

    public bool HasSeenCrime
    {
        get => _hasSeenCrime;
        set
        {
            _hasSeenCrime = value;
        }
    }

    public bool IsReaped
    {
        get => _isReaped;
        set => _isReaped = value;
    }

    public AgentState State
    {
        get => _state;
        set => _state = value;
    }

    void Start()
    {
        CalculateWanderDestination();
    }

    public void Update()
    {
        HandleCoolDowns();

        if (_isMarkedForKilling && _state == AgentState.Dead)
        {
            // stop agent and mark as dead.
            _agent.isStopped = true;
            // unlock rotation.

            // set to prevent entering this again
            _isDead = true;
            _state = AgentState.Dead;

            // unmark to prevent looping animation
            _isMarkedForKilling = false;

            _Visuals.GetComponent<MeshRenderer>().material = _DeathMaterial;

            Instantiate(_DeathParticle, transform);
        }
        else if (_state == AgentState.Follow && _player)
        {
            Target = _player.transform.position;
            Seek();

            return;
        }
        else if (_state == AgentState.Wander)
        {
            Wander();
        }

        WatchForCrimes();
    }

    private void HandleCoolDowns()
    {
        if (_isTurningBlindEye)
        {
            _blindEyeTimer += Time.deltaTime;

            if (_blindEyeTimer >= _blindEyeCooldown)
                _isTurningBlindEye = false;
        }
    }

    private void Wander()
    {
        if (_hasReachedDestination && _wanderCooldown >= _wanderMaxCooldown)
        {
            _hasReachedDestination = false;
            CalculateWanderDestination();
        }

        if (Vector3.Distance(_wanderDestination, transform.position) <= _reachedDestination)
        {
            _hasReachedDestination = true;

            // Increase wander delay
            _wanderCooldown += Time.deltaTime;
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

    // Tell the agent to ignore any future deaths until cooldown is over
    public void TurnBlindEye()
    {
        _isTurningBlindEye = true;
    }

    private void WatchForCrimes()
    {
        // vision cone and detect player stabbing agent
        if (!_isTurningBlindEye) 
        {
            // if agent has seen stuff
            TurnBlindEye();
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 1, 0.4f);
        Gizmos.DrawSphere(transform.position, _reachedDestination);

        Gizmos.DrawSphere(_wanderDestination, 2f);
    }
}
