using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOnDeath : MonoBehaviour
{
    [SerializeField] private GameObject _spawnPrefab;
    [SerializeField] private GameObject _spawnLocation;

    void OnDestroy()
    {
        Instantiate(_spawnPrefab, _spawnLocation.transform.position, Quaternion.identity);    
    }
}
