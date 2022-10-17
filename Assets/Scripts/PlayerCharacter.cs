using System.Collections;
using System.Collections.Generic;
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

    private Material _playerMat;
    private MeshRenderer _meshRenderer;
    private bool _canKill = true;
    private float _currentKillCooldown = 0f;
    private Plane _cursorMovementPlane;

    private const string _agentTag = "Agent";

    protected override void Awake()
    {
        base.Awake();
        _cursorMovementPlane = new Plane(Vector3.up, transform.position);

        _meshRenderer = _Visuals.GetComponent<MeshRenderer>();

        if (!_meshRenderer)
            throw new UnityException("No mesh renderer found");

        _playerMat = _meshRenderer.material;
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

            _meshRenderer.material = _playerMat;
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
        bool hasPressedFollow = Input.GetKeyUp(KeyCode.F);
        bool hasPressedKill = Input.GetKeyUp(KeyCode.K);

        if (hasPressedKill || hasPressedFollow)
        {
            // Get nearby colliders
            Collider[] colliders = Physics.OverlapSphere(transform.position, _interactRadius, _interactionMask);

            foreach (Collider collider in colliders)
            {
                // If we aren't dealing with an enemy exit early.
                if (collider.gameObject.tag != _agentTag)
                    return;

                AgentCharacter agentCharacter = collider.gameObject.GetComponentInParent<AgentCharacter>();

                // Exit out early if agent is already dead.
                if (agentCharacter.IsMarkedForKilling)
                    return;

                if (hasPressedKill && _canKill)
                {
                    agentCharacter.IsMarkedForKilling = true;
                    _canKill = false;

                    _meshRenderer.material = _DeathMaterial;
                }

                if (hasPressedFollow)
                    agentCharacter.IsFollowing = !agentCharacter.IsFollowing;
            }
        }
    }

    public void DestroyPlayer()
    {
        Instantiate(_deathParticle, transform);

        // set active to let animation play out
        gameObject.SetActive(false);
        Destroy(gameObject, 2f);
    }
}
