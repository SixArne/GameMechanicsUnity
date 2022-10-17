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

    // Update is called once per frame
    void Kill()
    {
        Destroy(gameObject);
    }
}
