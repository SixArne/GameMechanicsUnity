using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Soul upgrades", menuName = "ScriptableObjects/Soul-Upgrades", order = 1)]
public class SoulUpgradeData : ScriptableObject
{
    public enum AbilityType
    {
        OnDead,
        OnAwareness,
    }

    public string upgradeName;
    [TextArea] public string upgradeDescriptionn;
    public int upgradeCost;
    public AbilityType abilityType;
    public BasicAbility abilityScript;
}
