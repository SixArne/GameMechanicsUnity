using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] [Range(0, 1000)] private float _spawnDistanceMax;
    [SerializeField] [Range(0, 100)] private float _spawnDistanceMin;
    [SerializeField] [Range(0, 100)] private int _spawnAmount;

    [SerializeField] [Range(0, 1)] private float _gizmosOpacity = 0.5f;

    void Awake()
    {
        NavMeshAgent navMeshComp = _prefab.GetComponent<NavMeshAgent>();

        if (navMeshComp == null)
            throw new UnityException("NO NAVMESH COMPONENT FOUND");

        SpawnEntities();
    }

    void SpawnEntities()
    {
        for (int i = 0; i < _spawnAmount; i++)
        {
            Vector2 randomLocation = Random.insideUnitCircle;
            float randomDistance = Random.Range(_spawnDistanceMin, _spawnDistanceMax);

            Vector3 spawnPosition = new Vector3(
                transform.position.x + (randomLocation.x * randomDistance), 
                transform.position.y,
                transform.position.z + (randomLocation.y * randomDistance)
                );

            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawnPosition, out hit, 100f, NavMesh.AllAreas)){
                Instantiate(_prefab, hit.position, Quaternion.identity);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 0, _gizmosOpacity);
        Gizmos.DrawSphere(transform.position, _spawnDistanceMax);
    }
}
