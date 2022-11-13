using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class GrimKillAbility : BasicAbility
{
    // Particle when grim dies
    [SerializeField] private ParticleSystem _killReaperParticle;

    List<AgentCharacter> _agentCharacters = new List<AgentCharacter>();

    public override void OnAttachPlayer()
    {
        base.OnAttachPlayer();

        // We gather all agentCharacters and then reduce them to 1 agent.
        // This ability is a timerace, grim vs the player, if the player gets a kill before grim gets the player
        // then the player will win
        _agentCharacters = GameObject.FindObjectsOfType<AgentCharacter>().ToList();

        while (_agentCharacters.Count != 1)
        {
            int randomAgent = Random.Range(0, _agentCharacters.Count);
            AgentCharacter agent = _agentCharacters[randomAgent];
            _agentCharacters.RemoveAt(randomAgent);
            Destroy(agent.gameObject);
        }

        _grimReaper.GetComponent<NavMeshAgent>().speed += 2f;
    }

    // Ability specific execution
    public override void OnExecute()
    {
        // Particles have AutoKill on them, no need to clean them up manually
        Instantiate(_killReaperParticle, _grimReaper.transform.position, Quaternion.identity);
        Destroy(_grimReaper.gameObject);

        _playerCharacter.DiscardAbilityData(); // Cleanup ability info.
    }
}
