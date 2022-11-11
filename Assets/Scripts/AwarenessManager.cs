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
    [SerializeField] private AudioSource _theme = null;
    [SerializeField] private AudioSource _ambientNoise = null;
    [SerializeField] private int _amountOnSeen = 5;
    [SerializeField] private float _randomThoughtTimer = 10f;

    [Header("Audio")]
    [SerializeField] [Range(0, 1f)] private float _maxThemeVolume = 1f;
    [SerializeField] [Range(0, 0.05f)] private float _themeStep = 0.005f;

    [Header("Player thoughts")]
    [SerializeField] private ChatBillboard _chatBillboard;
    [SerializeField] private bool _playerThoughtsEnabled = true;

    const string _playerTag = "Friendly";
    const string _agentTag = "Agent";
    const string _grimTag = "Enemy";

    private List<AgentCharacter> _agents = new List<AgentCharacter>();
    private GrimReaper _reaper = null;
    private Vignette _ppp;
    private bool _isPlayingTheme = false;

    private float _currentRandomThoughtTimer = 0f;

    private List<string> _randomThoughts = new List<string>();

    void Awake()
    {
        _player = GameObject.FindGameObjectWithTag(_playerTag);
        _grim = GameObject.FindGameObjectWithTag(_grimTag);
        _ppp = Camera.main.gameObject.GetComponent<PostProcessVolume>().profile.GetSetting<Vignette>();
        _ambientNoise.Play();

        GameObject[] agents = GameObject.FindGameObjectsWithTag(_agentTag);

        foreach (GameObject agent in agents)
            _agents.Add(agent.GetComponent<AgentCharacter>());

        if (!_player)
            throw new UnityException("Could not find player");

        if (_grim)
            _reaper = _grim.GetComponent<GrimReaper>();

        if (!_ppp)
            throw new UnityException("Could not find post-process volume");

        PopulateRandomThoughts();
    }

    void PopulateRandomThoughts()
    {
        _randomThoughts.Add("I better act carefull... don't want Grim and the police chasing me");
        _randomThoughts.Add("Will I be able to eat tonight?");
        _randomThoughts.Add("Does Grim have a family?");
        _randomThoughts.Add("Maybe I can make some people follow me in an alley");
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
        _publicAwareness += _amountOnSeen;
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

        if (_level == AwarenessLevel.Alerted)
        {
            if (!_isPlayingTheme && _theme != null)
            {
                _theme.Play();
                StartCoroutine("IncreaseAudio");
                _isPlayingTheme = true;
            }
                

            foreach (AgentCharacter agent in _agents)
            {
                int val = Random.Range(0, 4);

                if (val % 2 == 0)
                    agent.CanRunAway = true;
            }
        }
        else if (_level == AwarenessLevel.HighAlert)
        {
            if (!_isPlayingTheme && _theme != null)
            {
                _theme.Play();
                StartCoroutine("IncreaseAudio");
                _isPlayingTheme = true;
            }

            foreach (AgentCharacter agent in _agents)
            {
                agent.CanRunAway = true;
            }
        }
        //TODO: add highest alert
        

        // Spawn special units after while
    }

    IEnumerator IncreaseAudio()
    {
        float audioLevel = 0f;

        while (audioLevel < _maxThemeVolume)
        {
           
            audioLevel += _themeStep;
            _theme.volume = audioLevel;
            yield return new WaitForSeconds(0.05f);
        }
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

    private void RandomThought()
    {
        int random = Random.Range(0, _randomThoughts.Count);
        _chatBillboard.SetText(_randomThoughts[random], 3f);
    }

    private void Update()
    {
        MonitorPublicAwareness();

        _currentRandomThoughtTimer += Time.deltaTime;
        if (_currentRandomThoughtTimer >= _randomThoughtTimer && _playerThoughtsEnabled)
        {
            RandomThought();
            _currentRandomThoughtTimer = 0f;
        }

        foreach (AgentCharacter agent in _agents)
        {
            if (agent.State == AgentCharacter.AgentState.Dead && !agent.IsReaped && _reaper)
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
