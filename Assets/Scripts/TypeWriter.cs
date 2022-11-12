using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TypeWriter : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float _typeSpeed = 0.2f;
    [SerializeField] private float _lineDelay = 2.0f;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private List<TMPro.TMP_Text> _lineHoldersText = new List<TMPro.TMP_Text>();
    [SerializeField] private CustomSceneManager _sceneManager;

    private List<string> _lines = new List<string>();

    private bool _hasFinishedTyping = false;
    private int _currentLine = 0;
    private int _currentChar = 0;

    private float _currentCharTime = 0f;
    private float _currentLineTime = 0f;

    private bool _isWaitingLine = false;

    void Start()
    {
        foreach (var l in _lineHoldersText)
        {
            _lines.Add(l.text);
            l.text = "";
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_isWaitingLine)
        {
            _currentLineTime += Time.deltaTime;

            if (_currentLineTime >= _lineDelay)
            {
                _isWaitingLine = false;
                _currentLineTime = 0f;

                _lineHoldersText[_currentLine].text = "";
                _currentLine++;
                _currentChar = 0;

                if (_currentLine > _lines.Count - 1)
                {
                    _hasFinishedTyping = true;
                }
            }
        }
        else if (!_hasFinishedTyping)
        {
            _currentCharTime += Time.deltaTime;

            if (_currentCharTime >= _typeSpeed && !_isWaitingLine)
            {
                _currentCharTime = 0f;

                char currentChar = _lines[_currentLine][_currentChar];

                if (currentChar == '\n')
                {
                    _isWaitingLine = true;

                    return;
                }
                else
                {
                    _lineHoldersText[_currentLine].text += currentChar;
                    _currentChar++;
                }
            }
        }
        else if (_hasFinishedTyping && !_isWaitingLine)
        {
            _sceneManager.IntroScene();
        }
    }
}
