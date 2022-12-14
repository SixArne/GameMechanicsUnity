using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BasicNavMeshAgent : MonoBehaviour
{
    [Header("Flee")]
    [SerializeField] protected float _fleeDistance = 5f;
    [SerializeField] protected float _fleeFactor = 2f;

    [Header("Arrive")]
    [SerializeField] protected float _arriveDistance = 2f;

    [Header("Target movement")]
    [SerializeField] protected MovementBehavior _targetMovement;

    [Header("Pursue")]
    [SerializeField] protected float _pursueMultiplier = 4f;
    
    protected Vector3 _target = Vector3.zero;
    protected NavMeshAgent _agent;

    protected Vector3 Target
    {
        get => _target;
        set => _target = value;
    }

    protected virtual void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    protected virtual void Flee()
    {
        if (Vector3.Distance(_target, transform.position) <= _fleeDistance)
        {
            Vector3 destination = _target - transform.position;
            _agent.SetDestination(-destination * _fleeFactor);
        }
    }

    protected virtual void Pursue(Vector3 velocity)
    {
        float distance = (_target - transform.position).magnitude;
        float distanceFactor = distance / _targetMovement.MovementSpeed;

        Vector3 targetForward = velocity.normalized;

        _agent.SetDestination(_target + targetForward * distanceFactor * _pursueMultiplier);
    }

    protected virtual void Seek()
    {
        _agent.SetDestination(_target);
    }
}
