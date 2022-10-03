using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] private float _spawnDistanceMax;
    [SerializeField] private float _spawnDistanceMin;
    [SerializeField] private float _spawnAmount;

    [SerializeField] [Range(0, 1)] private float _gizmosOpacity = 0.5f;

    void Awake()
    {
        NavMeshAgent navMeshComp = _prefab.GetComponent<NavMeshAgent>();

        if (navMeshComp == null)
            throw new UnityException("NO NAVMESH COMPONENT FOUND");
    }

    void Start()
    {
        for (int i = 0; i < _spawnAmount; i++)
        {
            GameObject agent = Instantiate(_prefab);
            Vector2 randomLocation = Random.insideUnitCircle;
            float randomDistance = Random.Range(_spawnDistanceMin, _spawnDistanceMax);

            agent.transform.position = new Vector3(
                transform.position.x + (randomLocation.x * randomDistance), 
                transform.position.y,
                transform.position.z + (randomLocation.y * randomDistance)
                );
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow.WithAlpha(_gizmosOpacity);
        Gizmos.DrawSphere(transform.position, _spawnDistanceMax);
    }
}
