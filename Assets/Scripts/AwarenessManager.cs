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
    }

    [Header("Settings")]
    [SerializeField] private AwarenessLevel _level = AwarenessLevel.Normal;
    [SerializeField] private int _awarenessAmountOnSeen = 5;
    [SerializeField] private float _randomThoughtInterval = 10f;
    [SerializeField] private bool _canIgnoreGrim = false;
    [SerializeField][Range(0, 0.05f)] private float _vignetteStep = 0.005f;

    [Header("Audio")]
    [SerializeField] private AudioSource _securityIncreaseMildSFX = null;
    [SerializeField] private AudioSource _securityIncreaseSevereSFX = null;
    [SerializeField] private AudioSource _amnesiaSFX = null;
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

    // Random thoughts to display above player
    private List<string> _randomThoughts = new List<string>();

    // Chat bubble above player
    private ChatBillboard _chatBillboard;
    private PlayerCharacter _playerCharacter;

    void Start()
    {
        // Gather required components and check if they exist
        _playerCharacter = GameObject.FindObjectOfType<PlayerCharacter>();
        if (!_playerCharacter)
            throw new UnityException("Could not find player character");

        _reaper = GameObject.FindObjectOfType<GrimReaper>();
        if (!_reaper && !_canIgnoreGrim)
            throw new UnityException("Could not find reaper script");

        _chatBillboard = _playerCharacter.gameObject.GetComponent<ChatBillboard>();
        if (!_chatBillboard)
            throw new UnityException("Could not find chat board");

        _vignette = Camera.main.gameObject.GetComponent<PostProcessVolume>().profile.GetSetting<Vignette>();
        if (!_vignette)
            throw new UnityException("Could not find post-process volume");

        // Play the white noise city sounds
        _cityWhiteNoise.Play();

        // Gather all agents
        _agents = GameObject.FindObjectsOfType<AgentCharacter>().ToList<AgentCharacter>();

        PopulateRandomThoughts();

        // Random thought every x seconds
        InvokeRepeating("RandomThought", _randomThoughtInterval, _randomThoughtInterval);
    }

    void PopulateRandomThoughts()
    {
        // These could be jokes or in game hints
        _randomThoughts.Add("If someone sees a body they will be more wary");
        _randomThoughts.Add("Will I be able to eat tonight?");
        _randomThoughts.Add("Does Grim have a family?");
        _randomThoughts.Add("Maybe I can make some people follow me in an alley");
        _randomThoughts.Add("I need to make sure grim is always far away");
        _randomThoughts.Add("What if someone sees my sacrifices?");
        _randomThoughts.Add("Maybe I can trade souls for cool powers");
    }

    // Getters and setters
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

    private void OnSecurityIncrease()
    {
        if (!_isPlayingTheme && _theme != null)
        {
            _theme.Play();
            StartCoroutine("IncreaseAudio");
            _isPlayingTheme = true;
        }

        if (_level == AwarenessLevel.Alerted)
        {
            // Starts to transition vignette towards a given level
            StartCoroutine("IncreaseVignette", 0.4f);

            // Play mild level sfx
            if (_securityIncreaseMildSFX != null)
            {
                _securityIncreaseMildSFX.Play();
            }
                
            // We only want some agents to run from the player on a mild level
            foreach (AgentCharacter agent in _agents)
            {
                int val = Random.Range(0, 2);

                if (val % 2 == 0)
                {
                    agent.CanRunAway = true;
                }
            }
        }
        else if (_level == AwarenessLevel.HighAlert)
        {
            // Tell all agents to run on sight of player
            foreach (AgentCharacter agent in _agents)
            {
                agent.CanRunAway = true;
            }

            // Starts to transition vignette towards a given level
            StartCoroutine("IncreaseVignette", 0.8f);

            // Only execute if the player has not used and ability, has an ability and is using the right type
            if (!_playerCharacter.HasUsedAbility && _playerCharacter.AbilityData && _playerCharacter.AbilityData.abilityType == SoulUpgradeData.AbilityType.OnAwareness)
            {
                // Execute the player abilty scripts
                _playerCharacter.AbilityData.abilityScript.OnExecute();
                _amnesiaSFX.Play(); // Dirty fix but it works
            }
            else if (_securityIncreaseSevereSFX != null)
            {
                _securityIncreaseSevereSFX.Play();
            }
        }
    }

    IEnumerator IncreaseAudio()
    {
        // Coroutine will gradually increase volume level
        while (_theme.volume < _maxThemeVolume)
        {
            _theme.volume += _themeStep;
            yield return null;
        }
    }

    IEnumerator IncreaseVignette(float level)
    {
        // Coroutine will gradually increase vignette intensity
        while (_vignette.intensity.value <= level)
        {
            _vignette.intensity.value += _vignetteStep;
            yield return null;
        }
    }

    private void MonitorPublicAwareness()
    {
        // Every 10 stages the awareness will increase.
        if (_publicAwareness >= 10f)
        {
            int oldLevel = (int)_level;
            int newLevel = Mathf.Clamp(oldLevel + 1, 0, 2);
            _level = (AwarenessLevel)newLevel;
            
            if ((AwarenessLevel)oldLevel != _level)
            {
                OnSecurityIncrease();
            }
                
            _publicAwareness = 0f;
        }
    }

    private void RandomThought()
    {
        if (!_chatBillboard  || !_playerThoughtsEnabled)
            return; // exit early if chatboard is gone

        // Pick a random thought and display it.
        int random = Random.Range(0, _randomThoughts.Count);
        _chatBillboard.SetText(_randomThoughts[random], 5f);
    }

    private void Update()
    {
        // Check if something has happen and if so increase security level
        MonitorPublicAwareness();

        foreach (AgentCharacter agent in _agents)
        {
            if (!agent) // agent was disposed off
                continue;

            // If body wwas discovered inform grim
            if (agent.State == AgentCharacter.AgentState.Dead && !agent.IsReaped && _reaper)
            {
                _reaper.State = GrimReaper.GrimState.Collecting;
                _reaper.DeadAgent = agent.gameObject;
            }

            // If agent has seen crime, increase awareness
            if (agent.HasSeenCrime && agent.State != AgentCharacter.AgentState.Dead)
            {
                IncreaseAwareness();

                // Set agent blind to crimes
                agent.HasSeenCrime = false;
            }

            if ( agent.State == AgentCharacter.AgentState.Dead && agent.IsReaped)
            {
                Destroy(agent.gameObject);
            }
        }
    }

    #region Amnesia ability
    public void MakeAllAgentsBlind(float duration)
    {
        foreach (AgentCharacter agent in _agents)
        {
            if (agent.State != AgentCharacter.AgentState.Dead)
            {
                agent.BlindAgent(duration);
                agent.CanRunAway = false;
                agent.State = AgentCharacter.AgentState.Wander;
            }
        }

        // This will automatically unblind them
        StartCoroutine("UnBlindAllAgents", duration);
        StopCoroutine("IncreaseVignette");
    }

    public void ResetPublicAwareness()
    {
        _vignette.intensity.value = 0f;
        _publicAwareness = 0f;
        _level = AwarenessLevel.Normal;
        _theme.Stop(); // Stop creepy music
    }

    public IEnumerator UnBlindAllAgents(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        foreach (var agent in _agents)
        {
            agent.UnblindAgent();
        }
    }
    #endregion
}
