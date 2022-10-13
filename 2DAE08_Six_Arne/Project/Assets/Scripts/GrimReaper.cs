using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GrimReaper : BasicNavMeshAgent
{
    enum GrimState
    {
        Collecting,
        Chasing,
    }

    [Header("Grim")]
    [SerializeField] private GrimState _state = GrimState.Collecting;
    [SerializeField] private float _collectDuration = 5f;
    [SerializeField] private float _killRadius = 5f;


    private GameObject _player;
    private float _currentTimer = 0f;

    protected override void Awake()
    {
        base.Awake();

        _player = GameObject.FindGameObjectWithTag("Friendly");
    }

    private void Update()
    {
        if (!_player)
            return;

        switch (_state)
        {
            case GrimState.Collecting:
                CollectSoul();
                break;
            case GrimState.Chasing:
                ChaseProtagist();
                break;
        }

        Vector3 transformPosition = _player.transform.position - transform.position;
        if (transformPosition.sqrMagnitude <= _killRadius * _killRadius)
        {
            Destroy(_player);
            _state = GrimState.Collecting;
        }
    }

    private void CollectSoul()
    {
        _currentTimer += Time.deltaTime;

        if (_currentTimer > _collectDuration)
        {
            _state = GrimState.Chasing;
            _currentTimer = 0f;
        }
    }

    private void ChaseProtagist()
    {
        Target = _player.transform.position;
        base.Seek();
    }

    protected override void OnDrawGizmos()
    {
        // Drawing steering behaviour
        base.OnDrawGizmos();

        Gizmos.color = new Color(1, 0, 0, 0.4f);
        Gizmos.DrawSphere(transform.position, _killRadius);
    }
}
