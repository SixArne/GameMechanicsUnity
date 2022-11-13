using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : BasicCharacter
{
    const string HORIZONTAL_MOVEMENT = "horizontalMovement";
    const string VERTICAL_MOVEMENT = "verticalMovement";
    const string GROUND_LAYER = "Ground";
    const string _agentTag = "Agent";

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
    private MeshRenderer _meshRenderer;
    private AgentCharacter _follower = null;
    private bool _canKill = true;
    private Plane _cursorMovementPlane;
    private int _closestId = -1; // set to a impossible value

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

        // plane used to raycast on, this prevents the player from turning when the mouse is outside the playable area.
        _cursorMovementPlane = new Plane(Vector3.up, transform.position);

        // reference to set materials later
        _meshRenderer = _Visuals.GetComponent<MeshRenderer>();

        if (!_meshRenderer)
            throw new UnityException("No mesh renderer found");
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
        // Handle movement related input and actions
        HandleMovement();

        // Handle interactions with agents
        HandleInteraction();
    }

    void EnableDaggers()
    {
        // This is a cooldown invoked method
        _canKill = true;
        _daggers.SetActive(true);
    }

    void HandleMovement()
    {
        if (_movementBehavior == null)
            return;

        float horizontalMovement = Input.GetAxisRaw(HORIZONTAL_MOVEMENT);
        float verticalMovement = Input.GetAxisRaw(VERTICAL_MOVEMENT);

        // Our movementbehavior will handle the actual movement
        Vector3 movement = horizontalMovement * Vector3.right + verticalMovement * Vector3.forward;
        _movementBehavior.DesiredMovementDirection = movement;

        Vector3 newPosition = transform.position;

        // We will check if our mouse camera ray hits the floor of the map, if not we take the hidden plane.
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(mouseRay, out hitInfo, 10000.0f, LayerMask.GetMask(GROUND_LAYER)))
        {
            newPosition = hitInfo.point;
        }
        else
        {
            _cursorMovementPlane.Raycast(mouseRay, out float distance);
            newPosition = mouseRay.GetPoint(distance);
        }

        // Set the lookat point
        _movementBehavior.DesiredLookAtPoint = newPosition;
    }

    void HandleInteraction()
    {
        // We only want keyup events, hence the hardcoded values
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

            // Get the agent component
            AgentCharacter agentCharacter = collider.gameObject.GetComponentInParent<AgentCharacter>();

            // early exit
            if (!agentCharacter)
                return;

            // Some agents will refuse to interact, hence the check
            if (!agentCharacter.IsInteractable)
                return;

            // We tell the actual agent that the player can interact with him
            agentCharacter.CanInteract = true;
            _closestId = agentCharacter.GetInstanceID();


            // Exit out early if agent is already dead.
            if (agentCharacter.State == AgentCharacter.AgentState.Dead)
                return;

            if (hasPressedKill && _canKill)
            {
                if (!_hasUsedAbility && _abilityData && _abilityData.abilityType == SoulUpgradeData.AbilityType.OnEndGame)
                {
                    // Execute ability on the player.
                    _abilityData.abilityScript.OnExecute();

                    Invoke("EndGame", 5f);
                }

                agentCharacter.State = AgentCharacter.AgentState.Dead;
                agentCharacter.IsMarkedForKilling = true;

                // Start kill cooldown
                _canKill = false;
                Invoke("EnableDaggers", _killCooldown);

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
                else if (_follower == null) // if pressing first agent
                {
                    agentCharacter.State = AgentCharacter.AgentState.Follow;
                    agentCharacter.IsFollowing = true;
                    _follower = agentCharacter;
                }
                else if (_follower != null && _follower == agentCharacter) // if releasing following agent
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

    private void EndGame()
    {
        // After killing grim go to menu scene
        GameObject.FindObjectOfType<CustomSceneManager>().MenuScene();
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

            // Needs to be closer and needs to be an agent
            if (distanceSqr < closestDistanceSqr && collider.CompareTag(_agentTag))
            {
                closest = collider;
                closestDistanceSqr = distanceSqr;
            }
        }

        // We only want agents to be registered, so if the closest is not an agent we exit
        if (!closest.CompareTag(_agentTag))
            return null;

        return closest;
    }
}
