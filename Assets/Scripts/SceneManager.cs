using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    const string _gameScene = "GameScene";
    const string _menuScene = "MainMenu";
    const string _introScene = "IntroScene";
    const string _storyScene = "StoryScene";

    public void Play()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(_gameScene);
    }

    public void MenuScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(_menuScene);
    }

    public void IntroScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(_introScene);
    }

    public void StoryScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(_storyScene);
    }

    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
