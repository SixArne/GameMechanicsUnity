using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject _spawnPrefab;
    [SerializeField] private GameObject _spawnLocation;
    [SerializeField] private AgentCharacter _agentCharacter;
    [SerializeField] private BoxCollider _progressWall;
    [SerializeField] private ChatBillboard _chatbox;


    private string _runMessage = "I should run...";
    private string _followMessage = "Maybe I should lure them to a quit place next time...";
    private bool _hasRegisteredDeath = false;

    private void Update()
    {
        if (_agentCharacter.State == AgentCharacter.AgentState.Dead && !_hasRegisteredDeath)
        {
            GameObject reaper = Instantiate(_spawnPrefab, _spawnLocation.transform.position, Quaternion.identity);
            GrimReaper reaperLogic = reaper.GetComponent<GrimReaper>();
            reaperLogic.DeadAgent = _agentCharacter.gameObject;
            reaperLogic.State = GrimReaper.GrimState.Collecting;

            
            _progressWall.enabled = false;
            _hasRegisteredDeath = true;

            _chatbox.SetText(_runMessage);
            StartCoroutine("DisplayHelpMessage");
        }
    }

    private IEnumerator DisplayHelpMessage()
    {
        yield return new WaitForSeconds(2f);
        _chatbox.SetText(_followMessage);
    }
}
