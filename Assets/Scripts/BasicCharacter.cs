using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCharacter : MonoBehaviour
{

    protected MovementBehavior _movementBehavior = null;

    protected virtual void Awake()
    {
        _movementBehavior = GetComponent<MovementBehavior>();   
    }
}
