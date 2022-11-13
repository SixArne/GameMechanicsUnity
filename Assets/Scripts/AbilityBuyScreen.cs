using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityBuyScreen : MonoBehaviour
{
    [SerializeField] List<SoulUpgradeData> _abilities;
    [SerializeField] List<GameObject> _cards;
    [SerializeField] private TMPro.TMP_Text _soulPointTextElement;
    [SerializeField] private Button _skipButton;
    
    // List of card ui elements with setter functionality
    private List<AbilityCard> _abilityCards = new List<AbilityCard>();
    
    // Gamemode info
    private Gamemode _gamemode;
    
    // Quick Scene transitions
    private CustomSceneManager _sceneManager;
    

    private void Awake()
    {
        // Get gamemode reference
        _gamemode = GameObject.FindObjectOfType<Gamemode>();

        // Get the scene reference
        _sceneManager = GameObject.FindObjectOfType<CustomSceneManager>();
    }

    void Start()
    {
        // Populate the data of the 
        int abilityIndex = 0;
        foreach (var card in _cards)
        {
            // Set the ability data
            AbilityCard abilityCard = card.GetComponent<AbilityCard>();
            abilityCard.SetData(_abilities[abilityIndex]);

            // Check if player can buy, if not disable button
            int soulCurrency = _gamemode.SoulsToSpend;
            if (soulCurrency < _abilities[abilityIndex].upgradeCost)
            {
                abilityCard.DisableButton();
            }

            // Add current card to populated ability cards
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
                // Set the new chosen ability as the active ability
                _gamemode.AbilityData = ability.AbilityData;

                // Call next map
                _sceneManager.Play();
            }
        }

        // Display amount of souls player can spend.
        _soulPointTextElement.text = ((int)_gamemode.SoulsToSpend).ToString();
    }

    public void SkipBuy()
    {
        _sceneManager.Play();
    }
}
