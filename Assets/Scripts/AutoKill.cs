using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoKill : MonoBehaviour
{
    [SerializeField] private float _lifetime = 5f;

    void Awake()
    {
        Invoke("Kill", _lifetime);
    }

    void Kill()
    {
        Destroy(gameObject);
    }
}
