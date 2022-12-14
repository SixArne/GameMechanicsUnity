using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class Gamemode : MonoBehaviour
{
    private const string _levelname = "BuyScene";

    [SerializeField]
    [Tooltip("The number of people to kill by end of level")]
    private int _quotaCount = 5;

    [SerializeField]
    [Tooltip("The number of quota to increase on level change.")]
    private int _quotaIncrease = 5;

    [SerializeField]
    private float _endLevelDelaySeconds = 5f;

    [SerializeField]
    private int _soulsToSpend = 0;

    private int _totalKills = 0;
    private int _leftToKill = 0;
    private bool _levelHasEnded = false;
    private int _originalQuotaCount = 1;

    private SoulUpgradeData _abilityData = null;
    private bool _hasUsedUpgrade = false;

    public int QuotaCount
    {
        get => _quotaCount;
    }

    public int LeftToKill
    {
        get => _leftToKill;
    }

    public bool LevelHasEnded
    {
        get => _levelHasEnded;
    }

    public int TotalKills
    {
        get => _totalKills;
    }

    public int SoulsToSpend
    {
        get => _soulsToSpend;
    }

    public bool HasUsedUpgrade
    {
        get => _hasUsedUpgrade;
        set => _hasUsedUpgrade = value;
    }

    public void DecreaseSoulsToSpend(int cost)
    {
        _soulsToSpend -= cost;
    }

    public SoulUpgradeData AbilityData
    {
        get => _abilityData;
        set => _abilityData = value;
    }

    private void Awake()
    {
        // Make Gamemode class persist through game
        _leftToKill = _quotaCount;
        DontDestroyOnLoad(this);
    }

    public void OnAgentCollected()
    {
        // Called when the reaper collects a soul

        --_leftToKill;
        _totalKills++;
        _soulsToSpend++;

        if (_leftToKill == 0)
        {
            _levelHasEnded = true;

            IncreaseDifficulty();

            // Start callback that will select scene
            Invoke("OnLevelCompleted", _endLevelDelaySeconds);
        }
    }

    private void OnLevelCompleted()
    {
        SceneManager.LoadScene(_levelname);
        _levelHasEnded = false;
    }

    private void IncreaseDifficulty()
    {
        _quotaCount += _quotaIncrease;
        _leftToKill = _quotaCount;
    }

    public void Reset()
    {
        _leftToKill = 1;
        _totalKills = 0;
        _levelHasEnded = false;
        _quotaCount = _originalQuotaCount;
    }

    public void Update()
    {
        if (_hasUsedUpgrade)
        {
            // if player has used ability take it away from him.
            _abilityData = null;
            _hasUsedUpgrade = false;
        }
    }
}
