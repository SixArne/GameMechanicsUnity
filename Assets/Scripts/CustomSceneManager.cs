using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomSceneManager : MonoBehaviour
{
    const string _gameScene = "GameScene";
    const string _menuScene = "MainMenu";
    const string _introScene = "IntroScene";
    const string _storyScene = "StoryScene";
    const string _deathScene = "DeathMenu";
    const string _buyScene = "BuyScene";

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

    public void DeathScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(_deathScene);
    }

    public void BuyScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(_buyScene);
    }

    public Scene GetActiveScene()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene();
    }
}
