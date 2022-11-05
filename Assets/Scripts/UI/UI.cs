using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text _text;
    [SerializeField] private AwarenessManager _awarenessManager;
    [SerializeField] private Image _image; 

    private float a = 0;

    void Update()
    {
        UpdateScore();
        UpdateAwareness();
    }

    void UpdateScore()
    {
        a += Time.deltaTime;
        _text.text = ((int)a).ToString();
    }

    void UpdateAwareness()
    {
        float size = 0f;

        switch (_awarenessManager.Level)
        {
            case AwarenessManager.AwarenessLevel.Normal:
                size = 0 + _awarenessManager.PublicAwareness / 100f;
                break;
            case AwarenessManager.AwarenessLevel.Alerted:
                size = 0.3f + _awarenessManager.PublicAwareness / 100f;
                break;
            case AwarenessManager.AwarenessLevel.HighAlert:
                size = 0.6f + _awarenessManager.PublicAwareness / 100f;
                break;
            case AwarenessManager.AwarenessLevel.Elimination:
                size = 0.9f + _awarenessManager.PublicAwareness / 100f;
                break;
        }

        _image.transform.localScale = new Vector3(Mathf.Clamp(size, 0.0f, 1.0f), 1f, 1f);
    }


}
