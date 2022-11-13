using System.Collections;
using UnityEngine;

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
    private const string _tooManyPeopleMessage = "Better make sure nobody notices";
    private const string _needToKillFirst = "Grim will be here soon... I need a sacrifice";
    private const string _perfectMessage = "This will do as a sacrifice";
    private const string _followMessage = "As long as he can reap he is happy...";
    private const string _almostDone = "I should go before he's done";
    private bool _hasRegisteredDeath = false;
    private bool _isInAgentRadius = false;

    private void Update()
    {
        if (!_playerCharacter)
        {
            throw new UnityException("Player is gone??");
        }

        // Show help message if close to collider
        float distanceToWallSquared = (_progressWall.transform.position - _playerCharacter.transform.position).sqrMagnitude;
        float wallRadiusSquared = _wallHelpMessageRadius * _wallHelpMessageRadius;
        if (_progressWall.enabled && distanceToWallSquared <= wallRadiusSquared)
        {
            _chatbox.SetText(_needToKillFirst);
        }

        if (_agentCharacter)
        {
            // Display help messages if close to agent
            float distanceToAgentSquared = (_agentCharacter.transform.position - _playerCharacter.transform.position).sqrMagnitude;
            float agentKillRadiusSquared = _agentKillMessageRadius * _agentKillMessageRadius;
            if (distanceToAgentSquared <= agentKillRadiusSquared &&
                _agentCharacter.State != AgentCharacter.AgentState.Dead &&
                !_isInAgentRadius)
            {
                // bool used to ensure only 1 time use while close to agent.
                _isInAgentRadius = true;
                _chatbox.SetText(_perfectMessage);
                StartCoroutine("DisplayHelpMessage", new object[3] { _tooManyPeopleMessage, 3f, 1.0f });
            }
            else if (distanceToAgentSquared > agentKillRadiusSquared)
            {
                _isInAgentRadius = false;
            }
        }

        if (_agentCharacter.State == AgentCharacter.AgentState.Dead && !_hasRegisteredDeath)
        {
            // Spawn reaper on first kill
            GameObject reaper = Instantiate(_spawnPrefab, _spawnLocation.transform.position, Quaternion.identity);
            GrimReaper reaperLogic = reaper.GetComponent<GrimReaper>();
            reaperLogic.DeadAgent = _agentCharacter.gameObject;
            reaperLogic.State = GrimReaper.GrimState.Collecting;

            _progressWall.enabled = false;
            _hasRegisteredDeath = true;

            _chatbox.SetText(_runMessage, 1f);
            // message showtime and show delay
            StartCoroutine("DisplayHelpMessage", new object[3] { _followMessage, 2f, 1.5f });
            StartCoroutine("DisplayHelpMessage", new object[3] { _almostDone, 3f, 4.0f });
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
        if (!_agentCharacter)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_progressWall.transform.position, _wallHelpMessageRadius);
        Gizmos.DrawWireSphere(_agentCharacter.transform.position, _agentKillMessageRadius);
    }
}
