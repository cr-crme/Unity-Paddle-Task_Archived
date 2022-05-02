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

	// utility functions --------------------------------------------

}
