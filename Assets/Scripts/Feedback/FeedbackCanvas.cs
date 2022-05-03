using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackCanvas : MonoBehaviour {

    [SerializeField]
    private TextMeshPro currentNumberOfBouncesText;
    string initialCurrentNumberOfBounces;

    [SerializeField]
    TextMeshPro bestSoFarText;
    string initialBestSoFar;

    [SerializeField]
    private TextMeshPro numberOfTargetHitsText;
    string initialNumberOfTargetHits;

    [SerializeField]
    private DifficultyManager difficultyManager;

    void Awake() 
    {
        initialCurrentNumberOfBounces = currentNumberOfBouncesText.text;
        initialBestSoFar = bestSoFarText.text;
        initialNumberOfTargetHits = numberOfTargetHitsText.text;
    }

    public void UpdateAllInformation(TrialsManager _trialsManager)
    {
        UpdateCurrentNumberOfBounces(_trialsManager.currentNumberOfBounces);
        UpdateBestSoFar(_trialsManager.bestSoFarNbOfBounces);
        UpdateTargetHits(_trialsManager.currentNumberOfAccurateBounces);
    }

    public void UpdateCurrentNumberOfBounces(int _nbBounces)
    {
        currentNumberOfBouncesText.text = string.Format(initialCurrentNumberOfBounces, _nbBounces);
    }

    public void UpdateBestSoFar(int _bestSoFar)
    {
        bestSoFarText.text = string.Format(initialBestSoFar, _bestSoFar);
    }

    public void UpdateTargetHits(int _nbTargetHits) 
    { 
        if (difficultyManager.hasTarget)
        {
            numberOfTargetHitsText.text = string.Format(initialNumberOfTargetHits, _nbTargetHits);
        }
        else
        {
            numberOfTargetHitsText.text = "";
        }
    }
}
