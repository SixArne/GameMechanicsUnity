using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

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
    [SerializeField] private float _heinSpeedIncrease = 0.5f;
    [SerializeField] private float _speedIncreaseEveryXSeconds = 10f;

    [Header("SceneManager")]
    [SerializeField] private CustomSceneManager _sceneManager = null;

    [Header("Audio")]
    [SerializeField] private AudioSource _playerKillSFX = null;

    [Header("References")]
    [SerializeField] private ChatBillboard _chatBillBoard = null;
    [SerializeField] private ParticleSystem _teleportParticle = null;

    const string _playerTag = "Friendly";
    const string _levelEndMessage = "I EXPECT MORE TOMORROW";
    private PlayerCharacter _playerCharacter;

    private bool _canKillPlayer = true;
    private bool _isGameOver = false;
    private bool _hasPlayedLevelEndAnimation = false;
    private bool _isCollecting = false;
    private GameObject _deadAgent = null;
    private Gamemode _gamemode = null;

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
    }

    private void Start()
    {   
        // only 1 of each in scene so this way of fetching is fine
        _playerCharacter = GameObject.FindObjectOfType<PlayerCharacter>();
        if (!_playerCharacter)
            throw new UnityException("Player script not found");

        _sceneManager = GameObject.FindObjectOfType<CustomSceneManager>();
        if (!_sceneManager)
            throw new UnityException("Scene manager script not found");

        _gamemode = GameObject.FindObjectOfType<Gamemode>();
        if (!_gamemode)
            throw new UnityException("Gamemode not found");

        // We need our chatbillboard of our player
        _chatBillBoard = GetComponent<ChatBillboard>();
        if (!_chatBillBoard)
            throw new UnityException("Chatbillboard script not found");

        // Increases Hein speed automatically;
        InvokeRepeating("IncreaseSpeed", _speedIncreaseEveryXSeconds, _speedIncreaseEveryXSeconds);
    }

    private void IncreaseSpeed()
    {
        _agent.speed += _heinSpeedIncrease;
    }

    private void TeleportToPlayer()
    {
        Vector3 playerPos = _playerCharacter.transform.position;
        float wanderDistance = 2f;

        Vector2 randomDirection = Random.insideUnitCircle;
        Vector3 randomWanderLocation = new Vector3(
            playerPos.x + randomDirection.x * wanderDistance,
            playerPos.y,
            playerPos.z + randomDirection.y * wanderDistance
          );

        // Make sure that teleport position is on navmesh
        NavMeshHit navMeshHit;
        if (NavMesh.SamplePosition(randomWanderLocation, out navMeshHit, 100f, NavMesh.AllAreas))
        {
            transform.position = navMeshHit.position;
        }
    }

    private void Update()
    {
        if (!_playerCharacter)
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

        if (!_hasPlayedLevelEndAnimation && _gamemode.LevelHasEnded)
        {
            TeleportToPlayer();
            Instantiate(_teleportParticle, transform.position, transform.rotation);
            _chatBillBoard.SetText(_levelEndMessage, 3f);
            _hasPlayedLevelEndAnimation = true;
            _agent.isStopped = true; // Prevent grim from walking
        }

        if (_gamemode.LevelHasEnded)
        {
            return; // Disable Grim reaper on level end.
        }

        CheckTouchPlayer();
    }

    private void CheckTouchPlayer()
    {
        Vector3 transformPosition = _playerCharacter.transform.position - transform.position;

        // If in distance and chasing
        if (transformPosition.sqrMagnitude <= _killRadius * _killRadius && _canKillPlayer && _state == GrimState.Chasing)
        {
            if (!_playerCharacter.HasUsedAbility && _playerCharacter.AbilityData && _playerCharacter.AbilityData.abilityType == SoulUpgradeData.AbilityType.OnDead)
            {
                // Execute ability on the player.
                _playerCharacter.AbilityData.abilityScript.OnExecute();
                return;
            }

            _canKillPlayer = false;

            // Cleanup player data
            if (_playerCharacter)
            {
                _playerKillSFX.Play();
                _playerCharacter.DestroyPlayer();
            }

            if (!_isGameOver)
            {
                Invoke("EndGame", 0.5f);
            }

            // Put grim in collecting mode to avoid missing player script
            _state = GrimState.Collecting;
        }
    }
    
    private void EndGame()
    {
        _sceneManager.DeathScene();
    }

    private void CollectSoul()
    {
        // if no body found skip
        if (!_deadAgent)
        {
            return;
        }

        float distanceSqr = Vector3.SqrMagnitude(_deadAgent.gameObject.transform.position - transform.position);

        if (distanceSqr <= _collectRadius * _collectRadius && !_isCollecting)
        {
            // Invoked function will unset the _isCollecting variable
            Invoke("CollectBody", _collectDuration);
            _isCollecting = true;
        }
        else if (distanceSqr > _collectRadius * _collectRadius)
        {
            // Set location data.
            Target = _deadAgent.gameObject.transform.position;
            base.Seek();
        }
    }

    private void CollectBody()
    {
        _state = GrimState.Chasing;
        _deadAgent.GetComponent<AgentCharacter>().IsReaped = true;
        

        // Make hein bit faster
        _agent.speed += _heinSpeedIncrease;

        _gamemode.OnAgentCollected();
        _isCollecting = false;
        _deadAgent = null;
    }

    private void ChaseProtagist()
    {
        Target = _playerCharacter.transform.position;
        base.Seek();
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.4f);
        Gizmos.DrawSphere(transform.position, _killRadius);

        if (_deadAgent)
        {
            Gizmos.color = new Color(0, 1, 1, 0.5f);
            Gizmos.DrawSphere(_deadAgent.gameObject.transform.position, _collectRadius);
        }   
    }
}
