using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using System;
using TMPro;
using UnityEngine.SceneManagement;

public class PaddleGame : MonoBehaviour
{	
	private float _trialTimer;
	private bool _isInTrial;

	[Tooltip("The head mounted display")]
	[SerializeField]
	private GameObject hmd;

	[SerializeField]
	[Tooltip("The paddles in the game")]
	PaddlesManager paddlesManager;
	
	[Tooltip("The ball being bounced")]
	[SerializeField]
	private GameObject ball;

	[Tooltip("The line that denotes where the ball should be bounced ideally")]
	[SerializeField]
	private GameObject targetLine;

	[Tooltip("The canvas that displays score information to the user")]
	[SerializeField]
	private FeedbackCanvas feedbackCanvas;

	[Tooltip("A reference to the Time to Drop countdown display quad")]
	[SerializeField]
	private GameObject timeToDropQuad;

	[Tooltip("A reference to the Time to Drop countdown display text")]
	[SerializeField]
	private Text timeToDropText;

	[SerializeField]
	AudioClip feedbackExample;

	[SerializeField]
	AudioSource feedbackSource, difficultySource;

	[SerializeField]
	TextMeshPro difficultyDisplay;

	[SerializeField]
	TextMeshPro highestBouncesDisplay;

	[SerializeField]
	TextMeshPro highestAccurateBouncesDisplay;

	/// <summary>
	/// list of the audio clips played at the beginning of difficulties in some cases
	/// </summary>
	[SerializeField]
	List<DifficultyAudioClip> difficultyAudioClips;

	// Manage the current task to perform
	SessionManager sessionManager;

	// Current number of bounces that the player has acheieved in this trial
	private int numBounces = 0;
	private int numAccurateBounces = 0;
	// Current score during this trial
	private float curScore = 0f;

	// Running total number of bounces this instance
	private int numTotalBounces = 0;

	// The current trial number. This is increased by one every time the ball is reset.
	public int trialNum = 0;

	[SerializeField]
	private GlobalPauseHandler pauseHandler;

	// If 3 of the last 10 bounces were successful, update the exploration mode physics 
	private const int EXPLORATION_MAX_BOUNCES = 10;
	private const int EXPLORATION_SUCCESS_THRESHOLD = 6;
	private CircularBuffer<bool> explorationModeBuffer = new CircularBuffer<bool>(EXPLORATION_MAX_BOUNCES);

	// The paddle bounce height, velocity, and acceleration to be recorded on each bounce.
	// These are the values on the *paddle*, NOT the ball
	private float paddleBounceHeight;
	private Vector3 paddleBounceVelocity;
	private Vector3 paddleBounceAccel;

	// Degrees of freedom, how many degrees in x-z directions ball can bounce after hitting paddle
	// 0 degrees: ball can only bounce in y direction, 90 degrees: no reduction in range
	public float degreesOfFreedom;

	// This session information
	private TaskType.Condition condition;
	private TaskType.ExpCondition expCondition;
	private TaskType.Session session;
	private int maxTrialTime;


	// Variables for countdown timer display
	private bool inCoutdownCoroutine = false;

	// Timescale
	public bool slowtime = false;
	private List<float> bounceHeightList = new List<float>();

	private List<ScoreEffect> scoreEffects = new List<ScoreEffect>();
	
	private int difficultyEvaluationIndex = 0;

	int scoreEffectTarget = 0;
	bool maxScoreEffectReached = false;

	List<TrialCondition> trialConditions = new List<TrialCondition>();
	TrialCondition baseTrialCondition, moderateTrialCondition, maximaltrialCondition;

	float difficultyExampleTime = 30f;

	int highestBounces, highestAccurateBounces;

	GlobalControl globalControl;
	DataHandler dataHandler;

	void Start()
	{
		globalControl = GlobalControl.Instance;
		dataHandler = GetComponent<DataHandler>();

		sessionManager = new SessionManager(paddlesManager);

		Instantiate(globalControl.environments[globalControl.environmentOption]);

		// Calibrate the target line to be at the player's eye level
		SetTargetLineHeight(globalControl.targetLineHeightOffset);
		SetTargetLineWidth(globalControl.targetRadius);


		PopulateScoreEffects();

		//InitializeTrialConditions();

		if(globalControl.session == TaskType.Session.SHOWCASE)
		{
			globalControl.recordingData = false;
			globalControl.maxTrialTime = 0;
		}
	
		Initialize(true);

		SetTrialLevel(sessionManager.currentLevel);

		// difficulty shifts timescale, so pause it again
		Time.timeScale = 0;

		globalControl.ResetTimeElapsed();

		pauseHandler.Pause();


		// countdown for first drop
		// ResetTrial();
		// inRespawnMode = true;
	}

