using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishGameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _resultText;

    private void Start()
    {
        var result = GameManager.Instance.currentGameResult;

        switch (result)
        {
            case GameResult.WIN:
                _resultText.text = "¡Ganaste!";
                break;

            case GameResult.LOSE_BY_ANSWER:
                _resultText.text = "¡Respuesta Incorrecta!";
                break;

            case GameResult.LOSE_BY_TIMER:
                _resultText.text = "¡Se termino el timepo!";
                break;
        }
    }

    public void RestartGame()
    {
        GameManager.Instance?.DestroyInstance();
        UIManagment.Instance?.DestroyInstance();
        TimerController.Instance?.DestroyInstance();

        SceneManager.LoadScene("MainMenu");
    }

    public void ExitGame()
    {
        Debug.Log(Time.realtimeSinceStartup);
        Application.Quit();
    }
}
