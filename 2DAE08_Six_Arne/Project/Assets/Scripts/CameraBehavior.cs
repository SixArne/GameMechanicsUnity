using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    [SerializeField]
    private GameObject _player = null;

    [SerializeField]
    private float _followSpeed = 5.0f;

    void LateUpdate()
    {
        if (_player != null)
        {
            transform.position = Vector3.Lerp(transform.position, _player.transform.position, _followSpeed * Time.deltaTime);
        }
    }
}