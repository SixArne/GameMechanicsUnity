using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(VisionCone))]
public class AgentCharacter : BasicNavMeshAgent
{
    // States of agent
    public enum AgentState
    {
        Wander,
        Idle,
        Follow,
        Dead,
        Flee
    }

    [Header("Materials")]
    [SerializeField] private Material _DeathMaterial;
    [SerializeField] private Material _FollowMaterial;
    [SerializeField] private Material _AvoidMaterial;
    [SerializeField] private Material _NormalMaterial;
    [SerializeField] private ParticleSystem _DeathParticle;

    [Header("References")]
    [SerializeField] private GameObject _visuals;

    [Header("Settings")]
    [SerializeField] float _normalSpeed = 0f;
    [SerializeField] float _followSpeed = 3f;
    [SerializeField] float _fleeSpeed = 5f;
    [SerializeField] readonly private float _wanderMaxCooldown = 5f;
    [SerializeField] private float _blindEyeCooldown = 60f;
    [SerializeField] private float _wanderDistance = 5f;
    [SerializeField] private float _reachedDestination = 5f;
    [SerializeField] float _playerDetectRadius = 10f;
    [SerializeField] AgentState _state = AgentState.Wander;
    [SerializeField] LayerMask _playerMask;

    [Header("Gizmos")]
    [SerializeField] bool _displayPlayerDetectRadius = false;
    [SerializeField] bool _displayReachedDestination = false;
    [SerializeField] bool _displayWanderDestination = false;

    [Header("Settings --")]
    [SerializeField] [Tooltip("Disabling this will disable its update loop entirely")] bool _isInteractable = true;

    private bool _hasReachedDestination = false;
    private Vector3 _wanderDestination = Vector3.zero;
    private float _wanderCooldown = 0f;
    private float _blindEyeTimer = 0f;
    private PlayerCharacter _player;

    private bool _isFollowing = false;
    private bool _isMarkedForKilling = false;
    private bool _hasSeenCrime = false;
    private bool _isReaped = false;
    private bool _isTurningBlindEye = false;
    private bool _canInteract = false;
    private bool _canRunAway = false;
    private float _originalBlindEyeCooldown = 0f;

    private AwarenessBehavior _awarenessBehavior = null;
    private VisionCone _visionCone = null;
    private MeshRenderer _meshRenderer = null;

    protected override void Awake()
    {
        base.Awake();

        _originalBlindEyeCooldown = _blindEyeCooldown;
    }

    #region GetSet
    public bool IsFollowing
    {
        get => _isFollowing;
        set
        {
            _isFollowing = value;
            _awarenessBehavior.IsFollowing = value;
        }
    }

