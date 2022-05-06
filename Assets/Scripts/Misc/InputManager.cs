using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

// InputManager listens for keyboard input and calls the appropriate function.
public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Ball ball;

    private TrialsManager trialsManager;

    private DifficultyManager difficultyManager;

    private GlobalPauseHandler pauseHandler;

    private UiManager paddleGame;

    [SerializeField]
    private VideoControl videoControl;

    private void Start()
    {
        trialsManager = GetComponent<TrialsManager>();
        difficultyManager = GetComponent<DifficultyManager>();
        pauseHandler = GetComponent<GlobalPauseHandler>();
        paddleGame = GetComponent<UiManager>();
    }

    void Update()
    {
        ListenForInput();
#if UNITY_EDITOR
        ListenForInputDebug();
#endif
    }

    private void ListenForInput()
    {
        // Restart the ball from remote control
        if (SteamVR_Actions.default_GrabPinch.GetStateDown(SteamVR_Input_Sources.Any))
        {
            Debug.Log("Forcing restart.");
            StartCoroutine(ball.GetComponent<Ball>().RespawningCoroutine(pauseHandler));
        }

        // Toggle pause/resume state
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject.Find("[SteamVR]").GetComponent<GlobalPauseHandler>().TogglePause();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            paddleGame.QuitTask(trialsManager);
        }

        // Quit application
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            videoControl.ForceVideoEndingNow();
        }
    }

    private void ListenForInputDebug()
    {
        // Simulate ball bouncing
        if (Input.GetKeyDown(KeyCode.B))
        {
            ball.SimulateOnCollisionEnterWithPaddle(
                new Vector3(0, (float)0.5, 0),
                new Vector3(0, 1, 0)
            );
        }

        // Toggle debugger overlay
        if (Input.GetKeyDown(KeyCode.D))
        {
            GameObject.Find("Debugger Display").GetComponent<DebuggerDisplay>().ToggleDisplay();
        }

        // Simulate ball on floor
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Forcing restart.");
            StartCoroutine(ball.RespawningCoroutine(pauseHandler));
        }

        // Simulate enough bounces
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (GlobalControl.Instance.session == SessionType.Session.PRACTISE)
            {
                for (int i = 0; i < difficultyManager.nbOfBounceRequired * 7; i++)
                {
                    trialsManager.AddBounceToCurrentTrial();
                    trialsManager.AddAccurateBounceToCurrentTrial();
                }
            }
        }
    }
}