	void Update()
	{
		if (_isInTrial && !globalControl.paused)
		{
			_trialTimer += Time.unscaledDeltaTime;
		}

		if(GlobalControl.Instance.paused)
		{
			// no processing until unpaused
			return;
		}

		// Data handler. Record continuous ball & paddle info
		//GatherContinuousData();

		// Update Canvas display
		timeToDropQuad.SetActive(false);
		feedbackCanvas.UpdateScoreText(curScore, numBounces);

		// Record list of heights for bounce data analysis
		if (ball.GetComponent<Ball>().isBouncing)
		{
			bounceHeightList.Add(ball.transform.position.y);
		}

		// Reset time scale
		Time.timeScale = globalControl.timescale;

		// Reset ball if it drops 
		ManageIfBallOnGround();
		ManageHoveringPhase();

		// Check for inputs
		ManageInputs();


		if (sessionManager.isTimeOver(globalControl.GetTimeElapsed()))
		{
			Debug.Log(
				$"time elapsed {globalControl.GetTimeElapsed()} greater " +
                $"than max trial time {sessionManager.maximumTrialTime}"
			);
			sessionManager.EvaluatePerformance(globalControl.GetTimeElapsed());
			if (
				session == TaskType.Session.ACQUISITION 
				|| session == TaskType.Session.SHOWCASE 
				|| sessionManager.isSessionOver
			)
			{
				// over once end time is reached.
				QuitTask();
				return;
			}
		}
	}

	void ManageInputs()
    {
		// Actual game inputs
		if (SteamVR_Actions.default_GrabPinch.GetStateDown(SteamVR_Input_Sources.Any))
		{
			Debug.Log("Forcing restart.");
			StartCoroutine(ball.GetComponent<Ball>().Respawning(pauseHandler));
		}

#if UNITY_EDITOR
		// Debug related inputs
		if (Input.GetKeyDown(KeyCode.N))
		{
			globalControl.timescale = Mathf.Clamp(globalControl.timescale - .05f, .05f, 3f);
			Time.timeScale = globalControl.timescale;
			Debug.Log("reduced timescale to " + globalControl.timescale);
		}
		if (Input.GetKeyDown(KeyCode.M))
		{
			globalControl.timescale = Mathf.Clamp(globalControl.timescale + .05f, .05f, 3f);
			Time.timeScale = globalControl.timescale;
			Debug.Log("increased timescale to " + globalControl.timescale);

		}
		if (Input.GetKeyDown(KeyCode.U))
		{
			curScore -= 25f;
			BallBounced();
			Debug.Log("Score decreased");
		}
		if (Input.GetKeyDown(KeyCode.I))
		{
			curScore += 25f;
			BallBounced();
			Debug.Log("Score increased");
		}
		if (Input.GetKeyDown(KeyCode.Q))
		{
			QuitTask();
		}
		if (Input.GetKeyDown(KeyCode.P))
		{
			globalControl.timeElapsed += 60;
		}
		if (Input.GetKeyDown(KeyCode.J))
		{
		}
		if (Input.GetKeyDown(KeyCode.L))
		{
			if (session == TaskType.Session.BASELINE)
			{
				numBounces += sessionManager.nbOfBounceRequired * 7;
				numAccurateBounces += sessionManager.nbOfAccurateBounceRequired * 7;
			}
		}
		if (Input.GetKeyDown(KeyCode.B))
		{
			ball.GetComponent<Ball>().SimulateOnCollisionEnterWithPaddle(
				new Vector3(0, (float)1, 0),
				new Vector3(0, 1, 0)
			);
		}
		if (Input.GetKeyDown(KeyCode.R))
		{
			Debug.Log("Forcing restart.");
			StartCoroutine(ball.GetComponent<Ball>().Respawning(pauseHandler));
		}
#endif
	}

	void OnApplicationQuit()
	{
		QuitTask();
	}

