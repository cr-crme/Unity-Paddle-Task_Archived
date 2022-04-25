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

    GlobalControl globalControl;
    // Start is called before the first frame update
    void Start()
    {
        globalControl = GlobalControl.Instance;

        if (globalControl.numPaddles > 1)
        {
            rightPaddle.EnablePaddle();
            rightPaddle.SetPaddleIdentifier(Paddle.PaddleIdentifier.RIGHT);

            leftPaddle.EnablePaddle();
            leftPaddle.SetPaddleIdentifier(Paddle.PaddleIdentifier.LEFT);
        }
    }


    // In order to prevent bugs, wait a little bit for the paddles to switch
    public IEnumerator WaitThenSwitchPaddles()
    {
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
