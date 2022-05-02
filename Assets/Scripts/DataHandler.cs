using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReadWriteCSV;
using System.IO;
using UnityEngine.Networking;

/// <summary>
/// Writes a line of data after every trial, giving information on the trial.
/// </summary>
public class DataHandler : MonoBehaviour
{
	// stores the data for writing to file at end of task
	List<DifficultyEvaluationData<TrialResults>> trialDatas = new List<DifficultyEvaluationData<TrialResults>>();
	//{
	//    { DifficultyEvaluation.BASE, new List<TrialData>() },
	//    { DifficultyEvaluation.MODERATE, new List<TrialData>() },
	//    { DifficultyEvaluation.MAXIMAL, new List<TrialData>() },
	//    { DifficultyEvaluation.CUSTOM, new List<TrialData>() },
	//};
	List<DifficultyEvaluationData<DifficultyData>> difficultyDatas = new List<DifficultyEvaluationData<DifficultyData>>();
	//{
	//	{ DifficultyEvaluation.BASE, new List<DifficultyData>() },
	//	{ DifficultyEvaluation.MODERATE, new List<DifficultyData>() },
	//	{ DifficultyEvaluation.MAXIMAL, new List<DifficultyData>() },
	//	{ DifficultyEvaluation.CUSTOM, new List<DifficultyData>() },
	//};

	HeaderData headerData;

	private string pid;

	int difficultyEvaluationIndex = -1;

	Dictionary<DifficultyChoice, int> evaluationsCount = new Dictionary<DifficultyChoice, int>();

	public bool dataWritten = false;

	public void InitializeDifficultyEvaluationData(DifficultyDefinition difficultyEvaluation)
	{
		trialDatas.Add(new DifficultyEvaluationData<TrialResults>(difficultyEvaluation, new List<TrialResults>()));
		difficultyDatas.Add(new DifficultyEvaluationData<DifficultyData>(difficultyEvaluation, new List<DifficultyData>()));
		difficultyEvaluationIndex++;
	}

	int GetEvaluationsIteration(DifficultyDefinition difficultyEvaluation)
	{
		//int evaluation = 0;
		//if (!evaluationsCount.ContainsKey(difficultyEvaluation))
		//{
		//	evaluation = 1;
		//	evaluationsCount.Add(difficultyEvaluation, 1);
		//}
		//else
		//{
		//	evaluationsCount[difficultyEvaluation]++;
		//	evaluation = evaluationsCount[difficultyEvaluation];
		//}

		//return evaluation;
		// TODO: CHECK THIS
		return 0;
	}

	void ResetEvaluationsIteration()
	{
		evaluationsCount = new Dictionary<DifficultyChoice, int>();
	}


	/// <summary>
	/// Write all data to files
	/// </summary>
	public void WriteDataToFiles()
	{
		if (dataWritten)
		{
			Debug.Log("Data already written, skipping...");
			return;
		}

		dataWritten = true;
		// make pid folder unique
		System.DateTime now = System.DateTime.Now;
		pid = GlobalControl.Instance.participantID + "_" + now.Month.ToString() + "-" + now.Day.ToString() + "-" + now.Year + "_" + now.Hour + "-" + now.Minute + "-" + now.Second; // + "_" + pid;

		// TODO: If should write the file
		WriteDifficultyFile();
	}

    // Records trial data into the data list
    public void recordTrial(float degreesOfFreedom, float time, float trialTime, int trialNum, int numBounces, int numAccurateBounces, DifficultyChoice difficultyEvaluation, int difficulty)
	{
		trialDatas[difficultyEvaluationIndex].datas.Add(new TrialResults(time, numBounces, numAccurateBounces));
	}


	public void recordDifficulty(float ballSpeed, bool targetLineActive, float targetLineHeightOffset, float targetLineWidth, float time, int difficulty)
	{
		difficultyDatas[difficultyEvaluationIndex].datas.Add(new DifficultyData(ballSpeed, targetLineActive, targetLineHeightOffset, targetLineWidth, time, difficulty));
	}

	public void recordHeaderInfo(SessionType.Session s, int maxtime, float htime, float twidth)
	{
		headerData = new HeaderData(s, maxtime, htime, twidth);
	}

	private void WriteHeaderInfo(CsvFileWriter writer)
	{
		CsvRow s = new CsvRow();
		s.Add("Session");
		s.Add(headerData.session.ToString());

		CsvRow maxtime = new CsvRow();
		maxtime.Add("Time Limit (s)");
		maxtime.Add(headerData.maxTrialTimeMin.ToString());

		CsvRow hovertime = new CsvRow();
		hovertime.Add("Ball Hover Time (s)");
		hovertime.Add(headerData.hoverTime.ToString());

		CsvRow trad = new CsvRow();
		trad.Add("Target Line Acceptance Width (m)");
		trad.Add(headerData.targetWidth.ToString());

		writer.WriteRow(s);
		writer.WriteRow(maxtime);
		writer.WriteRow(hovertime);
		writer.WriteRow(trad);
		writer.WriteRow(new CsvRow());
	}


	/// <summary>
	/// Writes the Difficulty file to a CSV
	/// </summary>
	private void WriteDifficultyFile()
	{
		// Write all entries in data list to file
		string directory = "Data/" + pid;
		Directory.CreateDirectory(@directory);

		if (difficultyDatas.Count <= 0) return;

		using (CsvFileWriter writer = new CsvFileWriter(@directory + "/" + "DifficultyData_" + pid + ".csv"))
		{
			Debug.Log("Writing diffiuclty data to file");

			// write session data
			WriteHeaderInfo(writer);

			// write header
			CsvRow header = new CsvRow();
			header.Add("Difficulty Evaluation");
			header.Add("Difficulty");
			header.Add("Participant ID");
			header.Add("Timestamp");
			header.Add("Ball Speed");
			header.Add("Target Line Active");
			header.Add("Target Line Height Offset");
			header.Add("Target Line Width");

			writer.WriteRow(header);

			foreach (var difficultyData in difficultyDatas)
			{
				// var evaluation = GetEvaluationsIteration(difficultyData.difficultyEvaluation);

				// write each line of data
				foreach (DifficultyData d in difficultyData.datas)
				{
					CsvRow row = new CsvRow();

					row.Add(difficultyData.difficultyEvaluation.ToString());
					row.Add(d.difficulty.ToString());
					row.Add(pid);
					row.Add(d.time.ToString());
					row.Add(d.ballSpeed.ToString());
					row.Add(d.targetLineActive.ToString());
					row.Add(d.targetLineHeightOffset.ToString());
					row.Add(d.targetLineWidth.ToString());

					writer.WriteRow(row);
				}
			}
		}
		// ResetEvaluationsIteration();
	}

	// utility functions --------------------------------------------

}
