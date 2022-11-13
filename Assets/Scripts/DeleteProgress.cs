using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteProgress : MonoBehaviour
{
    void Start()
    {
        // Reset gamemode data.
        Gamemode gamemode = GameObject.FindObjectOfType<Gamemode>();
        gamemode.Reset();
    }
}