	/// <summary>
	/// Stop the task, write data and return to the start screen
	/// </summary>
	void QuitTask()
	{
		// This is to ensure that the final trial is recorded.
		ResetTrial(true);

		dataHandler.WriteDataToFiles();

		// clean DDoL objects and return to the start scene
		Destroy(GlobalControl.Instance.gameObject);
		Destroy(gameObject);

		SceneManager.LoadScene(0);
	}

#region Initialization

	public void Initialize(bool firstTime)
	{
		if (globalControl.playVideo)
		{
			// Wait for end of video playback to initialize
			return;
		}

		dataHandler.dataWritten = false;
		// Initialize Condition and Visit types
		condition = globalControl.condition;
		expCondition = globalControl.expCondition;
		session = globalControl.session;
		maxTrialTime = globalControl.maxTrialTime;
		degreesOfFreedom = globalControl.degreesOfFreedom;

		//if (globalControl.recordingData)
		//{
		//	StartRecording();
		//}

		if (globalControl.targetHeightEnabled == false) targetLine.SetActive(false);

		ball.GetComponent<EffectController>().dissolve.effectTime = globalControl.ballResetHoverSeconds;
		ball.GetComponent<EffectController>().respawn.effectTime = globalControl.ballResetHoverSeconds;

		curScore = 0;

		if (globalControl.session == TaskType.Session.BASELINE)
		{
			difficultyDisplay.text = sessionManager.currentLevel.ToString();
			//trialData.Add(new DifficultyEvaluationData<TrialData>(
			//	difficultyEvaluationOrder[difficultyEvaluationIndex], new List<TrialData>())
			//);
			//dataHandler.InitializeDifficultyEvaluationData(
			//	difficultyEvaluationOrder[difficultyEvaluationIndex]
			//);

		}
		else if (globalControl.session == TaskType.Session.SHOWCASE)
		{
			sessionManager.currentLevel = 2;
			StartShowcase();
		}
		else
		{
			sessionManager.currentLevel = globalControl.difficulty;
			//trialData.Add(new DifficultyEvaluationData<TrialData>(TrialDifficultyPreset.CUSTOM, new List<TrialData>()));
			// TODO: CHECK NEXT LINE
			//dataHandler.InitializeDifficultyEvaluationData(DifficultyChoice.BASE);
			pauseHandler.Pause();
			// difficulty shifts timescale, so pause it again
			Time.timeScale = 0;
		}


		globalControl.ResetTimeElapsed();


		highestBounces = 0;
		highestAccurateBounces = 0;
		UpdateHighestBounceDisplay();
		feedbackCanvas.UpdateScoreText(curScore, numBounces);

        // ensure drop time on first drop
		if (firstTime)
        {
			ball.GetComponent<EffectController>().StopAllParticleEffects();
		}
        else
        {
			StartCoroutine(ball.GetComponent<Ball>().Respawning(pauseHandler));
		}

		Debug.Log("Initialized");
		if (session == TaskType.Session.BASELINE)
		{
			Debug.Log("Evaluating trial difficulty index: " + difficultyEvaluationIndex);
		}
	}

	// Sets Target Line height based on HMD eye level and target position preference
	public void SetTargetLineHeight(float offset)
	{
		Vector3 tlPosn = targetLine.transform.position;

		float x = tlPosn.x;
		float z = tlPosn.z;
		float y = ApplyInstanceTargetHeightPref(GetHmdHeight()) + offset;

		targetLine.transform.position = new Vector3(x, y, z);

		// Update Exploration Mode height calibration
		GetComponent<ExplorationMode>().CalibrateEyeLevel(targetLine.transform.position.y);

		var kinematics = ball.GetComponent<Kinematics>();
		if (kinematics)
		{
			kinematics.storedPosition = Ball.spawnPosition(targetLine);
		}
		// ball.transform.position = Ball.spawnPosition(targetLine);
	}

	private void SetTargetLineWidth(float width)
    {
		targetLine.transform.localScale = new Vector3(targetLine.transform.localScale.x, globalControl.targetRadius * 2f, targetLine.transform.localScale.z);
	}

	private void toggleTargetLine()
    {
		if (globalControl.targetHeightEnabled)
		{
			targetLine.SetActive(true);
		}
		else
		{
			targetLine.SetActive(false);
		}
	}

