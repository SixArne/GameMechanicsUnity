using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AwarenessManager : MonoBehaviour
{
    public enum AwarenessLevel
    {
        Normal = 0,
        Alerted = 10,
        HighAlert = 20,
        Elimination = 30
    }

    [SerializeField] private float _publicAwareness = 0f;
    [SerializeField] private AwarenessLevel _level = AwarenessLevel.Normal;
    [SerializeField] private GameObject _player = null;
    [SerializeField] private GameObject _grim = null;

    const string _playerTag = "Friendly";
    const string _agentTag = "Agent";
    const string _grimTag = "Enemy";

    private List<AgentCharacter> _agents = new List<AgentCharacter>();
    private GrimReaper _reaper = null;

    void Awake()
    {
        _player = GameObject.FindGameObjectWithTag(_playerTag);
        _grim = GameObject.FindGameObjectWithTag(_grimTag);

        GameObject[] agents = GameObject.FindGameObjectsWithTag(_agentTag);

        foreach (GameObject agent in agents)
            _agents.Add(agent.GetComponent<AgentCharacter>());

        if (!_player)
            throw new UnityException("Could not find player");

        if (_grim)
            _reaper = _grim.GetComponent<GrimReaper>();

    }

    public AwarenessLevel Level
    {
        get => _level;
    }

    private void IncreaseAwareness()
    {
        _publicAwareness += 1;
    }

    private void DecreaseAwareness(int amount)
    {
        _publicAwareness -= amount;
    }

    private void OnSecurityIncrease()
    {
        foreach (AgentCharacter agent in _agents)
        {
            // notify agents of security increase.
        }

        // Spawn special units after while
    }

    private void MonitorPublicAwareness()
    {
        // Every 10 stages the awareness will increase.
        if (_publicAwareness / 10 >= 1)
        {
            _level = (AwarenessLevel)Mathf.Clamp((int)_level++, 0, 3);
            OnSecurityIncrease();
        }
    }

    private void Update()
    {
        MonitorPublicAwareness();

        foreach (AgentCharacter agent in _agents)
        {
            if (agent.State == AgentCharacter.AgentState.Dead && !agent.IsReaped)
            {
                _reaper.State = GrimReaper.GrimState.Collecting;
                _reaper.DeadAgent = agent.gameObject;
            }

            if (agent.HasSeenCrime)
            {
                IncreaseAwareness();

                // Set agent blind to crimeS
                agent.HasSeenCrime = false;
            }
        }
    }
}
