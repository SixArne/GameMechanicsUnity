using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityBuyScreen : MonoBehaviour
{
    [SerializeField] List<SoulUpgradeData> _abilities;
    [SerializeField] List<GameObject> _cards;
    [SerializeField] private float _buyTime = 10f;
    [SerializeField] private TMPro.TMP_Text _countDownTextElement;
    [SerializeField] private TMPro.TMP_Text _soulPointTextElement;
    [SerializeField] private Button _skipButton;
    
    private List<AbilityCard> _abilityCards = new List<AbilityCard>();
    private Gamemode _gamemode; // Holds data about game state
    private SceneManager _sceneManager;
    

    private void Awake()
    {
        // Get gamemode reference
        _gamemode = GameObject.FindObjectOfType<Gamemode>();
        _sceneManager = GameObject.FindObjectOfType<SceneManager>();
    }

    void Start()
    {

        int abilityIndex = 0;
        foreach (var card in _cards)
        {
            AbilityCard abilityCard = card.GetComponent<AbilityCard>();
            abilityCard.SetData(
                _abilities[abilityIndex].upgradeName,
                _abilities[abilityIndex].upgradeDescriptionn,
                _abilities[abilityIndex].upgradeCost.ToString(),
                _abilities[abilityIndex].abilityScript,
                abilityIndex
                );

            int soulCurrency = _gamemode.SoulsToSpend;
            if (soulCurrency < _abilities[abilityIndex].upgradeCost)
            {
                abilityCard.DisableButton();
            }

            _abilityCards.Add(abilityCard);

            abilityIndex++;
        }
    }

    void Update()
    {
        foreach (var ability in _abilityCards)
        {
            if (ability.IsSelected)
            {
                SoulUpgradeData upgradeData = _abilities[ability.AbilityIndex];
                _gamemode.AbilityData = upgradeData;

                // Call next map
                _sceneManager.Play();
            }
        }

        _countDownTextElement.text = ((int)_buyTime).ToString();
        _soulPointTextElement.text = ((int)_gamemode.SoulsToSpend).ToString();

        _buyTime -= Time.deltaTime;
        if (_buyTime <= 0)
        {
            // Call next map
            _sceneManager.Play();
        }
    }

    public void SkipBuy()
    {
        _sceneManager.Play();
    }
}