	private float ApplyInstanceTargetHeightPref(float y)
	{
		switch (globalControl.targetHeightPreference)
		{
			case TaskType.TargetHeight.RAISED:
				y *= 1.1f;
				break;
			case TaskType.TargetHeight.LOWERED:
				y *= 0.9f;
				break;
			case TaskType.TargetHeight.DEFAULT:
				break;
			default:
				Debug.Log("Error: Invalid Target Height Preference");
				break;
		}
		return y;
	}

	/// <summary>
	/// note that score exists as a vestige. it is tracked internally to allow for these effects but will not be shown to the user
	/// </summary>
	private void PopulateScoreEffects()
	{
		// enter score effects in ascending order of the score needed to trigger them
		scoreEffects.Add(new ScoreEffect(25, ball.GetComponent<EffectController>().embers, null));
		scoreEffects.Add(new ScoreEffect(50, ball.GetComponent<EffectController>().fire, null));
		scoreEffects.Add(new ScoreEffect(75, ball.GetComponent<EffectController>().blueEmbers, null, new List<Effect>() { ball.GetComponent<EffectController>().embers }));
		scoreEffects.Add(new ScoreEffect(100, ball.GetComponent<EffectController>().blueFire, null, new List<Effect>() { ball.GetComponent<EffectController>().fire }));


		int highestScore = 0;
		for(int i = 0; i < scoreEffects.Count; i++)
		{
			if (scoreEffects[i].score > highestScore)
			{
				highestScore = scoreEffects[i].score;
			}
			else
			{
				// could create a sorting algorithm but it's a bit more work to deal with the custom class. dev input in the correct order will be sufficient
				Debug.LogErrorFormat("ERROR! Invalid Score order entered, must be in ascending order. Entry {0} had score {1}, lower than the minimum {2}", i, scoreEffects[i], highestScore);
			}
		}

		scoreEffectTarget = 0;
	}

	///// <summary>
	///// Set up the conditions in which conditions or feedback would be triggered. a function is created for each condition to evaluate the data and will return the number of true conditions
	///// </summary>
	//void InitializeTrialConditions()
	//{
	//	// difficulty conditions
	//	baseTrialCondition = new TrialCondition(7, 10, false, feedbackExample, (TrialData trialData) => 
	//	{
	//		if (trialData.numBounces >= targetConditionBounces[TrialDifficultyPreset.BASE])
	//		{
	//			return trialData.numBounces / targetConditionBounces[TrialDifficultyPreset.BASE];
	//		}
	//		return 0; 
	//	});
	//	moderateTrialCondition = new TrialCondition(7, 10, false, feedbackExample, (TrialData trialData) => 
	//	{ 
	//		if (trialData.numBounces >= targetConditionBounces[TrialDifficultyPreset.MODERATE] && (!globalControl.targetHeightEnabled || trialData.numAccurateBounces >= targetConditionAccurateBounces[TrialDifficultyPreset.MODERATE])) 
	//		{
	//			int bounces = trialData.numBounces / targetConditionBounces[TrialDifficultyPreset.MODERATE];
	//			int accurateBounces = trialData.numAccurateBounces / targetConditionAccurateBounces[TrialDifficultyPreset.MODERATE];
	//			return !globalControl.targetHeightEnabled ? bounces : accurateBounces; 
	//		} 
	//		return 0; 
	//	});
	//	maximaltrialCondition = new TrialCondition(7, 10, false, feedbackExample, (TrialData trialData) => 
	//	{
	//		if (trialData.numBounces >= targetConditionBounces[TrialDifficultyPreset.MAXIMAL])
	//		{
	//			return trialData.numAccurateBounces / targetConditionBounces[TrialDifficultyPreset.MAXIMAL];
	//		}
	//		return 0; 
	//	});

	//	// feedback conditions
	//	trialConditions.Add(new TrialCondition(5, 5, true, feedbackExample, (TrialData trialData) => { if (trialData.numBounces < 0) { return 1; } return 0; }));
	//}

	/// <summary>
	/// run through all diffiuclties in a short amount of time to get a feel for them
	/// </summary>
	void StartShowcase()
	{
		pauseHandler.Resume();
		SetTrialLevel(sessionManager.currentLevel);
		StartCoroutine(StartDifficultyDelayed(difficultyExampleTime, true));
	}

