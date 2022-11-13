using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// AbilityCard will fill in the data from a scriptable object
/// </summary>
public class AbilityCard : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text _title;
    [SerializeField] private TMPro.TMP_Text _description;
    [SerializeField] private TMPro.TMP_Text _cost;
    [SerializeField] private Button _button;
    private SoulUpgradeData _abilityData;

    private bool _isSelected = false;

    public bool IsSelected
    {
        get => _isSelected;
    }

    public SoulUpgradeData AbilityData
    {
        get => _abilityData;
    }

    public void SetData(SoulUpgradeData data)
    {
        _title.text = data.upgradeName;
        _description.text = data.upgradeDescription;
        _cost.text = data.upgradeCost.ToString();

        _abilityData = data;
    }

    public void Select()
    {
        _isSelected = true;
    }

    public void DisableButton()
    {
        _button.interactable = false;
    }
}
