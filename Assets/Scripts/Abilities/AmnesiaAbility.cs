using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmnesiaAbility : BasicAbility
{
    [SerializeField] float _blindDuration;

    AwarenessManager _awarenessManager;
    AudioSource _audioSource;

    public override void OnAttachPlayer()
    {
        base.OnAttachPlayer();

        _awarenessManager = GameObject.FindObjectOfType<AwarenessManager>();
    }

    // Ability specific execution
    public override void OnExecute()
    {
        _awarenessManager.MakeAllAgentsBlind(_blindDuration);
        _awarenessManager.ResetPublicAwareness();

        _playerCharacter.DiscardAbilityData();
    }
}