	IEnumerator StartDifficultyDelayed(float delay, bool initial = false)
	{
		if (initial)
		{
			// wait until after the pause is lifted, when timescale is 0
			yield return new WaitForSeconds(.1f);
		}

		var audioClip = GetDifficiultyAudioClip(sessionManager.currentLevel);
		if (audioClip != null)
		{
			difficultySource.PlayOneShot(audioClip);
		}
		Debug.Log("playing difficulty audio " + (audioClip != null ? audioClip.name : "null"));

		yield return new WaitForSecondsRealtime(delay);

		// reset ball, change difficulty level, possible audio announcement.
		if (sessionManager.currentLevel >= 10)
		{
			// finish up the difficulty showcase, quit application
			QuitTask();
		}
		else
		{
			SetTrialLevel(sessionManager.currentLevel + 2);
			StartCoroutine(StartDifficultyDelayed(difficultyExampleTime));
			if (sessionManager.currentLevel > 10) // OG ==
			{
				// yield return new WaitForSecondsRealtime(delay);
			}
		}

		ball.GetComponent<Ball>().ResetBall();
	}

#endregion // Initialization

#region Reset Trial

	// Holds the ball over the paddle at Target Height for 0.5 seconds, then releases
	void ManageHoveringPhase()
	{
		if (!ball.GetComponent<Ball>().inHoverMode)
			return;

		timeToDropQuad.SetActive(true);

		ball.GetComponent<SphereCollider>().enabled = false;

		// Hover ball at target line for a second
		StartCoroutine(ball.GetComponent<Ball>().PlayDropSound(globalControl.ballResetHoverSeconds - 0.15f));
		StartCoroutine(ball.GetComponent<Ball>().ReleaseHoverOnReset(globalControl.ballResetHoverSeconds));

		// Start countdown timer 
		StartCoroutine(UpdateTimeToDropDisplay());

		ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
		ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		ball.transform.position = Ball.spawnPosition(targetLine);
		ball.transform.rotation = Quaternion.identity;

		Time.timeScale = 1f;
		//Debug.Log("Entering hover mode");
	}

	void ManageIfBallOnGround()
    {
		if (ball.GetComponent<Ball>().inHoverMode) 
			return;

		if (ball.GetComponent<Ball>().inRespawnMode)
			return;         
		
		// Check if ball is on ground
		if (ball.GetComponent<Ball>().isOnGround())
		{
			ResetTrial();
			_isInTrial = true;
			_trialTimer = 0;
		}
	}


	// Update time to drop
	IEnumerator UpdateTimeToDropDisplay()
	{
		if (inCoutdownCoroutine)
		{
			yield break;
		}
		inCoutdownCoroutine = true;

		int countdown = globalControl.ballResetHoverSeconds;

		while (countdown >= 1.0f)
		{
			timeToDropText.text = countdown.ToString();
			countdown--;
			yield return new WaitForSeconds(1.0f);
		}

		inCoutdownCoroutine = false;
	}

	// The ball was reset after hitting the ground. Reset bounce and score.
	public void ResetTrial(bool final = false)
	{
		// Don't run this code the first time the ball is reset or when there are 0 bounces
		//if (trialNum < 1 /*|| numBounces < 1*/)
		//{
		//	trialNum++;
		//	CheckEndCondition();
		//	return;
		//}

		//if (!_isInTrial)
		//	return;

		//_isInTrial = false;

		//// Record data for final bounce in trial
		//GatherBounceData();

		//if (globalControl.recordingData)
		//{
		//	// Record Trial Data from last trial
		//	dataHandler.recordTrial(degreesOfFreedom, Time.time, _trialTimer, trialNum, numBounces, numAccurateBounces, sessionManager.currentDifficulty, difficulty);
		//	// CheckDifficulty();
		//	trialData[difficultyEvaluationIndex].datas.Add(new TrialData(degreesOfFreedom, Time.time, _trialTimer, trialNum, numBounces, numAccurateBounces, difficulty));

		//}

		if (!final && trialNum != 0 && trialNum % 10 == 0)
		{
			// some difficulty effects are regenerated every 10 trials
			SetTrialLevel(sessionManager.currentLevel);
		}
			

		if (numBounces > highestBounces)
		{
			highestBounces = numBounces;
		}
		if (numAccurateBounces > highestAccurateBounces)
		{
			highestAccurateBounces = numAccurateBounces;
		}

		UpdateHighestBounceDisplay();

		trialNum++;
		numBounces = 0;
		numAccurateBounces = 0;
		curScore = 0f;
		scoreEffectTarget = 0;
		maxScoreEffectReached = false;

		if (!final)
		{
			// Check if game should end or evaluation set change
			CheckEndCondition();
			Initialize(false);
		}
	}

#endregion // Reset

#region Checks, Interactions, Data

