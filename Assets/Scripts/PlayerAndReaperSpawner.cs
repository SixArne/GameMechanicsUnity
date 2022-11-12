using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerAndReaperSpawner : MonoBehaviour
{
    [SerializeField] private List<Transform> _reaperSpawnPoints;
    [SerializeField] private List<Transform> _playerSpawnPoints;

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _reaperPrefab;
    
    void Awake()
    {
        int reaperSpawnIndex = Random.Range(0, _reaperSpawnPoints.Count);
        int playerSpawnIndex = Random.Range(0, _playerSpawnPoints.Count);

        Vector3 reaperSpawnLocation = _reaperSpawnPoints[reaperSpawnIndex].position;
        Vector3 playerSpawnLocation = _playerSpawnPoints[playerSpawnIndex].position;

        // Instantiate player
        Instantiate(_playerPrefab, playerSpawnLocation, Quaternion.identity);

        NavMeshHit navMeshHit;
        if (NavMesh.SamplePosition(reaperSpawnLocation, out navMeshHit, 100f, NavMesh.AllAreas))
        {
            reaperSpawnLocation = navMeshHit.position;
        }

        // Instantiate and setup reaper
        GameObject reaper = Instantiate(_reaperPrefab, reaperSpawnLocation, Quaternion.identity);
        GrimReaper reaperLogic = reaper.GetComponent<GrimReaper>();
        reaperLogic.State = GrimReaper.GrimState.Chasing;

       
    }
}
