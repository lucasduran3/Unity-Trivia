using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerController : MonoBehaviour
{
    [SerializeField] private int _duration = 10;
    private int _timer = 0;
    private Coroutine timerCoroutine;

    public static TimerController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } 
        else
        {
            Destroy(gameObject);
        }

        tag = "Timer";
    }
    public void StartTimer()
    {
        Debug.Log("Se deberia iniciar el timer");
        StopTimer();
        timerCoroutine = StartCoroutine(runTimerCoroutine());
    }

    public void StopTimer()
    {
        Debug.Log("Se detiene el timer");
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
    }

    private IEnumerator runTimerCoroutine()
    {
        Debug.Log("Se ejecuta el coroutine");
        _timer = 0;
        while (_timer <= _duration)
        {
            UIManagment.Instance.UpdateTimerText(_timer);
            yield return new WaitForSeconds(1);
            _timer++;
        }
        GameManager.Instance.EndGame(GameResult.LOSE_BY_TIMER);
    }

    public void DestroyInstance()
    {
        if (Instance == this)
        {
            Instance = null;
            Destroy(gameObject);
        }
    }

    public int Timer => _timer;
    public int Duration => _duration;
}