	// This will be called when the ball successfully bounces on the paddle.
	public void BallBounced()
	{
		//if (numBounces > 0)
		//{
		//	GatherBounceData();
		//}
		//SetUpPaddleData();
		numBounces++;
		numTotalBounces++;

		// If there are two paddles, switch the active one
		if (sessionManager.mustSwitchPaddleAfterHitting)
		{
			StartCoroutine(paddlesManager.WaitThenSwitchPaddles());
		}

		if (!maxScoreEffectReached && curScore >= scoreEffects[scoreEffectTarget].score)
		{
			Debug.Log(
				$"max score for effects not reached. current score " +
                $"{curScore} is greater than the target {scoreEffectTarget} for effect " +
                $"{scoreEffects[scoreEffectTarget].score}. startign effects, increasing " +
                $"score target..."
			);

			foreach (var disableEffect in scoreEffects[scoreEffectTarget].disableEffects)
			{
				ball.GetComponent<EffectController>().StopParticleEffect(disableEffect);
			}
			ball.GetComponent<EffectController>().StartEffect(scoreEffects[scoreEffectTarget].effect);
			ball.GetComponent<BallSoundPlayer>().PlayEffectSound(scoreEffects[scoreEffectTarget].audioClip);

			if (scoreEffectTarget + 1 >= scoreEffects.Count)
			{
				maxScoreEffectReached = true;
				Debug.Log($"max effect score reached, score is {curScore} and score target was {scoreEffects[scoreEffectTarget].score}");
			}
			else
			{
				scoreEffectTarget++;
			}
		}

		CheckEndCondition(true);
	}

	/// <summary>
	/// if the evaluation does not end, chacks all other conditions
	/// </summary>
	void CheckEndCondition(bool fromBounce = false)
	{
		if(session == TaskType.Session.SHOWCASE)
		{
			return;
		}

 		if (CheckScoreCondition())
		{
			sessionManager.EvaluatePerformance(globalControl.GetTimeElapsed());
			if (sessionManager.isSessionOver)
            {
				QuitTask();
				return;
			}
		}
		else
		{
			CheckTrialConditions(fromBounce);
		}
	}

	void CheckTrialConditions(bool fromBounce = false)
	{
		foreach(var trialCondition in trialConditions)
		{
			CheckCondition(trialCondition, fromBounce);
		}
	}

	bool CheckScoreCondition()
	{
		//TrialCondition difficultyCondition = GetDifficultyCondition(sessionManager.currentDifficultyChoice);
		//return CheckCondition(difficultyCondition);
		return true;
	}

	/// <summary>
	/// evaluate the set of recent bouces in the trial and recent trials. will determine how many true conditions there are/>
	/// </summary>
	/// <param name="trialCondition"></param>
	/// <param name="fromBounce"></param>
	/// <returns></returns>
	bool CheckCondition(TrialCondition trialCondition, bool fromBounce = false)
	{
		return true;
		//var datas = new List<TrialResults>(trialData[difficultyEvaluationIndex].datas);

		//if (fromBounce)
		//{
		//	// add data from current set in progress
		//	datas.Add(new TrialData(degreesOfFreedom, Time.time, _trialTimer, trialNum, numBounces, numAccurateBounces, sessionManager.currentLevel));
		//}

		//if (trialCondition.trialEvaluationCooldown > 0)
		//{
		//	trialCondition.trialEvaluationCooldown--;
		//	return false;
		//}

		//int trueCount = 0;

		//// check for true conditions in recent data  
 	//	for(int i = datas.Count - 1; i >= datas.Count - (trialCondition.trialEvaluationsSet < datas.Count ? trialCondition.trialEvaluationsSet : datas.Count) && i >= 0; i--)
		//{
		//	int trueInTrial = trialCondition.checkTrialCondition(datas[i]);
		//	if (trueInTrial > 0)
		//	{
		//		trueCount += trueInTrial;
		//	}
		//	else if (trialCondition.sequential)
		//	{
		//		trueCount = 0;
		//	}

		//	if (trueCount >= trialCondition.trialEvaluationTarget)
		//	{
		//		// successful condition
		//		if (trialCondition.conditionFeedback != null)
		//		{	
		//			feedbackSource.PlayOneShot(trialCondition.conditionFeedback);
		//		}
		//		trialCondition.trialEvaluationCooldown = trialCondition.trialEvaluationsSet;
		//		return true;
		//	}
		//}
		//return false;
	}

