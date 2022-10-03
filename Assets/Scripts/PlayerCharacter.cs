using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCharacter : BasicCharacter
{
    const string HORIZONTAL_MOVEMENT = "horizontalMovement";
    const string VERTICAL_MOVEMENT = "verticalMovement";
    const string GROUND_LAYER = "Ground";

    private Plane _cursorMovementPlane;

    protected override void Awake()
    {
        base.Awake();
        _cursorMovementPlane = new Plane(Vector3.up, transform.position);
    }

    void Update()
    {
        HandleMovement();
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
}
