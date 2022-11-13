using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmnesiaAbility : BasicAbility
{
    [SerializeField] float _blindDuration;

    // Needed to blind agents and reset awareness
    AwarenessManager _awarenessManager;

    public override void OnAttachPlayer()
    {
        base.OnAttachPlayer();

        // This only gets fetched at the beginning of each level once.
        _awarenessManager = GameObject.FindObjectOfType<AwarenessManager>();
    }

    public override void OnExecute()
    {
        _awarenessManager.MakeAllAgentsBlind(_blindDuration);
        _awarenessManager.ResetPublicAwareness();

        _playerCharacter.DiscardAbilityData();
    }
}
