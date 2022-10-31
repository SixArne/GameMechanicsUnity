using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AwarenessBehavior : MonoBehaviour
{
    [SerializeField] private Image _killImage;
    [SerializeField] private Image _followImage;
    [SerializeField] private GameObject _UIPannel;
    [SerializeField] private float _agentInteractRadius = 5f;
    [SerializeField] private float _crimeRayLength = 5f;
    [SerializeField] private LayerMask _playerMask;
    [SerializeField] private Material _agentDeathMaterial;


    const string _playerTag = "Friendly";

    private GameObject _player;
    private bool _canInteract = false;
    private bool _canKill = true;
    private bool _isFollowing = false;
    private bool _isDead = false;

    public bool IsFollowing
    {
        get => _isFollowing;
        set => _isFollowing = value;
    }

    public bool CanKill
    {
        get => _canKill;
        set => _canKill = value;
    }

    public bool IsDead
    {
        get => _isDead;
        set => _isDead = value;
    }

    public bool CanInteract
    {
        get => _canInteract;
        set => _canInteract = value;
    }

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag(_playerTag);
    }

    void Update()
    {
        if (!_player)
            return;

        if (!_canInteract)
        {
            _UIPannel.SetActive(false);
        }
        else
        {
            _UIPannel.SetActive(true);
        }

        if (_isDead)
        {
            _UIPannel.SetActive(false);
        }

        if (!_canKill && _canInteract || _isDead)
        {
            _killImage.color = new Color(1, 0, 0, .5f);
        }
        else if (_canInteract)
        {
            _killImage.color = new Color(1, 1, 1, 1);
        }
        else
        {
            _killImage.color = new Color(1, 1, 1, 0);
        }

        if (_isDead || _isFollowing)
        {
            _followImage.color = new Color(1, 1, 1, .5f);
        }
        else if (!_isFollowing && _canInteract)
        {
            _followImage.color = new Color(1, 1, 1, 1f);
        }
        else
        {
            _followImage.color = new Color(1, 1, 1, 0);
        }

        DetectCrime();

        //_canInteract = false;
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
