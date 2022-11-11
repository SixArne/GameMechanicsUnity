using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatBillboard : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text _placeholder;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private float _appearTime;
    
    public void SetText(string message)
    {
        // Disable if running
        StopCoroutine("DisableCanvas");
        _canvas.enabled = true;
        _placeholder.SetText(message);
        StartCoroutine("DisableCanvas", _appearTime);
    }

    public void SetText(string message, float appearTime)
    {
        // Disable if running
        StopCoroutine("DisableCanvas");
        _canvas.enabled = true;
        _placeholder.SetText(message);
        StartCoroutine("DisableCanvas", appearTime);
    }

    private IEnumerator DisableCanvas(float appearTime = 2f)
    {
        yield return new WaitForSeconds(appearTime);
        _canvas.enabled = false;
    }
}
