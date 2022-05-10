using System.Collections.Generic;

public class SaveTrialManager
{
    private FileIO_CSV.CsvFileWriter trial;
    private FileIO_CSV.CsvRow trialCsv;
    public SaveTrialManager()
    {
        trial = new FileIO_CSV.CsvFileWriter("result/Trial.csv");

        trialCsv = new FileIO_CSV.CsvRow(new List<string>() { "Time", "Ball position" });
        trial.Write(trialCsv);
    }

    public void AddFrameToTrial(Trial _trial, Ball _ball)
    {
        trialCsv.SetColumns(new List<string>() { _trial.time.ToString(), _ball.height.ToString() });
        trial.Write(trialCsv);
    }
    ~SaveTrialManager()
    {
        trial.Close();
    }
}
