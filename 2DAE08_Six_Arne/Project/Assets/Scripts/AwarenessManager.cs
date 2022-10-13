using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AwarenessManager : MonoBehaviour
{
    public enum AwarenessLevel
    {
        Normal = 0,
        Alerted = 10,
        HighAlert = 20,
        Elimination = 30
    }

    [SerializeField] private float _publicAwareness = 0f;
    [SerializeField] private AwarenessLevel _level = AwarenessLevel.Normal;
    [SerializeField] private GameObject _player = null;

    void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Friendly");
    }

    public AwarenessLevel Level
    {
        get => _level;
    }

    public void IncreaseAwareness()
    {
        _publicAwareness += 1;
    }

    public void DecreaseAwareness(int amount)
    {
        _publicAwareness -= amount;
    }

    private void Update()
    {
        if (_publicAwareness % 10 >= 1)
        {
            _level = (AwarenessLevel)Mathf.Clamp((int)_level++, 0, 3);
        }
    }
}
