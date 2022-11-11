using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Rendering;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject _spawnPrefab;
    [SerializeField] private GameObject _spawnLocation;
    [SerializeField] private AgentCharacter _agentCharacter;
    [SerializeField] private BoxCollider _progressWall;
    [SerializeField] private ChatBillboard _chatbox;
    [SerializeField] private PlayerCharacter _playerCharacter;

    [Header("Message appear radia")]
    [SerializeField] private float _wallHelpMessageRadius = 5f;
    [SerializeField] private float _agentKillMessageRadius = 2f;

    private const string _runMessage = "I should run...";
    private const string _tooManyPeopleMessage = "To many people here... Need to lure him away";
    private const string _needToKillFirst = "Grim will be here soon... I need a distraction";
    private const string _perfectMessage = "This will do as a distraction...";
    private const string _followMessage = "Maybe I should lure them to a quit place next time...";
    private bool _hasRegisteredDeath = false;
    private bool _isInAgentRadius = false;

    private void Update()
    {
        float distanceToWallSquared = (_progressWall.transform.position - _playerCharacter.transform.position).sqrMagnitude;
        float wallRadiusSquared = _wallHelpMessageRadius * _wallHelpMessageRadius;
        if (_progressWall.enabled && distanceToWallSquared <= wallRadiusSquared)
        {
            _chatbox.SetText(_needToKillFirst);
        }

        float distanceToAgentSquared = (_agentCharacter.transform.position - _playerCharacter.transform.position).sqrMagnitude;
        float agentKillRadiusSquared = _agentKillMessageRadius * _agentKillMessageRadius;
        if (distanceToAgentSquared <= agentKillRadiusSquared &&
            _agentCharacter.State != AgentCharacter.AgentState.Dead &&
            !_isInAgentRadius)
        {
            _isInAgentRadius = true;
            _chatbox.SetText(_perfectMessage);
            StartCoroutine("DisplayHelpMessage", new object[3] { _tooManyPeopleMessage, 3f, 0.5f });
        }
        else if (distanceToAgentSquared > agentKillRadiusSquared)
        {
            _isInAgentRadius = false;
        }


        if (_agentCharacter.State == AgentCharacter.AgentState.Dead && !_hasRegisteredDeath)
        {
            GameObject reaper = Instantiate(_spawnPrefab, _spawnLocation.transform.position, Quaternion.identity);
            GrimReaper reaperLogic = reaper.GetComponent<GrimReaper>();
            reaperLogic.DeadAgent = _agentCharacter.gameObject;
            reaperLogic.State = GrimReaper.GrimState.Collecting;

            _progressWall.enabled = false;
            _hasRegisteredDeath = true;

            _chatbox.SetText(_runMessage, 1f);
            // message showtime and show delay
            StartCoroutine("DisplayHelpMessage", new object[3] { _followMessage, 3f, 0.5f });
        }
    }

    private IEnumerator DisplayHelpMessage(object[] parms)
    {
        string message = (string)parms[0];
        float appearTime = (float)parms[1];
        float messageDelay = (float)parms[2];

        yield return new WaitForSeconds(messageDelay);
        _chatbox.SetText(message, appearTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_progressWall.transform.position, _wallHelpMessageRadius);
        Gizmos.DrawWireSphere(_agentCharacter.transform.position, _agentKillMessageRadius);
    }
}
