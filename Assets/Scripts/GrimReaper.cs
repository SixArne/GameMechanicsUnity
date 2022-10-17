using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GrimReaper : BasicNavMeshAgent
{
    public enum GrimState
    {
        Collecting,
        Chasing,
    }

    [Header("Grim")]
    [SerializeField] private GrimState _state = GrimState.Collecting;
    [SerializeField] private float _collectDuration = 5f;
    [SerializeField] private float _collectRadius = 3f;
    [SerializeField] private float _killRadius = 5f;

    const string _playerTag = "Friendly";

    private GameObject _player;
    private float _currentTimer = 0f;
    private PlayerCharacter _playerCharacter;

    private bool _canKillPlayer = true;
    private GameObject _deadAgent = null;

    public GrimState State
    {
        get => _state;
        set => _state = value;
    }

    public GameObject DeadAgent
    {
        get => _deadAgent;
        set => _deadAgent = value;
    }

    protected override void Awake()
    {
        base.Awake();

        _player = GameObject.FindGameObjectWithTag(_playerTag);
        _playerCharacter = _player.GetComponent<PlayerCharacter>();

        if (!_player)
            throw new UnityException("No player found");
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
        if (transformPosition.sqrMagnitude <= _killRadius * _killRadius && _canKillPlayer)
        {
            _canKillPlayer = false;
            _playerCharacter.DestroyPlayer();

            _state = GrimState.Collecting;
        }
    }

    private void CollectSoul()
    {
        if (!_deadAgent)
            return;

        float distanceSqr = Vector3.SqrMagnitude(_deadAgent.gameObject.transform.position - transform.position);

        if (distanceSqr <= _collectRadius * _collectRadius)
        {
            _currentTimer += Time.deltaTime;

            if (_currentTimer > _collectDuration)
            {
                _state = GrimState.Chasing;
                _deadAgent.GetComponent<AgentCharacter>().IsReaped = true;
                _currentTimer = 0f;

                // Make hein bit faster
                _agent.speed += 10f;

                Destroy(_deadAgent);
            }
        }
        else
        {
            // Set location data.
            Target = _deadAgent.gameObject.transform.position;
            base.Seek();
        }
    }

    private void ChaseProtagist()
    {
        Target = _player.transform.position;
        base.Seek();
    }

    protected override void OnDrawGizmos()
    {
        // Drawing steering behavior
        base.OnDrawGizmos();

        Gizmos.color = new Color(1, 0, 0, 0.4f);
        Gizmos.DrawSphere(transform.position, _killRadius);

        if (_deadAgent)
        {
            Gizmos.color = new Color(0, 1, 1, 0.5f);
            Gizmos.DrawSphere(_deadAgent.gameObject.transform.position, _collectRadius);
        }   
    }
}
