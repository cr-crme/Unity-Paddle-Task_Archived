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

	// Manage the current task to perform
	[SerializeField, Tooltip("The main manager for the game difficulty")]
	private DifficultyManager difficultyManager;

	// Manage the current task to perform
	[SerializeField, Tooltip("The main trial manager for the game")]
	private TrialsManager trialsManager;


	[SerializeField]
	[Tooltip("The paddles in the game")]
	PaddlesManager paddlesManager;
	
	[Tooltip("The ball being bounced")]
	[SerializeField]
	private GameObject ball;

	[Tooltip("The line that denotes where the ball should be bounced ideally")]
	[SerializeField]
	private Target targetLine;

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
	AudioClip successfulTrialSound;

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

	// The paddle bounce height, velocity, and acceleration to be recorded on each bounce.
	// These are the values on the *paddle*, NOT the ball
	private float paddleBounceHeight;
	private Vector3 paddleBounceVelocity;
	private Vector3 paddleBounceAccel;

	// Degrees of freedom, how many degrees in x-z directions ball can bounce after hitting paddle
	// 0 degrees: ball can only bounce in y direction, 90 degrees: no reduction in range
	public float degreesOfFreedom;

	// Variables for countdown timer display
	private bool inCoutdownCoroutine = false;

	// Timescale
	public bool slowtime = false;
	private List<float> bounceHeightList = new List<float>();

	private List<ScoreEffect> scoreEffects = new List<ScoreEffect>();
	
	private int difficultyEvaluationIndex = 0;

	int scoreEffectTarget = 0;
	bool maxScoreEffectReached = false;

	float difficultyExampleTime = 30f;

	int highestBounces, highestAccurateBounces;

	GlobalControl globalControl;

	void Start()
	{
		globalControl = GlobalControl.Instance;

		Instantiate(globalControl.environments[globalControl.environmentIndex]);

		// Calibrate the target line to be at the player's eye level
		var kinematics = ball.GetComponent<Kinematics>();
		if (kinematics)
		{
			kinematics.storedPosition = ball.GetComponent<Ball>().SpawnPosition;
		}


		PopulateScoreEffects();

		if(globalControl.session == SessionType.Session.SHOWCASE)
		{
			globalControl.practiseMaxTrialTime = 0;
		}

		SetTrialLevel(globalControl.level);
		Initialize(true);


        // difficulty shifts timescale, so pause it again
        Time.timeScale = 0;
		globalControl.ResetTimeElapsed();
		pauseHandler.Pause();
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


		if (trialsManager.isTimeOver(globalControl.GetTimeElapsed()))
		{
			Debug.Log(
				$"time elapsed {globalControl.GetTimeElapsed()} greater " +
                $"than max trial time {difficultyManager.maximumTrialTime}"
			);
			trialsManager.EvaluateSessionPerformance(globalControl.GetTimeElapsed());
			if (
				globalControl.session == SessionType.Session.SHOWCASE || trialsManager.isSessionOver
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
			if (globalControl.session == SessionType.Session.PRACTISE)
			{
				numBounces += difficultyManager.nbOfBounceRequired * 7;
				numAccurateBounces += difficultyManager.nbOfAccurateBounceRequired * 7;
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

		// Initialize Condition and Visit types
		degreesOfFreedom = globalControl.degreesOfFreedom;

		ball.GetComponent<EffectController>().dissolve.effectTime = globalControl.ballResetHoverSeconds;
		ball.GetComponent<EffectController>().respawn.effectTime = globalControl.ballResetHoverSeconds;

		curScore = 0;

		if (globalControl.session == SessionType.Session.PRACTISE)
		{
			difficultyDisplay.text = difficultyManager.currentLevel.ToString();
        }
        else if (globalControl.session == SessionType.Session.SHOWCASE)
		{
			difficultyManager.currentLevel = 2;
			StartShowcase();
		}
		else
		{
			Debug.LogError($"SessionType: {globalControl.session} not implemented yet");
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


    /// <summary>
    /// run through all diffiuclties in a short amount of time to get a feel for them
    /// </summary>
    void StartShowcase()
	{
		pauseHandler.Resume();
		SetTrialLevel(difficultyManager.currentLevel);
		StartCoroutine(StartDifficultyDelayed(difficultyExampleTime, true));
	}

	IEnumerator StartDifficultyDelayed(float delay, bool initial = false)
	{
		if (initial)
		{
			// wait until after the pause is lifted, when timescale is 0
			yield return new WaitForSeconds(.1f);
		}

		var audioClip = GetDifficultyAudioClip(difficultyManager.currentLevel);
		if (audioClip != null)
		{
			difficultySource.PlayOneShot(audioClip);
		}
		Debug.Log("playing difficulty audio " + (audioClip != null ? audioClip.name : "null"));

		yield return new WaitForSecondsRealtime(delay);

		// reset ball, change difficulty level, possible audio announcement.
		if (difficultyManager.currentLevel >= 10)
		{
			// finish up the difficulty showcase, quit application
			QuitTask();
		}
		else
		{
			SetTrialLevel(difficultyManager.currentLevel + 2);
			StartCoroutine(StartDifficultyDelayed(difficultyExampleTime));
			if (difficultyManager.currentLevel > 10) // OG ==
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

		ball.GetComponent<Ball>().IsCollisionEnabled = false;

		// Hover ball at target line for a second
		StartCoroutine(ball.GetComponent<Ball>().PlayDropSound(globalControl.ballResetHoverSeconds - 0.15f));
		StartCoroutine(ball.GetComponent<Ball>().ReleaseHoverOnReset(globalControl.ballResetHoverSeconds));

		// Start countdown timer 
		StartCoroutine(UpdateTimeToDropDisplay());

		ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
		ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		ball.transform.position = ball.GetComponent<Ball>().SpawnPosition;
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
        if (trialNum < 1 /*|| numBounces < 1*/)
        {
            trialNum++;
			return;
        }

        if (!_isInTrial)
            return;

        _isInTrial = false;

        if (!final && trialNum != 0 && trialNum % 10 == 0)
		{
			// some difficulty effects are regenerated every 10 trials
			SetTrialLevel(difficultyManager.currentLevel);
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
			if (trialsManager.isSessionOver)
			{
				QuitTask();
				return;
			}
			Initialize(false);
		}
	}

#endregion // Reset

#region Checks, Interactions, Data

	// This will be called when the ball successfully bounces on the paddle.
	public void BallBounced()
	{
        numBounces++;
		numTotalBounces++;

		// If there are two paddles, switch the active one
		if (difficultyManager.mustSwitchPaddleAfterHitting)
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
			ball.GetComponent<BallSoundManager>().PlayEffectSound(scoreEffects[scoreEffectTarget].audioClip);

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

		if (trialsManager.CheckIfTrialIsOver())
		{
			feedbackSource.PlayOneShot(successfulTrialSound);
		}

		if (trialsManager.isSessionOver)
		{
			QuitTask();
			return;
		}
	}


	void UpdateHighestBounceDisplay()
	{
		string bounces = highestBounces.ToString();
		highestBouncesDisplay.text = $"{bounces} bounces in a row!";
	
		if (difficultyManager.hasTarget)
		{
			string accurateBounces = highestAccurateBounces.ToString();
			highestAccurateBouncesDisplay.text = $"{accurateBounces} target hits!";
		}
		else
		{
			highestAccurateBouncesDisplay.text = "";
		}
	}

	public int GetMaxDifficultyTrialTime()
	{
		int trialTime = (int)difficultyManager.maximumTrialTime;
		return trialTime != -1 ? trialTime * 60 : trialTime;
	}

	private AudioClip GetDifficultyAudioClip(int difficulty)
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

	#endregion // Checks, Interactions, Data

	#region Difficulty
	void EvaluatePerformance()
    {
		double _score = trialsManager.EvaluateSessionPerformance(globalControl.GetTimeElapsed());

		// each are evaluating for the next difficulty
		int _newLevel = difficultyManager.ScoreToLevel(_score);

		SetTrialLevel(_newLevel);

        difficultyEvaluationIndex++;
		Debug.Log(
			$"Increased Difficulty Evaluation to {difficultyEvaluationIndex} with new difficulty " +
			$"evaluation difficulty evaluation: {difficultyManager.difficultyName}"
		);
	}
	private void SetTrialLevel(int _newLevel)
    {
		difficultyManager.currentLevel = _newLevel;

		// TODO: This should be done by GlobalControl itself
		Debug.Log("Setting Difficulty: " + difficultyManager.currentLevel);
		GlobalControl globalControl = GlobalControl.Instance;
		globalControl.targetWidth = difficultyManager.hasTarget ? difficultyManager.targetWidth / 2f : 0;
		globalControl.timescale = difficultyManager.ballSpeed;

		// Reset trial
		numBounces = 0;
		numAccurateBounces = 0;
		curScore = 0f;
		scoreEffectTarget = 0;
		maxScoreEffectReached = false;

		targetLine.UpdateCondition();
		difficultyDisplay.text = difficultyManager.currentLevel.ToString();

		if (trialsManager.isSessionOver)
		{
			// all difficulties recored
			QuitTask();
		}
	}

#endregion // Difficulty


}
