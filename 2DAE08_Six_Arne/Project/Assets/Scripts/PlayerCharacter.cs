using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : BasicCharacter
{
    const string HORIZONTAL_MOVEMENT = "horizontalMovement";
    const string VERTICAL_MOVEMENT = "verticalMovement";
    const string GROUND_LAYER = "Ground";

    [SerializeField] private LayerMask _interactionMask;

    private Plane _cursorMovementPlane;

    protected override void Awake()
    {
        base.Awake();
        _cursorMovementPlane = new Plane(Vector3.up, transform.position);
    }

    void Update()
    {
        HandleMovement();

        HandleInteraction();
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
        if (Input.GetKeyUp(KeyCode.F))
        {
            var colliders = Physics.OverlapSphere(transform.position, 3, _interactionMask);

            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.tag == "Enemy")
                {
                    AgentCharacter agentCharacter = collider.gameObject.GetComponentInParent<AgentCharacter>();
                        //todo ask for better way to solve this
                    agentCharacter.IsFollowing = !agentCharacter.IsFollowing;
                    return;
                }
            }
        }
    }
}
