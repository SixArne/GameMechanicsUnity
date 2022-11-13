using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicAbility : MonoBehaviour
{
    protected PlayerCharacter _playerCharacter;
    protected GrimReaper _grimReaper;

    public virtual void OnAttachPlayer() 
    {
        // Values that are used often are found and cached in the base class.
        _playerCharacter = GameObject.FindObjectOfType<PlayerCharacter>();
        _grimReaper = GameObject.FindObjectOfType<GrimReaper>();
    }

    public virtual void OnExecute() { }
}
