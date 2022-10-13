using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class AwarenessBehavior : MonoBehaviour
{
    [SerializeField] private GameObject _killImage;
    [SerializeField] private GameObject _followImage;
    [SerializeField] private float _agentInteractRadius = 3f;
    [SerializeField] private float _crimeRayLength = 5f;
    [SerializeField] private bool _canKill = true;
    [SerializeField] private bool _canFollow = true;
    [SerializeField] private LayerMask _playerMask;

    private GameObject _player;
    private bool _canInteract = false;

    public bool CanFollow
    {
        get { return _canFollow; }
        set { _canFollow = value; }
    }

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Friendly");
    }

    // Update is called once per frame
    void Update()
    {
        if (!_player)
            return;

        _canInteract = false;

        if ((transform.position - _player.transform.position).sqrMagnitude <=
            _agentInteractRadius * _agentInteractRadius)
        {
            _canInteract = true;
        }

        _killImage.SetActive(_canKill && _canInteract);
        _followImage.SetActive(_canFollow && _canInteract);

        DetectCrime();
    }

    void FixedUpdate()
    {

    }

    void DetectCrime()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, (_player.transform.position - transform.position).normalized);
        
        if (Physics.Raycast(ray, out hit, _crimeRayLength, _playerMask))
        {
            //Debug.Log("hit");
        }
        else
        {
            
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 0, 0.4f);

        Gizmos.DrawSphere(transform.position, _agentInteractRadius);
        
        if (!_player)
            return;

        Gizmos.color = Color.blue;
        Vector3 origin = transform.position;
        Vector3 target = _player.transform.position;

        Gizmos.DrawLine(origin, origin + ((target - origin).normalized * _crimeRayLength));

        //Gizmos.DrawRay(TrackingOriginModeFlags  * _crimeRayLength);
    }
}
