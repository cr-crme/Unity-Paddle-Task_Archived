using System.Collections;
using UnityEngine;

public class BallColorManager : MonoBehaviour
{
    [Tooltip("The normal ball color")]
    [SerializeField]
    private Material ballMat;

    [Tooltip("The ball color when it is in the target height range")]
    [SerializeField]
    private Material greenBallMat;

    [Tooltip("Auxilliary color materials")]
    [SerializeField]
    private Material redBallMat;

    [Tooltip("Auxilliary color materials")]
    [SerializeField]
    private Material blueBallMat;

    // For Green/White IEnumerator coroutine 
    bool inTurnBallWhiteCR = false;

    public void IndicateSuccess()
    {
        TurnBallGreen();
        StartCoroutine(TurnBallWhiteCR(0.3f));
    }

    public void SetToNormalColor()
    {
        TurnBallWhite();
    }

    private void TurnBallGreen()
    {
        GetComponent<MeshRenderer>().material = greenBallMat;
    }

    private void TurnBallRed()
    {
        GetComponent<MeshRenderer>().material = redBallMat;
    }

    private void TurnBallBlue()
    {
        GetComponent<MeshRenderer>().material = blueBallMat;
    }

    private void TurnBallWhite()
    {
        GetComponent<MeshRenderer>().material = ballMat;
    }

    private IEnumerator TurnBallWhiteCR(float time = 0.0f)
    {
        if (inTurnBallWhiteCR)
        {
            yield break;
        }
        yield return new WaitForSeconds(time);
        inTurnBallWhiteCR = true;

        TurnBallWhite();

        inTurnBallWhiteCR = false;
    }

    private IEnumerator TurnBallGreenCR(float time = 0.0f)
    {
        yield return new WaitForSeconds(time);
        TurnBallGreen();
    }

}
