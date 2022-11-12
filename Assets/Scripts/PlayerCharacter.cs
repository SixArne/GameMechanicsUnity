using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;

public class PlayerCharacter : BasicCharacter
{
    const string HORIZONTAL_MOVEMENT = "horizontalMovement";
    const string VERTICAL_MOVEMENT = "verticalMovement";
    const string GROUND_LAYER = "Ground";

    [SerializeField] private LayerMask _interactionMask;
    [SerializeField] private float _interactRadius = 5.0f;
    [SerializeField] private float _killCooldown = 10f;
    [SerializeField] private Material _DeathMaterial;
    [SerializeField] private GameObject _Visuals;
    [SerializeField] private ParticleSystem _deathParticle;
    [SerializeField] GameObject _daggers;

    [Header("Audio")]
    [SerializeField] private AudioSource _stabSFX = null;

    private bool _hasUsedAbility = false;
    private SoulUpgradeData _abilityData = null;
    private Gamemode _gamemode;

    private Material _playerMat;
    private MeshRenderer _meshRenderer;
    private AgentCharacter _follower = null;
    private bool _canKill = true;
    private float _currentKillCooldown = 0f;
    private Plane _cursorMovementPlane;

    private const string _agentTag = "Agent";
    private int _closestId = -1;

    public int ClosestId
    {
        get => _closestId;
    }

    public SoulUpgradeData AbilityData
    {
        get => _abilityData;
        set => _abilityData = value;
    }

    public bool HasUsedAbility
    {
        get => _hasUsedAbility;
        set => _hasUsedAbility = value;
    }

    protected override void Awake()
    {
        base.Awake();
        _cursorMovementPlane = new Plane(Vector3.up, transform.position);

        _meshRenderer = _Visuals.GetComponent<MeshRenderer>();

        if (!_meshRenderer)
            throw new UnityException("No mesh renderer found");

        _playerMat = _meshRenderer.material;

        
    }

    private void Start()
    {
        // Fetch gamemode and request bought abilityData
        // This has to be here to make sure that the reaper exists. #LifeCycle struggles
        FetchAbilityData();
    }

    private void FetchAbilityData()
    {
        _gamemode = GameObject.FindObjectOfType<Gamemode>();
        _abilityData = _gamemode.AbilityData;

        // Load data required for script. (can be null in intro level)
        if (_abilityData != null)
        {
            _abilityData.abilityScript.OnAttachPlayer();
        }
    }

    /// <summary>
    /// This will be called by the ability to ask the player to discard any information.
    /// 
    /// Some abilities are not controlled by the player when they happen, hence this.
    /// </summary>
    public void DiscardAbilityData()
    {
        _abilityData = null;
        _hasUsedAbility = true;

        _gamemode.HasUsedUpgrade = true; // Inform the gamemode to get rid of abilityData.
    }

    void Update()
    {
        HandleCoolDowns();

        HandleMovement();

        HandleInteraction();
    }

    void HandleCoolDowns()
    {
        if (!_canKill)
            _currentKillCooldown += Time.deltaTime;

        if (_currentKillCooldown >= _killCooldown)
        {
            _currentKillCooldown = 0f;
            _canKill = true;

            //_meshRenderer.material = _playerMat;
            _daggers.SetActive(true);
        }
    }

    void HandleMovement()
    {
        if (_movementBehavior == null)
            return;

        float horizontalMovement = Input.GetAxisRaw(HORIZONTAL_MOVEMENT);
        float verticalMovement = Input.GetAxisRaw(VERTICAL_MOVEMENT);

        Vector3 movement = horizontalMovement * Vector3.right + verticalMovement * Vector3.forward;

        _movementBehavior.DesiredMovementDirection = movement;
    
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        Vector3 positionOfMouseInWorld = transform.position;

        RaycastHit hitInfo;
        if (Physics.Raycast(mouseRay, out hitInfo, 10000.0f, LayerMask.GetMask(GROUND_LAYER)))
        {
            positionOfMouseInWorld = hitInfo.point;
        }
        else
        {
            _cursorMovementPlane.Raycast(mouseRay, out float distance);
            positionOfMouseInWorld = mouseRay.GetPoint(distance);
        }

        _movementBehavior.DesiredLookAtPoint = positionOfMouseInWorld;
    }

    void HandleInteraction()
    {
        bool hasPressedFollow = Input.GetKeyUp(KeyCode.Q);
        bool hasPressedKill = Input.GetKeyUp(KeyCode.E);

        // Get nearby colliders
        Collider[] colliders = Physics.OverlapSphere(transform.position, _interactRadius, _interactionMask);
        Collider collider = FindClosest(colliders);

        if (collider != null)
        {
            // If we aren't dealing with an enemy exit early.
            if (collider.gameObject.tag != _agentTag)
                return;

            AgentCharacter agentCharacter = collider.gameObject.GetComponentInParent<AgentCharacter>();

            if (!agentCharacter.IsInteractable)
                return;

            agentCharacter.CanInteract = true;
            _closestId = agentCharacter.GetInstanceID();


            // Exit out early if agent is already dead.
            if (agentCharacter.State == AgentCharacter.AgentState.Dead)
                return;

            if (hasPressedKill && _canKill)
            {
                agentCharacter.State = AgentCharacter.AgentState.Dead;
                agentCharacter.IsMarkedForKilling = true;
                _canKill = false;

                _stabSFX.Play();

                //_meshRenderer.material = _DeathMaterial;
                _daggers.SetActive(false);
            }

            if (hasPressedFollow && !agentCharacter.CanRunAway)
            {
                // if pressing different agent then the one following
                if (_follower != null && _follower.GetInstanceID() != agentCharacter.GetInstanceID())
                {
                    _follower.State = AgentCharacter.AgentState.Wander;
                    _follower.IsFollowing = false;

                    agentCharacter.State = AgentCharacter.AgentState.Follow;
                    agentCharacter.IsFollowing = true;
                    _follower = agentCharacter;
                }
                else if (_follower == null)
                {
                    agentCharacter.State = AgentCharacter.AgentState.Follow;
                    agentCharacter.IsFollowing = true;
                    _follower = agentCharacter;
                }
                else if (_follower != null && _follower == agentCharacter)
                {
                    agentCharacter.State = AgentCharacter.AgentState.Wander;
                    agentCharacter.IsFollowing = !agentCharacter.IsFollowing;
                    _follower = null;
                }
            }
        }
        else
        {
            _closestId = 0;
        }
    }

    public void DestroyPlayer()
    {
        Instantiate(_deathParticle, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    private Collider FindClosest(Collider[] colliders)
    {
        if (colliders.Length == 0)
            return null;

        Collider closest = colliders[0];
        float closestDistanceSqr = _interactRadius * _interactRadius;

        foreach (var collider in colliders)
        {
            float distanceSqr = (collider.transform.position - transform.position).sqrMagnitude;

            if (distanceSqr < closestDistanceSqr && collider.CompareTag(_agentTag))
            {
                closest = collider;
                closestDistanceSqr = distanceSqr;
            }
        }

        if (!closest.CompareTag(_agentTag))
            return null;
        else
            return closest;
    }
}
