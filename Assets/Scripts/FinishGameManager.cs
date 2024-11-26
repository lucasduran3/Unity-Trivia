using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishGameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _resultText;

    private void Start()
    {
        var result = GameManager.Instance.currentGameResult;

        if (result == GameManager.GameResult.Win)
        {
            _resultText.text = "¡Ganaste!";
        } 
        else
        {
            _resultText.text = "¡Respuesta Incorrecta!";
        }
    }

    public void RestartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DestroyInstance();
        }
        if (UIManagment.Instance != null)
        {
            UIManagment.Instance.DestroyInstance();
        }
        SceneManager.LoadScene("TriviaSelectScene");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
