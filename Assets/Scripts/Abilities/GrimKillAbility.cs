using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class GrimKillAbility : BasicAbility
{

    [SerializeField] private ParticleSystem _killReaperParticle;
    List<AgentCharacter> _agentCharacters = new List<AgentCharacter>();

    public override void OnAttachPlayer()
    {
        base.OnAttachPlayer();

        // These could have died or be null, so we have to check at execution time if
        // they are valid.
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
        Instantiate(_killReaperParticle, _grimReaper.transform.position, Quaternion.identity);
        //_grimReaper.enabled = false;
        Destroy(_grimReaper.gameObject);

        _playerCharacter.DiscardAbilityData(); // Cleanup ability info.
    }
}
