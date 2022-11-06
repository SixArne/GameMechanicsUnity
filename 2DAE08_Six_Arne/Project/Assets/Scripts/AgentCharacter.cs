using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(VisionCone))]
public class AgentCharacter : BasicNavMeshAgent
{
    [SerializeField] private float _wanderDistance = 5f;
    [SerializeField] private float _reachedDestination = 5f;
    [SerializeField] private ParticleSystem _DeathParticle;
    [SerializeField] private GameObject _visuals;
    [SerializeField] private Material _DeathMaterial;
    [SerializeField] readonly private float _wanderMaxCooldown = 5f;
    [SerializeField] private float _blindEyeCooldown = 60f;
    [SerializeField] AgentState _state = AgentState.Wander;
    [SerializeField] float _playerDetectRadius = 10f;
    [SerializeField] LayerMask _playerMask;
    [SerializeField] float _normalSpeed = 0f;
    [SerializeField] float _fleeSpeed = 5f;

    private bool _hasReachedDestination = false;
    private Vector3 _wanderDestination = Vector3.zero;
    private float _wanderCooldown = 0f;
    private float _blindEyeTimer = 0f;
    private PlayerCharacter _player;

    private bool _isFollowing = false;
    private bool _isMarkedForKilling = false;
    private bool _isDead = false;
    private bool _hasSeenCrime = false;
    private bool _isReaped = false;
    private bool _isTurningBlindEye = false;
    private bool _canInteract = false;
    private bool _canRunAway = false;

    private AwarenessBehavior _awarenessBehavior = null;
    private VisionCone _visionCone = null;

    public enum AgentState
    {
        Wander,
        Idle,
        Follow,
        Dead,
        Flee
    }

    protected override void Awake()
    {
        base.Awake();

        _player = GameObject.FindObjectOfType<PlayerCharacter>();
        _awarenessBehavior = GetComponent<AwarenessBehavior>();
        _visionCone = GetComponent<VisionCone>();

        _agent.speed = _normalSpeed;
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

    public bool CanInteract
    {
        get => _canInteract;
        set
        {
            _canInteract = value;
            _awarenessBehavior.CanInteract = value;
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

    public bool CanRunAway
    {
        get => _canRunAway;
        set
        {
            _canRunAway = value;
            _awarenessBehavior.CanFollow = false;
        }
    }

    void Start()
    {
        CalculateWanderDestination();
    }

    public void Update()
    {
        HandleCoolDowns();

        DetermineState();

        OnClosest();

        if (_isMarkedForKilling && _state == AgentState.Dead)
        {
            OnDead();
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
        else if (_state == AgentState.Flee)
        {
            Flee();
        }

        WatchForCrimes();
    }

    private void DetermineState()
    {
        if (_state != AgentState.Flee && _state != AgentState.Dead)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, _playerDetectRadius, _playerMask);

            if (colliders.Length > 0 && CanRunAway)
            {
                Debug.Log("RUN");
                _state = AgentState.Flee;
                _agent.speed = _fleeSpeed;
            }
        } 
    }

    private void OnClosest()
    {
        int myId = GetInstanceID();
        if (_player.ClosestId != myId)
        {
            // only the getter triggers the awareness behavior
            CanInteract = false;
        }
    }

    private void OnDead()
    {
        // stop agent and mark as dead.
        _agent.isStopped = true;
        // unlock rotation.

        // set to prevent entering this again
        _isDead = true;
        _state = AgentState.Dead;

        foreach (Transform t in transform)
        {
            t.gameObject.tag = "dead";
        }

        gameObject.tag = "dead";

        // unmark to prevent looping animation
        _isMarkedForKilling = false;

        _visuals.GetComponent<MeshRenderer>().material = _DeathMaterial;
        
        // Dead men tell no tales
        _visionCone.enabled = false;
        gameObject.GetComponent<NavMeshAgent>().enabled = false;

        transform.Rotate(new Vector3(0f, 0f, 90f));

        Instantiate(_DeathParticle, transform);
    }

    private void HandleCoolDowns()
    {
        if (_isTurningBlindEye)
        {
            _blindEyeTimer += Time.deltaTime;

            if (_blindEyeTimer >= _blindEyeCooldown)
            {
                _isTurningBlindEye = false;
                _blindEyeTimer = 0f;
            }
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

    protected override void Flee()
    {
        if (_hasReachedDestination)
        {
            if ((_player.transform.position - transform.position).sqrMagnitude <= _fleeDistance * _fleeDistance)
            {
                // calculate new dest
                _hasReachedDestination = false;
                CalculateFleeDestination();
            }
            else
            {
                _state = AgentState.Wander;
                _agent.speed = _normalSpeed;
                CalculateWanderDestination();
            }
        }

        if ((_wanderDestination - transform.position).sqrMagnitude <= _reachedDestination * _reachedDestination)
        {
            _hasReachedDestination = true;
            return;
        }

        _target = _wanderDestination;
        base.Seek();
    }

    private void CalculateFleeDestination()
    {
        Vector3 offset = (_player.transform.position - transform.position).normalized;

        NavMeshHit navMeshHit;
        if (NavMesh.SamplePosition(offset * 50f, out navMeshHit, 100f, NavMesh.AllAreas))
        {
            _wanderDestination = navMeshHit.position;
        }
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
        // if body is in sight
        if (_visionCone.IsInSight && !_isTurningBlindEye)
        {
            _hasSeenCrime = true;
            _isTurningBlindEye = true;
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 1, 0.4f);
        Gizmos.DrawSphere(transform.position, _reachedDestination);

        Gizmos.color = new Color(0, 0, 1, 0.4f);
        Gizmos.DrawSphere(transform.position, _playerDetectRadius);

        Gizmos.color = new Color(1, 1, 1, 0.4f);
        Gizmos.DrawSphere(_wanderDestination, 2f);
    }
}
