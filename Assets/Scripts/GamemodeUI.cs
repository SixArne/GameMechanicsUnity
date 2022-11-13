using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamemodeUI : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The placeholder in the UI where to display the quota as a number.")]
    private TMPro.TMP_Text _quotaPlaceholder;

    [SerializeField]
    [Tooltip("The placeholder in the UI where to display the quota as a number.")]
    private TMPro.TMP_Text _totalPlaceholder;

    private Gamemode _gamemode = null;

    private void Awake()
    {
        // We get the gamemode so we can display the data in the ui
        _gamemode = GameObject.FindObjectOfType<Gamemode>();
    }

    private void Update()
    {
        if (_gamemode.LevelHasEnded)
        {
            _quotaPlaceholder.text = 0.ToString();
        }   
        else
        {
            _quotaPlaceholder.text = _gamemode.LeftToKill.ToString();
        }
        
        _totalPlaceholder.text = _gamemode.TotalKills.ToString();
    }
}
