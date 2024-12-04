using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuSceneManager : MonoBehaviour
{   
    public void StartScene(string name)
    {
        GameManager.Instance.StartScene(name);
    }

    public void ExitGame()
    {
        GameManager.Instance.ExitGame();
    }
}