	void UpdateHighestBounceDisplay()
	{
		string bounces = highestBounces.ToString();
		highestBouncesDisplay.text = String.Format("{0} bounces in a row!", bounces);
	
		if (targetLine.activeInHierarchy)
		{
			string accurateBounces = highestAccurateBounces.ToString();
			highestAccurateBouncesDisplay.text = String.Format("{0} target hits!", accurateBounces);
		}
		else
		{
			highestAccurateBouncesDisplay.text = "";
		}
	}

	private float GetHmdHeight()
	{
		return hmd.transform.position.y;
	}

	// Returns true if the ball is within the target line boundaries.
	public bool GetHeightInsideTargetWindow(float height)
	{
		if (!globalControl.targetHeightEnabled) return false;

		float targetHeight = targetLine.transform.position.y;
		float lowerLimit = targetHeight - globalControl.targetRadius;
		float upperLimit = targetHeight + globalControl.targetRadius;

		return (height > lowerLimit) && (height < upperLimit);
	}

	public TrialCondition GetDifficultyCondition(DifficultyChoice evaluation)
	{
		if (evaluation == DifficultyChoice.BASE)
		{
			return baseTrialCondition;
		}
		else if (evaluation == DifficultyChoice.MODERATE)
		{
			return moderateTrialCondition;
		}
		else if (evaluation == DifficultyChoice.MAXIMAL)
		{
			return maximaltrialCondition;
		}

		return null;
	}

	public int GetMaxDifficultyTrialTime()
	{
		int trialTime = (int)sessionManager.maximumTrialTime;
		return trialTime != -1 ? trialTime * 60 : trialTime;
	}

	private AudioClip GetDifficiultyAudioClip(int difficulty)
	{
		foreach(var difficultyAudioClip in difficultyAudioClips)
		{
			if(difficultyAudioClip.difficulty == difficulty)
			{
				return difficultyAudioClip.audioClip;
			}
		}
		return null;
	}

	//#region Gathering and recording data

	//	public void StartRecording()
	//	{
	//		// Record session data
	//		dataHandler.recordHeaderInfo(
	//			condition, expCondition, session, maxTrialTime, globalControl.ballResetHoverSeconds, globalControl.targetRadius
	//		);
	//	}

	//	// Determine data for recording a bounce and finally, record it.
	//	private void GatherBounceData()
	//	{
	//		float apexHeight = Mathf.Max(bounceHeightList.ToArray());
	//		float apexTargetError = globalControl.targetHeightEnabled ? (apexHeight - targetLine.transform.position.y) : 0;

	//		bool apexSuccess = globalControl.targetHeightEnabled ? GetHeightInsideTargetWindow(apexHeight) : true;

	//		// If the apex of the bounce was inside the target window, increase the score
	//		if (apexSuccess)
	//		{
	//			curScore += 10;
	//			if (trialManager.hasTarget)
	//			{
	//				numAccurateBounces++;
	//			}

	//			ball.GetComponent<Ball>().IndicateSuccessBall(); // temporariliy disabled while testing apex coroutines in Ball
	//		}

	//		//Record Data from last bounce
	//		Vector3 cbm = ball.GetComponent<Ball>().GetBounceModification();

	//		if (globalControl.recordingData)
	//		{
	//			dataHandler.recordBounce(degreesOfFreedom, Time.time, cbm, trialNum, numBounces, numTotalBounces, apexTargetError, apexSuccess, paddleBounceVelocity, paddleBounceAccel, sessionManager.currentDifficulty);

	//		}

	//		bounceHeightList = new List<float>();
	//	}

	//	// Grab ball and paddle info and record it. Should be called once per frame
	//	private void GatherContinuousData()
	//	{
	//		Paddle paddle = paddlesManager.ActivePaddle;
	//		Vector3 ballVelocity = ball.GetComponent<Rigidbody>().velocity;
	//		Vector3 paddleVelocity = paddle.Velocity;
	//		Vector3 paddleAccel = paddle.Acceleration;

	//		Vector3 cbm = ball.GetComponent<Ball>().GetBounceModification();

