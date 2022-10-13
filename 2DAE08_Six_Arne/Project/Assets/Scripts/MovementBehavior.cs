using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovementBehavior : MonoBehaviour
{
    [SerializeField]
    private float _movementSpeed = 10.0f;

    protected Rigidbody _rigidBody;

    protected Vector3 _desiredMoveDirection = Vector3.zero;
    protected Vector3 _desiredLookAtPoint = Vector3.zero;

    public Vector3 DesiredMovementDirection
    {
        get => _desiredMoveDirection;
        set => _desiredMoveDirection = value;
    }

    public Vector3 DesiredLookAtPoint
    {
        get => _desiredLookAtPoint;
        set => _desiredLookAtPoint = value;
    }

    public float MovementSpeed
    {
        get => _movementSpeed;
    }

    protected virtual void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    protected virtual void Update()
    {
        HandleRotation();
    }

    protected virtual void FixedUpdate()
    {
        HandleMovement();
    }

    protected virtual void HandleMovement()
    {
        Vector3 movement = _desiredMoveDirection.normalized;
        movement *= _movementSpeed;

        _rigidBody.velocity = movement;
    }

    protected virtual void HandleRotation()
    {
        transform.LookAt(_desiredLookAtPoint, Vector3.up);
    }
}
