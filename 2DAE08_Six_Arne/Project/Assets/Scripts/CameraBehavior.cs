using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    [SerializeField]
    private GameObject _player = null;

    [SerializeField]
    private float _followSpeed = 5.0f;

    [SerializeField]
    private Vector3 _offset = Vector3.zero;

    void Start()
    {
        transform.position = _player.transform.position + _offset;
        transform.LookAt(_player.transform.position);    
    }

    void Update()
    {
        if (_player != null)
        {
            transform.position = Vector3.Lerp(transform.position, _player.transform.position + _offset, _followSpeed * Time.deltaTime);
        }
    }
}