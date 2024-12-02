using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerController : MonoBehaviour
{
    private int _timer = 10;
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
        StopTimer();
        timerCoroutine = StartCoroutine(runTimerCoroutine());
    }

    public void StopTimer()
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
    }

    private IEnumerator runTimerCoroutine()
    {
        _timer = 10;
        while (_timer >= 0)
        {
            UIManagment.Instance.UpdateTimerText(_timer);
            yield return new WaitForSeconds(1);
            _timer--;
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
    public int TimeToRespond => 10 - _timer;
}
