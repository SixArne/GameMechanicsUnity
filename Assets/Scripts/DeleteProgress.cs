using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteProgress : MonoBehaviour
{
    void Start()
    {
        // Delete old gamemode and remake
        Gamemode gamemode = GameObject.FindObjectOfType<Gamemode>();
        gamemode.Reset();
    }
}
