using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddlesManager : MonoBehaviour
{
    [Tooltip("The left paddle in the game")]
    [SerializeField]
    private Paddle leftPaddle;

    [Tooltip("The right paddle in the game")]
    [SerializeField]
    private Paddle rightPaddle;

    bool currentPaddleIsLeft = false;
    public int NbPaddles { get { return globalControl.nbPaddles; } }

    GlobalControl globalControl;

    void Start()
    {
        globalControl = GlobalControl.Instance;
        leftPaddle = GetComponent<Paddle>();
        rightPaddle = GetComponent<Paddle>();

        if (globalControl.nbPaddles > 1)
        {
            rightPaddle.EnablePaddle();
            rightPaddle.SetPaddleIdentifier(Paddle.PaddleIdentifier.RIGHT);

            leftPaddle.EnablePaddle();
            leftPaddle.SetPaddleIdentifier(Paddle.PaddleIdentifier.LEFT);
        }
    }


    public IEnumerator WaitThenSwitchPaddles()
    {
        // In order to prevent bugs, wait a little bit for the paddles to switch
        yield return new WaitForSeconds(0.1f);
        SwitchActivePaddle();
    }

    public void SwitchActivePaddle()
    {
        if (currentPaddleIsLeft)
        {
            leftPaddle.DisablePaddle();
            rightPaddle.EnablePaddle();
        }
        else
        {
            leftPaddle.EnablePaddle();
            rightPaddle.DisablePaddle();
        }
        currentPaddleIsLeft = !currentPaddleIsLeft;
    }

    // Finds the currently active paddle (in the case of two paddles)
    public Paddle ActivePaddle
    {
        get { return currentPaddleIsLeft ? leftPaddle : rightPaddle; }
    }
}
