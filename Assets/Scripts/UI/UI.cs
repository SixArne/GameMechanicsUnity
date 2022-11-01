using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text _text;

    private float a = 0;

    // Update is called once per frame
    void Update()
    {
        a += Time.deltaTime;
        _text.text = ((int)a).ToString();
    }
}
