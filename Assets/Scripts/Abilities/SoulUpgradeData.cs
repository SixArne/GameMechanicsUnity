using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Soul upgrades", menuName = "ScriptableObjects/Soul-Upgrades", order = 1)]
public class SoulUpgradeData : ScriptableObject
{
    public enum AbilityType // The game will check what type the ability is before executing to prevent wrongly executed abilities.
    {
        OnDead,
        OnAwareness,
        OnEndGame
    }

    public string upgradeName;
    [TextArea] public string upgradeDescription;
    public int upgradeCost;
    public AbilityType abilityType;
    public BasicAbility abilityScript;
}
