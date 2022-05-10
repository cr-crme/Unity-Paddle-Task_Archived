using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveTrialManager : MonoBehaviour
{
    private string resultsFolder;

    private FileIO_CSV.CsvFileWriter trial;
    private FileIO_CSV.CsvRow trialCsv;
    public SaveTrialManager()
    {
        // Create a results directory if it does not exist
        resultsFolder = $"Results/{GlobalPreferences.Instance.participantID}";
        Directory.CreateDirectory(resultsFolder);

        // Prepare the Trial csv file
        trial = new FileIO_CSV.CsvFileWriter($"{resultsFolder}/Trial.csv");
        trialCsv = new FileIO_CSV.CsvRow(new List<string>() { "Time", "Ball position"});
        trial.Write(trialCsv + "\n");
    }

    public void AddFrameToTrial(Trial _trial, Ball _ball)
    {
        trialCsv.SetColumns(new List<string>() { _trial.time.ToString(), _ball.height.ToString() });
        string tata = _trial.time.ToString();
        trial.Write(trialCsv.ToString() + "\n");
    }
    void OnDestroy()
    {
        trial.Close();
    }
}
