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
	HeaderData headerData;

	private string pid;

	int difficultyEvaluationIndex = -1;

	Dictionary<DifficultyChoice, int> evaluationsCount = new Dictionary<DifficultyChoice, int>();

	public bool dataWritten = false;

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



	// utility functions --------------------------------------------

}
