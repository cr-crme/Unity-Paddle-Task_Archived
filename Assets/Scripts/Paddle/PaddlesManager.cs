using System.Collections;
using UnityEngine;

public enum PaddleChoice { LEFT, BOTH, RIGHT };

public class PaddlesManager : MonoBehaviour
{
    [Tooltip("The left paddle in the game")]
    [SerializeField]
    private Paddle leftPaddle;

    [Tooltip("The right paddle in the game")]
    [SerializeField]
    private Paddle rightPaddle;

    private bool currentPaddleIsLeft = false;
    private PaddleChoice paddleChoice;
    public int NbPaddles { get { return GlobalPreferences.Instance.paddleChoice == PaddleChoice.BOTH ? 2 : 1; } }

    void Start()
    {
        paddleChoice = GlobalPreferences.Instance.paddleChoice;

        rightPaddle.EnablePaddle();
        leftPaddle.DisablePaddle();
        if (paddleChoice == PaddleChoice.LEFT)
        {
            rightPaddle.DisablePaddle();
            leftPaddle.EnablePaddle();
        }
    }

    public void SwitchPaddleIfNeeded(DifficultyManager _difficultyManager)
    {
        IEnumerator WaitThenSwitchPaddlesCoroutine()
        {
            // In order to prevent bugs, wait a little bit for the paddles to switch
            yield return new WaitForSeconds(0.1f);
            SwitchActivePaddle();
        }
        if (_difficultyManager.mustSwitchPaddleAfterHitting)
        {
            StartCoroutine(WaitThenSwitchPaddlesCoroutine());
        }
    }


    private void SwitchActivePaddle()
    {
        if (paddleChoice != PaddleChoice.BOTH)
            return;

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

    // Finds the currently active paddle
    public Paddle ActivePaddle
    {
        get { return currentPaddleIsLeft ? leftPaddle : rightPaddle; }
    }
}