    public bool IsInteractable
    {
        get => _isInteractable;
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
            _awarenessBehavior.CanFollow = !value;
        }
    }

    public bool IsTurningBlindEye
    {
        get => _isTurningBlindEye;
        set
        {
            _isTurningBlindEye = value;
            _blindEyeTimer = 0f;
        }
    }
    #endregion

    public void BlindAgent(float duration)
    {
        // This method is always called on the "Amnesia" ability usage
        _isTurningBlindEye = true;
        _hasSeenCrime = false;
        _blindEyeCooldown = duration;
    }

    public void UnblindAgent()
    {
        // This method is always called at the end of the "Amnesia" ability usage
        _isTurningBlindEye = false;
        _blindEyeCooldown = _originalBlindEyeCooldown;
    }

    void Start()
    {
        // Get player reference
        _player = GameObject.FindObjectOfType<PlayerCharacter>();

        // Get awareness behavior (resposible for visibilaty action menu above agent)
        _awarenessBehavior = GetComponent<AwarenessBehavior>();

        // Get vision cone component (resonsible for detecting dead agents)
        _visionCone = GetComponent<VisionCone>();

        // Meshrenderer for setting different materials based on state
        _meshRenderer = _visuals.GetComponent<MeshRenderer>();

        // Overwrite speed by serializefield
        _agent.speed = _normalSpeed;

        // Agents always start wandering, so we need to calculate a destination
        CalculateWanderDestination();
    }

    public void Update()
    {
        // Handle cooldowns of agent
        HandleCoolDowns();

        // See of agent needs to flee or not
        DetermineState();

        // Every agent is resposible for setting if he is interactible
        // Only player keeps track of closest agent
        MakeInteractibleIfClosest();

        // Make sure the player exists, otherwise we will get errors
        if (!_player)
            return;

        // This can only execute once, right after the player has killed an agent
        if (_isMarkedForKilling)
        {
            OnMarkedForKilling();
        }
        else if (_state == AgentState.Follow)
        {
            // Target is what the base class uses to move towards.
            // When setting this property Seek() will automatically go for this.
            Target = _player.transform.position;

            // overwrite old mats
            _meshRenderer.material = _FollowMaterial;
            _agent.speed = _followSpeed;

            Seek();
        }
        else if (_state == AgentState.Wander)
        {
            // overwrite old mats
            _meshRenderer.material = _NormalMaterial;
            _agent.speed = _normalSpeed;

            Wander();
        }
        else if (_state == AgentState.Flee)
        {
            // overwrite old mats
            _meshRenderer.material = _AvoidMaterial;
            _agent.speed = _fleeSpeed;
        
            Flee();
        }

        WatchForCrimes();
    }

    private void DetermineState()
    {
        // We only want to target wandering agents
        if (_state != AgentState.Flee && _state != AgentState.Dead)
        {
            // Check if player is in vacinity.
            Collider[] colliders = Physics.OverlapSphere(transform.position, _playerDetectRadius, _playerMask);

            // If player is here and the agent has the urge to run change state
            if (colliders.Length > 0 && CanRunAway)
            {
                _state = AgentState.Flee;
                _agent.speed = _fleeSpeed;
            }
        } 
    }

    private void MakeInteractibleIfClosest()
    {
        int myId = GetInstanceID();
        if (_player.ClosestId != myId)
        {
            // only the getter triggers the awareness behavior
            CanInteract = false;
        }
    }

    private void OnMarkedForKilling()
    {
        // Tell the navmesh agent to stop working
        _agent.isStopped = true;

        // Set state to dead.
        _state = AgentState.Dead;

        // Set all tags to dead. (Used in vision cone for agents)
        foreach (Transform t in transform)
        {
            t.gameObject.tag = "dead";
        }
        gameObject.tag = "dead";

        // unmark to prevent looping animation
        _isMarkedForKilling = false;

        // Change material
        _meshRenderer.material = _DeathMaterial;
        
        // Make sure the visioncone doesn't self detect
        _visionCone.enabled = false;
        gameObject.GetComponent<NavMeshAgent>().enabled = false;

        // Turn body for extra dead feeling
        transform.Rotate(new Vector3(0f, 0f, 90f));

        // Play a kill particle
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
        if ((_player.transform.position - transform.position).sqrMagnitude <= _fleeDistance * _fleeDistance)
        {
            // calculate new dest
            _hasReachedDestination = false;
            CalculateFleeDestination();
        }

        if ((_wanderDestination - transform.position).sqrMagnitude <= _reachedDestination * _reachedDestination)
        {
            _hasReachedDestination = true;
            return;
        }

        if (_hasReachedDestination)
        {
            _state = AgentState.Wander;
            _agent.speed = _normalSpeed;
            CalculateWanderDestination();
        }

        Target = _wanderDestination;
        base.Seek();
    }

    private void CalculateFleeDestination()
    {
        // Get any position away from the player.
        // This needs to have a better working implementation later
        Vector3 offset = (_player.transform.position - transform.position).normalized;

        // Actually sample the point and make sure it's on the navmesh.
        NavMeshHit navMeshHit;
        if (NavMesh.SamplePosition(offset * 50f, out navMeshHit, 100f, NavMesh.AllAreas))
        {
            _wanderDestination = navMeshHit.position;
        }
    }

    private void CalculateWanderDestination()
    {
        // Take a random direction in the unitCircle and set it as a destination.
        Vector2 randomDirection = Random.insideUnitCircle;
        Vector3 randomWanderLocation = new Vector3(
            transform.position.x + randomDirection.x * _wanderDistance,
            transform.position.y,
            transform.position.z + randomDirection.y * _wanderDistance
            );

        // Sample position to make sure it's on the navmesh.
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
            // NOTE: AwarenessManager will pick up on whether an agent has seen a crime and handle it accordinly.
            _hasSeenCrime = true;
            _isTurningBlindEye = true;
        }
    }

    public void OnDrawGizmos()
    {
        if (_displayReachedDestination)
        {
            Gizmos.color = new Color(1, 0, 1, 0.4f);
            Gizmos.DrawSphere(transform.position, _reachedDestination);
        }

        if (_displayPlayerDetectRadius)
        {
            Gizmos.color = new Color(0, 0, 1, 0.4f);
            Gizmos.DrawSphere(transform.position, _playerDetectRadius);
        }
        
        if (_displayWanderDestination)
        {
            Gizmos.color = new Color(1, 1, 1, 0.4f);
            Gizmos.DrawSphere(_wanderDestination, 2f);
        }
    }
}
