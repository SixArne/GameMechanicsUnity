using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoKill : MonoBehaviour
{
    [SerializeField] private float _lifetime = 5f;

    void Awake()
    {
        // This will kill the object once it's passed it's lifetime
        Invoke("Kill", _lifetime);
    }

    void Kill()
    {
        Destroy(gameObject);
    }
}
