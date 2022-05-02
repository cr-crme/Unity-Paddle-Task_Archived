using UnityEngine;

public class TrialCondition
{	
	public IsTrialSuccessfulEvaluator checkTrialCondition;

	public TrialCondition(IsTrialSuccessfulEvaluator checkTrialConditionVar)
	{
		checkTrialCondition = checkTrialConditionVar;
	}
}
