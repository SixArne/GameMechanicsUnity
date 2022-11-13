using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OutplayedAbility : BasicAbility
{
    [SerializeField] private ParticleSystem _teleportParticle;
    List<AgentCharacter> _agentCharacters = new List<AgentCharacter>();

    public override void OnAttachPlayer()
    {
        base.OnAttachPlayer();

        // These could have died or be null, so we have to check at execution time if
        // they are valid.
        _agentCharacters = GameObject.FindObjectsOfType<AgentCharacter>().ToList();
    }

    // Ability specific execution
    public override void OnExecute()
    {
        List<AgentCharacter> agents = _agentCharacters.Where(a => a && a.State != AgentCharacter.AgentState.Dead).ToList();

        if (agents.Count == 0)
        {
            // In case there are no agents... well player is screwed
            return;
        }

        // Pick a random player and teleport to it
        int randomAgent = Random.Range(0, agents.Count);
        AgentCharacter agent = agents[randomAgent];
        _playerCharacter.transform.position = agent.transform.position;

        _playerCharacter.DiscardAbilityData(); // Cleanup ability info.

        Instantiate(_teleportParticle, _playerCharacter.transform.position, Quaternion.identity);
    }
}
