using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [Header("References")]
    [SerializeField] private GameObject _player = null;
    [SerializeField] private GameObject _grim = null;

    [Header("Settings")]
    [SerializeField] private AwarenessLevel _level = AwarenessLevel.Normal;
    [SerializeField] private int _awarenessAmountOnSeen = 5;
    [SerializeField] private float _randomThoughtTimer = 10f;

    [Header("Audio")]
    [SerializeField] private AudioSource _securityIncreaseMildSFX = null;
    [SerializeField] private AudioSource _securityIncreaseSevereSFX = null;
    [SerializeField] private AudioSource _theme = null;
    [SerializeField] private AudioSource _cityWhiteNoise = null;
    [SerializeField] [Range(0, 1f)] private float _maxThemeVolume = 1f;
    [SerializeField] [Range(0, 0.05f)] private float _themeStep = 0.005f;

    [Header("Player thoughts")]
    [SerializeField] private bool _playerThoughtsEnabled = true;

    // Compile time strings
    const string _playerTag = "Friendly";
    const string _agentTag = "Agent";
    const string _grimTag = "Enemy";

    // Agents to keep track of, these will forward any crime
    private List<AgentCharacter> _agents = new List<AgentCharacter>();

    // Grim reaper script to set new corpse target
    private GrimReaper _reaper = null;

    // Used to control the public awareness vignette
    private Vignette _vignette = null;

    private bool _isPlayingTheme = false;
    private float _publicAwareness = 0f;

    // Counter for random thoughts
    private float _currentRandomThoughtTimer = 0f;

    // Random thoughts to display above player
    private List<string> _randomThoughts = new List<string>();

    // Chat bubble above player
    private ChatBillboard _chatBillboard;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag(_playerTag);
        _grim = GameObject.FindGameObjectWithTag(_grimTag);
        _vignette = Camera.main.gameObject.GetComponent<PostProcessVolume>().profile.GetSetting<Vignette>();
        

        // Play the white noise city sounds
        _cityWhiteNoise.Play();

        // Gather all agents
        _agents = GameObject.FindObjectsOfType<AgentCharacter>().ToList<AgentCharacter>();

        if (!_player)
            throw new UnityException("Could not find player");

        // Get board after getting player
        _chatBillboard = _player.GetComponent<ChatBillboard>();

        if (_grim)
            _reaper = _grim.GetComponent<GrimReaper>();

        if (!_vignette)
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
        _publicAwareness += _awarenessAmountOnSeen;
    }

    private void DecreaseAwareness(int amount)
    {
        _publicAwareness -= amount;
    }

    private void OnSecurityIncrease()
    {
        StartCoroutine("IncreaseVignette");

        if (_level == AwarenessLevel.Alerted)
        {
            if (!_isPlayingTheme && _theme != null)
            {
                _theme.Play();
                StartCoroutine("IncreaseAudio");
                _isPlayingTheme = true;
            }

            if (_securityIncreaseMildSFX != null)
                _securityIncreaseMildSFX.Play();


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

            if (_securityIncreaseSevereSFX != null)
                _securityIncreaseSevereSFX.Play();

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
            _vignette.intensity.value += Time.deltaTime;
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
            }
        }
    }
}
