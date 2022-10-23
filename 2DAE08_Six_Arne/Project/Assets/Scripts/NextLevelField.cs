using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextLevelField : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private SceneManager _sceneManager;
    [SerializeField] private string _levelToLoad;

    const string _playerTag = "Friendly";

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_playerTag))
        {
            Invoke("NextZone", 0.2f);
        }
    }

    private void NextZone()
    {
        _sceneManager.LoadScene(_levelToLoad);
    }
}
