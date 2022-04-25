using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DifficultyEvaluationData<T> 
{
	public TaskType.DifficultyEvaluation difficultyEvaluation;
	public List<T> datas;

	public DifficultyEvaluationData(TaskType.DifficultyEvaluation difficultyEvaluation, List<T> datas)
	{
		this.difficultyEvaluation = difficultyEvaluation;
		this.datas = datas;
	}
}
