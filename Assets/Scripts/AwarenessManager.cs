using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class AwarenessManager : MonoBehaviour
{
    public enum AwarenessLevel
    {
        Normal = 0,
        Alerted,
        HighAlert,
        Elimination
    }

    [SerializeField] private float _publicAwareness = 0f;
    [SerializeField] private AwarenessLevel _level = AwarenessLevel.Normal;
    [SerializeField] private GameObject _player = null;
    [SerializeField] private GameObject _grim = null;
    [SerializeField] private AudioSource _securityIncreaseSound = null;

    const string _playerTag = "Friendly";
    const string _agentTag = "Agent";
    const string _grimTag = "Enemy";

    private List<AgentCharacter> _agents = new List<AgentCharacter>();
    private GrimReaper _reaper = null;
    private Vignette _ppp;

    void Awake()
    {
        _player = GameObject.FindGameObjectWithTag(_playerTag);
        _grim = GameObject.FindGameObjectWithTag(_grimTag);
        _ppp = Camera.main.gameObject.GetComponent<PostProcessVolume>().profile.GetSetting<Vignette>();

        GameObject[] agents = GameObject.FindGameObjectsWithTag(_agentTag);

        foreach (GameObject agent in agents)
            _agents.Add(agent.GetComponent<AgentCharacter>());

        if (!_player)
            throw new UnityException("Could not find player");

        if (_grim)
            _reaper = _grim.GetComponent<GrimReaper>();

        if (!_ppp)
            throw new UnityException("Could not find post-process volume");
    }

    public AwarenessLevel Level
    {
        get => _level;
    }

    public float PublicAwareness
    {
        get => _publicAwareness;
    }

    private void IncreaseAwareness()
    {
        _publicAwareness += 5;
    }

    private void DecreaseAwareness(int amount)
    {
        _publicAwareness -= amount;
    }

    private void OnSecurityIncrease()
    {
        if (_securityIncreaseSound != null)
            _securityIncreaseSound.Play();

        StartCoroutine("IncreaseVignette");

        foreach (AgentCharacter agent in _agents)
        {
            // notify agents of security increase.
        }

        // Spawn special units after while
    }

    IEnumerator IncreaseVignette()
    {
        float vignetteLevel = 0f;

        while (vignetteLevel <= 0.17f)
        {
            _ppp.intensity.value += Time.deltaTime;
            vignetteLevel += Time.deltaTime;

            yield return new WaitForSeconds(0.05f);
        }
    }

    private void MonitorPublicAwareness()
    {
        // Every 10 stages the awareness will increase.
        if ((int)_publicAwareness / 10 >= 1)
        {
            int oldLevel = (int)_level;
            int newLevel = Mathf.Clamp(oldLevel + 1, 0, 3);
            _level = (AwarenessLevel)newLevel;
            
            if ((AwarenessLevel)oldLevel != _level)
                OnSecurityIncrease();

            _publicAwareness = 0f;
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

            if (agent.HasSeenCrime && agent.State != AgentCharacter.AgentState.Dead)
            {
                IncreaseAwareness();

                // Set agent blind to crimeS
                agent.HasSeenCrime = false;
                Debug.Log("OMG SHE IS DEAD");
            }
        }
    }
}
