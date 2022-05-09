using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalPauseHandler : MonoBehaviour
{
    // The single instance of this class
    public static GlobalPauseHandler Instance;

    [SerializeField, Tooltip("The canvas for pausing")]
    public GameObject pauseIndicator;

    [SerializeField]
    private Ball ball;

    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // If any game object requested the indicator to be block (sort of mutex)
    int lockKey = -1;

    public bool isPaused { get; private set; } = false;

    public void TogglePause()
    {
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public int Pause(int _key = -1, bool _lock = false, bool _showCanvas = true)
    {
        if (!Unlock(_key))
        {
            Debug.Log("Pausing requested, but it is currently lock");
            return -1;
        }
        isPaused = true;
        Time.timeScale = 0;
        ball.TriggerPause();
        SetIndicatorVisibility(_showCanvas);
        return _lock ? Lock() : -1;
    }

    public int Resume(int _key = -1, bool _lock = false)
    {
        if (!Unlock(_key))
        {
            Debug.Log("Resuming requested, but it is currently lock");
            return -1;
        }
        isPaused = false;
        Time.timeScale = 1f;
        ball.TriggerResume();
        SetIndicatorVisibility(false);
        return _lock ? Lock() : -1;
    }

    private void SetIndicatorVisibility(bool _visible)
    {
        // If there is an activelock
        pauseIndicator.SetActive(_visible);
    }

    private bool CanUnlock(int _key)
    {
        if (lockKey < 0) return true;

        // Refuse the modification if the wrong key is provided
        if (_key != lockKey)
        {
            Debug.Log("Pause indicator requested, but it is currently lock");
            return false;
        }
        return true;
    }

    private bool Unlock(int _lockKey)
    {
        if (!CanUnlock(_lockKey)) return false;

        lockKey = -1;
        return true;
    }

    private int Lock()
    {
        if (lockKey >= 0) return -1;

        lockKey = Random.Range(1, 1000);
        return lockKey;
    }
}

