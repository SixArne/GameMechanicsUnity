using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCharacter : MonoBehaviour
{
    protected MovementBehavior _movementBehavior;
    protected AbilityBehavior _abilityBehavior;
    protected virtual void Awake()
    {
        _abilityBehavior = GetComponent<AbilityBehavior>();
        _movementBehavior = GetComponent<MovementBehavior>();
    }
}
