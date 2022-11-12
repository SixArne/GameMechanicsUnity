using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityCard : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text _title;
    [SerializeField] private TMPro.TMP_Text _description;
    [SerializeField] private TMPro.TMP_Text _cost;
    [SerializeField] private Button _button;
    private BasicAbility _ability;

    private bool _isSelected = false;
    private int _abilityIndex = 0;

    public bool IsSelected
    {
        get => _isSelected;
    }

    public int AbilityIndex
    {
        get => _abilityIndex;
    }

    public void SetData(string title, string description, string cost, BasicAbility ability, int abilityIndex)
    {
        _title.text = title;
        _description.text = description;
        _cost.text = cost;
        _ability = ability;
        _abilityIndex = abilityIndex;
    }

    public void Select()
    {
        Debug.Log("Selected");
        _isSelected = true;
    }

    public void DisableButton()
    {
        _button.interactable = false;
    }
}
