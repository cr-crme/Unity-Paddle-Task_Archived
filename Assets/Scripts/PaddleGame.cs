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
	private const int difficultyMin = 1, difficultyMax = 10;
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

	[Tooltip("The radius of the target line area. Example: If this is 0.05, the target line will be 0.10 thick")]
	[SerializeField]
	private float targetRadius = 0.05f; // get from GlobalControl

	[Tooltip("A reference to the Time to Drop countdown display quad")]
	[SerializeField]
	private GameObject timeToDropQuad;

	[Tooltip("A reference to the Time to Drop countdown display text")]
	[SerializeField]
	private Text timeToDropText;

	[SerializeField, Tooltip("Handles the ball sound effects")]
	private BallSoundPlayer ballSoundPlayer;

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
	TaskManager taskManager;

	// Current number of bounces that the player has acheieved in this trial
	private int numBounces = 0;
	private int numAccurateBounces = 0;
	// Current score during this trial
	private float curScore = 0f;

	// Running total number of bounces this instance
	private int numTotalBounces = 0;

	// The current trial number. This is increased by one every time the ball is reset.
	public int trialNum = 0;

	public EffectController effectController;

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
	private TaskType.DifficultyEvaluation difficultyEvaluation;
	private int maxTrialTime;
	private float hoverTime;

	// Variables to keep track of resetting the ball after dropping to the ground
	private bool inHoverMode = false;
	private bool inHoverResetCoroutine = false;
	private bool inPlayDropSoundRoutine = false;
	private int ballResetHoverSeconds = 3;
	private bool inRespawnMode = false;
	private int ballRespawnSeconds = 1;

	// Variables for countdown timer display
	public int countdown;
	private bool inCoutdownCoroutine = false;

	// Timescale
	public bool slowtime = false;
	private List<float> bounceHeightList = new List<float>();

	int difficultyEvaluationTrials;
	private List<ScoreEffect> scoreEffects = new List<ScoreEffect>();
	private List<TaskType.DifficultyEvaluation> difficultyEvaluationOrder = new List<TaskType.DifficultyEvaluation>() 
	{
		TaskType.DifficultyEvaluation.BASE,
		TaskType.DifficultyEvaluation.MODERATE,
		TaskType.DifficultyEvaluation.MAXIMAL,
		TaskType.DifficultyEvaluation.MODERATE 
	};
	private int difficultyEvaluationIndex = 0;

	int scoreEffectTarget = 0;
	bool maxScoreEffectReached = false;

	int difficulty;
	List<TrialCondition> trialConditions = new List<TrialCondition>();
	TrialCondition baseTrialCondition, moderateTrialCondition, maximaltrialCondition;
	private List<DifficultyEvaluationData<TrialData>> trialData = new List<DifficultyEvaluationData<TrialData>>();
	// {
		//{ DifficultyEvaluation.BASE, new List<TrialData>() },
		//{ DifficultyEvaluation.MODERATE, new List<TrialData>() },
		//{ DifficultyEvaluation.MAXIMAL, new List<TrialData>() },
		//{ DifficultyEvaluation.CUSTOM, new List<TrialData>() }
	// };

	private Dictionary<TaskType.DifficultyEvaluation, int> targetConditionBounces = new Dictionary<TaskType.DifficultyEvaluation, int>()
	{
		{ TaskType.DifficultyEvaluation.BASE, 5 },
		{ TaskType.DifficultyEvaluation.MODERATE, 5 },
		{ TaskType.DifficultyEvaluation.MAXIMAL, 5 },
		{ TaskType.DifficultyEvaluation.CUSTOM, -1 }
	};
	private Dictionary<TaskType.DifficultyEvaluation, int> targetConditionAccurateBounces = new Dictionary<TaskType.DifficultyEvaluation, int>()
	{
		{ TaskType.DifficultyEvaluation.BASE, 0 },
		{ TaskType.DifficultyEvaluation.MODERATE, 5 },
		{ TaskType.DifficultyEvaluation.MAXIMAL, 5 },
		{ TaskType.DifficultyEvaluation.CUSTOM, -1 }
	};

	private List<int> performanceDifficulties = new List<int>();

	float difficultyExampleTime = 30f;

	int highestBounces, highestAccurateBounces;

	GlobalControl globalControl;
	DataHandler dataHandler;

	void Start()
	{
		globalControl = GlobalControl.Instance;
		dataHandler = GetComponent<DataHandler>();

		taskManager = new TaskManager(true);

		difficultyEvaluationTrials = globalControl.difficultyEvaluationTrials;

		Instantiate(globalControl.environments[globalControl.environmentOption]);

		if(globalControl.session == TaskType.Session.BASELINE)
		{
			performanceDifficulties.Add(1);
		}
		else
		{
			performanceDifficulties.Add(globalControl.difficulty);
		}

		// Calibrate the target line to be at the player's eye level
		SetTargetLineHeight(globalControl.targetLineHeightOffset);
		targetRadius = globalControl.targetHeightEnabled ? globalControl.targetRadius : 0f;


		PopulateScoreEffects();

		InitializeTrialConditions();

		if(globalControl.session == TaskType.Session.SHOWCASE)
		{
			globalControl.recordingData = false;
			globalControl.maxTrialTime = 0;
		}
	
		Initialize(true);

		SetDifficulty(difficulty);
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
		GatherContinuousData();

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


		if (globalControl.GetTimeElapsed() > GetMaxDifficultyTrialTime(difficultyEvaluation) /*globalControl.GetTimeLimitSeconds()*/)
		{
			Debug.Log(
				$"time elapsed {globalControl.GetTimeElapsed()} greater " +
                $"than max trial time {GetMaxDifficultyTrialTime(difficultyEvaluation)}"
			);
			EvaluateDifficultyResult(false);
		}
	}

	void ManageInputs()
    {
		// Actual game inputs
		if (SteamVR_Actions.default_GrabPinch.GetStateDown(SteamVR_Input_Sources.Any))
		{
			Debug.Log("Forcing restart.");
			StartCoroutine(Respawning());
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
				numBounces += targetConditionBounces[difficultyEvaluation] * 7;
				numAccurateBounces += targetConditionBounces[difficultyEvaluation] * 7;
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
			StartCoroutine(Respawning());
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
		hoverTime = globalControl.ballResetHoverSeconds;
		degreesOfFreedom = globalControl.degreesOfFreedom;
		ballResetHoverSeconds = globalControl.ballResetHoverSeconds;

		if (globalControl.recordingData)
		{
			StartRecording();
		}

		if (globalControl.targetHeightEnabled == false) targetLine.SetActive(false);

		effectController.dissolve.effectTime = ballRespawnSeconds;
		effectController.respawn.effectTime = hoverTime;

		curScore = 0;

		if (globalControl.session == TaskType.Session.BASELINE)
		{
			difficultyEvaluation = difficultyEvaluationOrder[difficultyEvaluationIndex];
			difficulty = GetDifficulty(difficultyEvaluationOrder[difficultyEvaluationIndex]);
#if UNITY_EDITOR
			// difficulty = 10;
#endif
			difficultyDisplay.text = difficulty.ToString();
			trialData.Add(new DifficultyEvaluationData<TrialData>(difficultyEvaluationOrder[difficultyEvaluationIndex], new List<TrialData>()));
			dataHandler.InitializeDifficultyEvaluationData(difficultyEvaluationOrder[difficultyEvaluationIndex]);

		}
		else if (globalControl.session == TaskType.Session.SHOWCASE)
		{
			difficulty = 2;
			StartShowcase();
		}
		else
		{
			difficulty = globalControl.difficulty;
			trialData.Add(new DifficultyEvaluationData<TrialData>(TaskType.DifficultyEvaluation.CUSTOM, new List<TrialData>()));
			dataHandler.InitializeDifficultyEvaluationData(TaskType.DifficultyEvaluation.CUSTOM);
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
			inHoverMode = true;
			effectController.StopAllParticleEffects();
		}
        else
        {
			StartCoroutine(Respawning());
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
		scoreEffects.Add(new ScoreEffect(25, effectController.embers, null));
		scoreEffects.Add(new ScoreEffect(50, effectController.fire, null));
		scoreEffects.Add(new ScoreEffect(75, effectController.blueEmbers, null, new List<Effect>() { effectController.embers }));
		scoreEffects.Add(new ScoreEffect(100, effectController.blueFire, null, new List<Effect>() { effectController.fire }));


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

	/// <summary>
	/// Set up the conditions in which conditions or feedback would be triggered. a function is created for each condition to evaluate the data and will return the number of true conditions
	/// </summary>
	void InitializeTrialConditions()
	{
		// difficulty conditions
		baseTrialCondition = new TrialCondition(7, 10, false, feedbackExample, (TrialData trialData) => 
		{
//#if UNITY_EDITOR
//			if (false)
//#else
			if (trialData.numBounces >= targetConditionBounces[TaskType.DifficultyEvaluation.BASE])
// #endif
			{
				return trialData.numBounces / targetConditionBounces[TaskType.DifficultyEvaluation.BASE];
			}
			return 0; 
		});
		moderateTrialCondition = new TrialCondition(7, 10, false, feedbackExample, (TrialData trialData) => 
		{ 
			if (trialData.numBounces >= targetConditionBounces[TaskType.DifficultyEvaluation.MODERATE] && (!globalControl.targetHeightEnabled || trialData.numAccurateBounces >= targetConditionAccurateBounces[TaskType.DifficultyEvaluation.MODERATE])) 
			{
				int bounces = trialData.numBounces / targetConditionBounces[TaskType.DifficultyEvaluation.MODERATE];
				int accurateBounces = trialData.numAccurateBounces / targetConditionAccurateBounces[TaskType.DifficultyEvaluation.MODERATE];
				return !globalControl.targetHeightEnabled ? bounces : accurateBounces; 
			} 
			return 0; 
		});
		maximaltrialCondition = new TrialCondition(7, 10, false, feedbackExample, (TrialData trialData) => 
		{
			if (trialData.numBounces >= targetConditionBounces[TaskType.DifficultyEvaluation.MAXIMAL])
			{
				return trialData.numAccurateBounces / targetConditionBounces[TaskType.DifficultyEvaluation.MAXIMAL];
			}
			return 0; 
		});

		// feedback conditions
		trialConditions.Add(new TrialCondition(5, 5, true, feedbackExample, (TrialData trialData) => { if (trialData.numBounces < 0) { return 1; } return 0; }));
	}

	/// <summary>
	/// run through all diffiuclties in a short amount of time to get a feel for them
	/// </summary>
	void StartShowcase()
	{
		pauseHandler.Resume();
		SetDifficulty(difficulty);
		StartCoroutine(StartDifficultyDelayed(difficultyExampleTime, true));
	}

	IEnumerator StartDifficultyDelayed(float delay, bool initial = false)
	{
		if (initial)
		{
			// wait until after the pause is lifted, when timescale is 0
			yield return new WaitForSeconds(.1f);
		}

		var audioClip = GetDifficiultyAudioClip(difficulty);
		if (audioClip != null)
		{
			difficultySource.PlayOneShot(audioClip);
		}
		Debug.Log("playing difficulty audio " + (audioClip != null ? audioClip.name : "null"));

		yield return new WaitForSecondsRealtime(delay);

		// reset ball, change difficulty level, possible audio announcement.
		if (difficulty >= 10)
		{
			// finish up the difficulty showcase, quit application
			QuitTask();
		}
		else
		{
			SetDifficulty(difficulty + 2);
			StartCoroutine(StartDifficultyDelayed(difficultyExampleTime));
			if (difficulty > 10) // OG ==
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
		if (!inHoverMode)
			return;

		timeToDropQuad.SetActive(true);

		ball.GetComponent<SphereCollider>().enabled = false;

		// Hover ball at target line for a second
		StartCoroutine(PlayDropSound(ballResetHoverSeconds - 0.15f));
		StartCoroutine(ReleaseHoverOnReset(ballResetHoverSeconds));

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
		if (inHoverMode) 
			return;

		if (inRespawnMode)
			return;         
		
		// Check if ball is on ground
		if (ball.transform.position.y < ball.transform.localScale.y)
		{
			ResetTrial();
			_isInTrial = true;
			_trialTimer = 0;
		}
	}

	IEnumerator Respawning()
	{
		Debug.Log("Respawning started " + Time.timeScale);
		inRespawnMode = true;
		Time.timeScale = 1f;
		effectController.StopAllParticleEffects();
		effectController.StartEffect(effectController.dissolve);
		yield return new WaitForSeconds(ballRespawnSeconds);
		inRespawnMode = false;
		inHoverMode = true;
		yield return new WaitForEndOfFrame();
		effectController.StopParticleEffect(effectController.dissolve);
		pauseHandler.Pause();
		effectController.StartEffect(effectController.respawn);
		yield return new WaitForSeconds(ballRespawnSeconds);
		ball.GetComponent<Ball>().TurnBallWhite();
		Debug.Log("Respawning finished " + Time.timeScale);
	}

	// Drops ball after reset
	IEnumerator ReleaseHoverOnReset(float time)
	{
		if (inHoverResetCoroutine)
		{
			yield break;
		}
		inHoverResetCoroutine = true;

		yield return new WaitForSeconds(time);

		// Stop hovering
		inHoverMode = false;
		inHoverResetCoroutine = false;
		inPlayDropSoundRoutine = false;

		ball.GetComponent<SphereCollider>().enabled = true;
		Debug.Log("Exiting hover mode to " + globalControl.timescale);

	}

	// Update time to drop
	IEnumerator UpdateTimeToDropDisplay()
	{
		if (inCoutdownCoroutine)
		{
			yield break;
		}
		inCoutdownCoroutine = true;

		countdown = (int)ballResetHoverSeconds;

		while (countdown >= 1.0f)
		{
			timeToDropText.text = countdown.ToString();
			countdown--;
			yield return new WaitForSeconds(1.0f);
		}

		inCoutdownCoroutine = false;
	}

	// Play drop sound
	IEnumerator PlayDropSound(float time)
	{
		if (inPlayDropSoundRoutine)
		{
			yield break;
		}
		inPlayDropSoundRoutine = true;
		yield return new WaitForSeconds(time);

		ballSoundPlayer.PlayDropSound();
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

		// Record data for final bounce in trial
		GatherBounceData();

		if (globalControl.recordingData)
		{
			// Record Trial Data from last trial
			dataHandler.recordTrial(degreesOfFreedom, Time.time, _trialTimer, trialNum, numBounces, numAccurateBounces, difficultyEvaluation, difficulty);
			// CheckDifficulty();
			trialData[difficultyEvaluationIndex].datas.Add(new TrialData(degreesOfFreedom, Time.time, _trialTimer, trialNum, numBounces, numAccurateBounces, difficulty));

		}

		if (!final && trialNum != 0 && trialNum % 10 == 0)
		{
			// some difficulty effects are regenerated every 10 trials
			SetDifficulty(difficulty);
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
		if (numBounces > 0)
		{
			GatherBounceData();
		}
		SetUpPaddleData();
		numBounces++;
		numTotalBounces++;

		// If there are two paddles, switch the active one
		if (globalControl.numPaddles > 1)
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
				effectController.StopParticleEffect(disableEffect);
			}
			effectController.StartEffect(scoreEffects[scoreEffectTarget].effect);
			ballSoundPlayer.PlayEffectSound(scoreEffects[scoreEffectTarget].audioClip);

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

	// Turns ball green briefly and plays success sound.
	public void IndicateSuccessBall()
	{
		Ball b = ball.GetComponent<Ball>();

		ballSoundPlayer.PlaySuccessSound();

		b.TurnBallGreen();
		StartCoroutine(b.TurnBallWhiteCR(0.3f));
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
			EvaluateDifficultyResult(true);
		}
		else
		{
			CheckTrialConditions(fromBounce);
		}

		if (difficultyEvaluation == TaskType.DifficultyEvaluation.CUSTOM && globalControl.GetTimeLimitSeconds() == 0)
		{
			return;
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
		TrialCondition difficultyCondition = GetDifficultyCondition(difficultyEvaluation);
   		return CheckCondition(difficultyCondition);
	}

	/// <summary>
	/// evaluate the set of recent bouces in the trial and recent trials. will determine how many true conditions there are/>
	/// </summary>
	/// <param name="trialCondition"></param>
	/// <param name="fromBounce"></param>
	/// <returns></returns>
	bool CheckCondition(TrialCondition trialCondition, bool fromBounce = false)
	{
		var datas = new List<TrialData>(trialData[difficultyEvaluationIndex].datas);

		if (fromBounce)
		{
			// add data from current set in progress
			datas.Add(new TrialData(degreesOfFreedom, Time.time, _trialTimer, trialNum, numBounces, numAccurateBounces, difficulty));
		}

		if (trialCondition.trialEvaluationCooldown > 0)
		{
			trialCondition.trialEvaluationCooldown--;
			return false;
		}

		int trueCount = 0;

		// check for true conditions in recent data  
 		for(int i = datas.Count - 1; i >= datas.Count - (trialCondition.trialEvaluationsSet < datas.Count ? trialCondition.trialEvaluationsSet : datas.Count) && i >= 0; i--)
		{
			int trueInTrial = trialCondition.checkTrialCondition(datas[i]);
			if (trueInTrial > 0)
			{
				trueCount += trueInTrial;
			}
			else if (trialCondition.sequential)
			{
				trueCount = 0;
			}

			if (trueCount >= trialCondition.trialEvaluationTarget)
			{
				// successful condition
				if (trialCondition.conditionFeedback != null)
				{	
					feedbackSource.PlayOneShot(trialCondition.conditionFeedback);
				}
				trialCondition.trialEvaluationCooldown = trialCondition.trialEvaluationsSet;
				return true;
			}
		}
		return false;
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
		float lowerLimit = targetHeight - targetRadius;
		float upperLimit = targetHeight + targetRadius;

		return (height > lowerLimit) && (height < upperLimit);
	}

	public TrialCondition GetDifficultyCondition(TaskType.DifficultyEvaluation evaluation)
	{
		if (evaluation == TaskType.DifficultyEvaluation.BASE)
		{
			return baseTrialCondition;
		}
		else if (evaluation == TaskType.DifficultyEvaluation.MODERATE)
		{
			return moderateTrialCondition;
		}
		else if (evaluation == TaskType.DifficultyEvaluation.MAXIMAL)
		{
			return maximaltrialCondition;
		}

		return null;
	}

	public int GetMaxDifficultyTrialTime(TaskType.DifficultyEvaluation difficultyEvaluation)
	{
		int trialTime = -1;
		if (difficultyEvaluation == TaskType.DifficultyEvaluation.BASE)
		{
			trialTime = globalControl.maxBaselineTrialTime;
		}
		else if (difficultyEvaluation == TaskType.DifficultyEvaluation.MODERATE)
		{
			if(difficultyEvaluationIndex == difficultyEvaluationOrder.IndexOf(TaskType.DifficultyEvaluation.MODERATE))
			{
				trialTime = globalControl.maxModerate1TrialTime;
			}
			else if (difficultyEvaluationIndex == difficultyEvaluationOrder.LastIndexOf(TaskType.DifficultyEvaluation.MODERATE))
			{
				trialTime = globalControl.maxModerate2TrialTime;
			}
		}
		else if (difficultyEvaluation == TaskType.DifficultyEvaluation.MAXIMAL)
		{
			trialTime = globalControl.maxMaximalTrialTime;
		}
		else if (difficultyEvaluation == TaskType.DifficultyEvaluation.CUSTOM)
		{
			trialTime = globalControl.maxTrialTime;
		}

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

#region Gathering and recording data

	public void StartRecording()
	{
		// Record session data
		dataHandler.recordHeaderInfo(condition, expCondition, session, maxTrialTime, hoverTime, targetRadius);
	}

	// Determine data for recording a bounce and finally, record it.
	private void GatherBounceData()
	{
		float apexHeight = Mathf.Max(bounceHeightList.ToArray());
		float apexTargetError = globalControl.targetHeightEnabled ? (apexHeight - targetLine.transform.position.y) : 0;

		bool apexSuccess = globalControl.targetHeightEnabled ? GetHeightInsideTargetWindow(apexHeight) : true;

		// If the apex of the bounce was inside the target window, increase the score
		if (apexSuccess)
		{
			curScore += 10;
			if (GetTargetLineActiveDifficulty(difficulty))
			{
				numAccurateBounces++;
			}

			// IndicateSuccessBall(); // temporariliy disabled while testing apex coroutines in Ball
		}

		//Record Data from last bounce
		Vector3 cbm = ball.GetComponent<Ball>().GetBounceModification();

		if (globalControl.recordingData)
		{
			dataHandler.recordBounce(degreesOfFreedom, Time.time, cbm, trialNum, numBounces, numTotalBounces, apexTargetError, apexSuccess, paddleBounceVelocity, paddleBounceAccel, difficultyEvaluation);

		}

		bounceHeightList = new List<float>();
	}

	// Grab ball and paddle info and record it. Should be called once per frame
	private void GatherContinuousData()
	{
		Paddle paddle = paddlesManager.ActivePaddle;
		Vector3 ballVelocity = ball.GetComponent<Rigidbody>().velocity;
		Vector3 paddleVelocity = paddle.Velocity;
		Vector3 paddleAccel = paddle.Acceleration;

		Vector3 cbm = ball.GetComponent<Ball>().GetBounceModification();

		if (globalControl.recordingData)
		{
			dataHandler.recordContinuous(degreesOfFreedom, Time.time, cbm, globalControl.paused, ballVelocity, paddleVelocity, paddleAccel, difficultyEvaluation);
		}
	}
	// Initialize paddle information to be recorded upon next bounce
	private void SetUpPaddleData()
	{
		Paddle paddle = paddlesManager.ActivePaddle;

		paddleBounceHeight = paddle.Position.y;
		paddleBounceVelocity = paddle.Velocity;
		paddleBounceAccel = paddle.Acceleration;
	}

	#endregion // Gathering and recording data

	#endregion // Checks, Interactions, Data

	#region Difficulty

	private int GetDifficulty(TaskType.DifficultyEvaluation evaluation)
	{
		if (evaluation == TaskType.DifficultyEvaluation.CUSTOM)
		{
			return globalControl.difficulty;
		}
		else if (evaluation == TaskType.DifficultyEvaluation.BASE)
		{
#if UNITY_EDITOR
			// return 10;
#endif
			return 1;
		}
		else
		{
			return performanceDifficulties[difficultyEvaluationIndex];
		}
	}

	/// <summary>
	/// evaluate the data from this evaluation, and set the difficulty and perpare next evaluation
	/// </summary>
	/// <param name="successfulCompletion"></param>
	void EvaluateDifficultyResult(bool successfulCompletion)
	{
		if(difficultyEvaluationIndex + 1 == difficultyEvaluationOrder.Count)
		{
			Debug.Log("Last evaluation start");
		}

		var difficultyScalar = GetPerformanceBasedDifficultyScalar(successfulCompletion);
		var tempDifficulty = 0;

		if(session == TaskType.Session.ACQUISITION || session == TaskType.Session.SHOWCASE)
		{
			// over once end time is reached.
			QuitTask();
			return;
		}

		Debug.Log($"{nameof(tempDifficulty)}={tempDifficulty} {nameof(difficultyScalar)}={difficultyScalar}");

		// each are evaluating for the next difficultyEvaluation
		if (difficultyEvaluation == TaskType.DifficultyEvaluation.BASE)
		{
			tempDifficulty = Mathf.RoundToInt(Mathf.Lerp(2, 5, difficultyScalar));
		}
		else if (difficultyEvaluation == TaskType.DifficultyEvaluation.MODERATE)
		{
			tempDifficulty = Mathf.RoundToInt(Mathf.Lerp(6, 10, difficultyScalar));

		}
		else if (difficultyEvaluation == TaskType.DifficultyEvaluation.MAXIMAL)
		{
			tempDifficulty = Mathf.RoundToInt(Mathf.Lerp(2, 5, difficultyScalar));
		}

		if (tempDifficulty < 0 || tempDifficulty > 10)
		{
			Debug.Log($"{nameof(tempDifficulty)}={tempDifficulty}");
			tempDifficulty = Mathf.Clamp(tempDifficulty, 0, 10);
		}

		performanceDifficulties.Add(tempDifficulty);

		// reset cooldown, condition may be used again
		GetDifficultyCondition(difficultyEvaluation).trialEvaluationCooldown = 0;

		if (difficultyEvaluationIndex + 1 == difficultyEvaluationOrder.Count)
		{
			QuitTask();
			return;
		}

		difficultyEvaluationIndex++;
		difficultyEvaluation = difficultyEvaluationOrder[difficultyEvaluationIndex];
		difficultyEvaluationTrials = GetDifficultyCondition(difficultyEvaluationOrder[difficultyEvaluationIndex]).trialEvaluationTarget;
		Debug.LogFormat("Increased Difficulty Evaluation to {0} with new difficulty evaluation difficulty evaluation: ", difficultyEvaluationIndex, difficultyEvaluation.ToString());

		SetDifficulty(tempDifficulty);
	}

	/// <summary>
	/// evaluates performance compared to trial condition
	/// </summary>
	/// <param name="successfulCompletion"></param>
	/// <returns></returns>
	private float GetPerformanceBasedDifficultyScalar(bool successfulCompletion)
	{
		if (difficultyEvaluationIndex >= 0 && difficultyEvaluationIndex < difficultyEvaluationOrder.Count)
		{
			var datas = trialData[difficultyEvaluationIndex].datas;

			// evaluating time percentage of the way to end, 10 min
			var maxDifficultyTrialTime = GetMaxDifficultyTrialTime(difficultyEvaluation);
			Debug.Log($"{nameof(maxDifficultyTrialTime)}={maxDifficultyTrialTime}");
			var timeScalar = 1 - (globalControl.GetTimeElapsed() / maxDifficultyTrialTime);

			// evaluating performance, average bounces and accurate bounces. 
			int bounces = 0, accurateBounces = 0;
			float averageBounces, averageAccurateBounces;


			foreach(var trial in datas)
			{
				bounces += trial.numBounces;
				accurateBounces += trial.numAccurateBounces;
			}

			var tempCount = datas.Count <= 0 ? 1 : datas.Count;
			averageBounces = (float)bounces / (float)tempCount;
			averageAccurateBounces = (float)accurateBounces / (float)tempCount;

			// average bounces
			float averageBounceScalar = Mathf.Clamp(averageBounces / (float)targetConditionBounces[difficultyEvaluation], 0f, 1.3f);
			
			// accurate bounces
			float averageAccurateBounceScalar = globalControl.targetHeightEnabled ? Mathf.Clamp(averageAccurateBounces / targetConditionAccurateBounces[difficultyEvaluation], 0f, 1.3f) : 0;
			Debug.Log($"{nameof(averageBounces)}={averageBounces} {nameof(averageAccurateBounces)}={averageAccurateBounces} {nameof(averageBounceScalar)}={averageBounceScalar} {nameof(averageAccurateBounceScalar)}={averageAccurateBounceScalar}");

			// Mostly for testing case
			if (averageBounceScalar == 0 && averageAccurateBounceScalar == 0)
			{
				return 0;
			}
			else
			{
				var tempTargetHeight = globalControl.targetHeightEnabled ? 3f : 2f;
				Debug.Log($"{nameof(tempTargetHeight)}={tempTargetHeight}");
				return Mathf.Clamp01((averageBounceScalar + averageAccurateBounceScalar + timeScalar) / (tempTargetHeight));
			}
		}
		
		return -1;
	}

	public void SetDifficulty(int difficultyNew)
	{
		bool difficultyChanged = difficulty != difficultyNew;
		if (difficultyNew < 0 || difficultyNew > 10)
		{
			Debug.LogError("Issue setting difficulty, not in expected range: " + difficultyNew);
			
			return;
		}
		else if (!difficultyChanged)
		{
			Debug.Log("Regeneration some difficulty elements if applicable...");
		}
		else
		{
			Debug.Log("Setting Difficulty: " + difficultyNew);
			numBounces = 0;
			numAccurateBounces = 0;
			curScore = 0f;
			scoreEffectTarget = 0;
			maxScoreEffectReached = false;
			difficulty = difficultyNew;	
		}

		float ballSpeedNew = GetBallSpeedDifficulty(difficulty); // Mathf.Lerp(ballSpeedMin, ballSpeedMax, difficultyScalar);

		// removed bounce height change for now
		// int ballBounceEnd = UnityEngine.Random.Range(0, 2) == 0 ? ballBounceMin : ballBounceMax;
		// int bouncinessNew = (int)Math.Round(Mathf.Lerp(ballBounceMid, ballBounceEnd, difficultyScalar), 0);

		bool targetLineHeightEnabled = GetTargetLineActiveDifficulty(difficulty);
		globalControl.targetHeightEnabled = targetLineHeightEnabled;

		if (globalControl.targetHeightEnabled)
		{
			targetLine.SetActive(true);
		}
		else
		{
			targetLine.SetActive(false);
		}

		float targetLineHeightOffset = GetTargetLineHeightOffsetDifficulty(performanceDifficulties[difficultyEvaluationIndex]);
		globalControl.targetLineHeightOffset = targetLineHeightOffset;
		SetTargetLineHeight(targetLineHeightOffset);


		float targetRadiusNew = globalControl.targetHeightEnabled ? GetTargetLineWidthDifficulty(difficulty) / 2f : 0; // Mathf.Lerp(targetRadiusMin, targetRadiusMax, difficultyScalar);

		globalControl.timescale = ballSpeedNew;
		Time.timeScale = ballSpeedNew;
		Debug.Log("difficulty set timescale to: " + Time.timeScale);

		globalControl.targetRadius = targetRadiusNew;
		targetRadius = targetRadiusNew;
		targetLine.transform.localScale = new Vector3(targetLine.transform.localScale.x, targetRadius * 2f, targetLine.transform.localScale.z);

		difficultyDisplay.text = difficulty.ToString();

		// record difficulty values change
		if (globalControl.recordingData)
		{
			dataHandler.recordDifficulty(ballSpeedNew, targetLineHeightEnabled, targetLineHeightOffset, targetRadiusNew, Time.time, difficulty);
		}

		if (performanceDifficulties.Count > difficultyEvaluationOrder.Count)
		{
			// all difficulties recored
			QuitTask();
		}
	}

	private float GetBallSpeedDifficulty(int difficulty)
	{
		switch (difficulty)
		{
#if UNITY_EDITOR
			// testing at .3 is time consuming
			case 1: return .3f;
#else
			case 1: return .3f;
#endif
			case 2: return .4f;
			case 3: return .45f;
			case 4: return .5f;
			case 5: return .55f;
			case 6: return .6f;
			case 7: return .7f;
			case 8: return .9f;
			case 9: return UnityEngine.Random.Range(.95f, .95f + .1f);
			case 10: return UnityEngine.Random.Range(.95f, .95f + .15f);
			default: return 1f;
		}
	}

	private bool GetTargetLineActiveDifficulty(int difficulty)
	{
		switch (difficulty)
		{
			case 1: return false;
			case 2: return false;
			case 3: return false;
			case 4: return false;
			case 5: return true;
			case 6: return true;
			case 7: return true;
			case 8: return true;
			case 9: return true;
			case 10: return true;
			default: return false;
		}
	}

	private float GetTargetLineHeightOffsetDifficulty(int difficulty)
	{
		switch (difficulty)
		{
			case 1: return 0f;
			case 2: return 0f;
			case 3: return 0f;
			case 4: return 0f;
			case 5: return 0f;
			case 6: return 0f;
			case 7: return 0f;
			case 8: return 0f;
			case 9: return UnityEngine.Random.Range(-.02f, .02f);
			case 10: return UnityEngine.Random.Range(-.02f, -.02f);
			default: return 0f;
		}
	}

	private float GetTargetLineWidthDifficulty(int difficulty)
	{
		switch (difficulty)
		{
			case 1: return 0f;
			case 2: return 0f;
			case 3: return 0f;
			case 4: return 0f;
			case 5: return .04f;
			case 6: return .04f;
			case 7: return .0375f;
			case 8: return .035f;
			case 9: return .0325f;
			case 10: return .03f;
			default: return .04f;
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

	// In order to prevent bugs, wait a little bit for the paddles to switch
	IEnumerator WaitToSwitchPaddles()
	{
		yield return new WaitForSeconds(0.1f);
		// We need the paddle identifier. This is the second parent of the collider in the heirarchy.
		paddlesManager.SwitchActivePaddle();
	}
#endregion // Exploration Mode

}
