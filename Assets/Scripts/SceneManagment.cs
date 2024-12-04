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
        /*UIManagment.Instance?.DestroyInstance();
        TimerController.Instance?.DestroyInstance();*/
    
        SceneManager.LoadScene(sceneName);
    }

    public void ChangeSceneFromEnd(string sceneName)
    {
        //System.Diagnostics.Process.Start(Application.dataPath.Replace("_Data", ".exe")); //new program Application.Quit();
        GameManager.Instance.StartTrivia();
        SceneManager.LoadScene(sceneName);
    }

    public static void CloseWindow()
    {
        GameManager.Instance?.ExitGame();
    }

    public void CloseWindowFromLogin()
    {
        Application.Quit();
    }
}