	//		if (globalControl.recordingData)
	//		{
	//			dataHandler.recordContinuous(degreesOfFreedom, Time.time, cbm, globalControl.paused, ballVelocity, paddleVelocity, paddleAccel, sessionManager.currentDifficulty);
	//		}
	//	}
	//	// Initialize paddle information to be recorded upon next bounce
	//	private void SetUpPaddleData()
	//	{
	//		Paddle paddle = paddlesManager.ActivePaddle;

	//		paddleBounceHeight = paddle.Position.y;
	//		paddleBounceVelocity = paddle.Velocity;
	//		paddleBounceAccel = paddle.Acceleration;
	//	}

	//	#endregion // Gathering and recording data

	#endregion // Checks, Interactions, Data

	#region Difficulty
	void EvaluatePerformance()
    {
		double _score = sessionManager.EvaluatePerformance(globalControl.GetTimeElapsed());

		// each are evaluating for the next difficulty
		int _newLevel = sessionManager.ScoreToLevel(_score);

		SetTrialLevel(_newLevel);

		// reset cooldown, condition may be used again
		//GetDifficultyCondition(sessionManager.currentDifficulty).trialEvaluationCooldown = 0;

		difficultyEvaluationIndex++;
		Debug.Log(
			$"Increased Difficulty Evaluation to {difficultyEvaluationIndex} with new difficulty " +
			$"evaluation difficulty evaluation: {sessionManager.difficultyName}"
		);
	}
	private void SetTrialLevel(int _newLevel)
    {
		sessionManager.currentLevel = _newLevel;

		// TODO: This should be done by GlobalControl itself
		Debug.Log("Setting Difficulty: " + sessionManager.currentLevel);
		GlobalControl globalControl = GlobalControl.Instance;
		globalControl.targetHeightEnabled = sessionManager.hasTarget;
		globalControl.targetLineHeightOffset = sessionManager.targetHeightOffset;
		globalControl.targetRadius = globalControl.targetHeightEnabled ? sessionManager.targetWidth / 2f : 0;
		globalControl.timescale = sessionManager.ballSpeed;

		// Reset trial
		numBounces = 0;
		numAccurateBounces = 0;
		curScore = 0f;
		scoreEffectTarget = 0;
		maxScoreEffectReached = false;

		toggleTargetLine();
		SetTargetLineHeight(sessionManager.targetHeightOffset);
		SetTargetLineWidth(globalControl.targetRadius);
		difficultyDisplay.text = sessionManager.currentLevel.ToString();

		// record difficulty values change
		if (globalControl.recordingData)
		{
			// TODO: FIX THAT
			//dataHandler.recordDifficulty(
			//	sessionManager.ballSpeed, 
			//	sessionManager.hasTarget, 
			//	sessionManager.targetHeightOffset,
			//	sessionManager.targetWidth, 
			//	Time.time, 
			//	sessionManager.currentLevel
			//);
		}

		if (sessionManager.isSessionOver)
		{
			// all difficulties recored
			QuitTask();
		}
	}

#endregion // Difficulty

#region Exploration Mode

	// Toggles the timescale to make the game slower 
	public void ToggleTimescale()
	{
		slowtime = !slowtime;

		if (slowtime)
		{
			Time.timeScale = globalControl.timescale; // 0.7f;
		}
		else
		{
			Time.timeScale = 1.0f;
		}
	}

	// If 6 of the last 10 bounces were successful, update ExplorationMode physics 
	// bool parameter is whether last bounce was success 
	public void ModifyPhysicsOnSuccess(bool bounceSuccess)
	{
		if (globalControl.explorationMode != GlobalControl.ExplorationMode.FORCED)
		{
			return;
		}

		explorationModeBuffer.Add(bounceSuccess);

		int successes = 0;

		bool[] temp = explorationModeBuffer.GetArray();
		for (int i = 0; i < explorationModeBuffer.length(); i++)
		{
			if (temp[i])
			{
				successes++;
			}
		}

		if (successes >= EXPLORATION_SUCCESS_THRESHOLD)
		{
			// Change game physics
			GetComponent<ExplorationMode>().ModifyBouncePhysics();
			GetComponent<ExplorationMode>().IndicatePhysicsChange();

			// Reset counter
			explorationModeBuffer = new CircularBuffer<bool>(EXPLORATION_MAX_BOUNCES);
			return;
		}
	}
#endregion // Exploration Mode

}
