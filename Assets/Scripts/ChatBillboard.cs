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
        StartCoroutine("DisableCanvas");
    }

    private IEnumerator DisableCanvas()
    {
        yield return new WaitForSeconds(_appearTime);
        _canvas.enabled = false;
    }
}
