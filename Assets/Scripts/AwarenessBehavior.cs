using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Responsable for the logic of showing the interact menu above agents
/// </summary>
public class AwarenessBehavior : MonoBehaviour
{
    [Header("Image sources")]
    [SerializeField] private Image _killImage;
    [SerializeField] private Image _followImage;

    // Pannel to hide
    [SerializeField] private GameObject _UIPannel;

    private bool _canInteract = false;
    private bool _canKill = true;
    private bool _isFollowing = false;
    private bool _isDead = false;
    private bool _canFollow = true;

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

    public bool CanFollow
    {
        get => _canFollow;
        set => _canFollow = value;
    }

    void Update()
    {
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

        if (_isDead || _isFollowing || !_canFollow)
        {
            _followImage.color = new Color(1, 1, 1, .5f);
        }
        else if (!_isFollowing && _canInteract)
        {
            _followImage.color = new Color(1, 1, 1, 1f);
        }
    }
}
