using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagment : MonoBehaviour
{
    public static void PreviousScene()
    {
        UIManagment.Instance?.DestroyInstance();
        TimerController.Instance?.DestroyInstance();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public static void ChangeScene(string sceneName)
    {
        UIManagment.Instance?.DestroyInstance();
        TimerController.Instance?.DestroyInstance();

        SceneManager.LoadScene(sceneName);
    }

    public static void CloseWindow()
    {
        GameManager.Instance?.ExitGame();
    }
}
