using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEzExp;
using System;

public class ExperienceController : MonoBehaviour {
	public bool runExperiment=true;
    #region Singleton
    private static ExperienceController _instance;
	long valencet1,valencet2,valencet,arousalt1,arousalt2,arousalt;
	public PatternManager pm;
	/*public float motorIntensity;
	public float temperatureIntensity;
	public float duration;
	public float startOffset;//time duration for which first peltier warms up
	*/
	Trial t;


    public static ExperienceController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (ExperienceController)FindObjectOfType(typeof(ExperienceController));

                if (FindObjectsOfType(typeof(ExperienceController)).Length > 1)
                {
                    Debug.LogError("[Singleton] Something went really wrong " +
                        " - there should never be more than 1 singleton!" +
                        " Reopening the scene might fix it.");
                    return _instance;
                }
                if (_instance == null)
                {
                    GameObject singleton = new GameObject();
                    _instance = singleton.AddComponent<ExperienceController>();
                    singleton.name = "_RobotController";
                    DontDestroyOnLoad(singleton);
                    Debug.LogWarning("[Singleton] An instance of " + typeof(ExperienceController) +
                        " is needed in the scene, so '" + singleton +
                        "' was created with DontDestroyOnLoad.");
                }
                else
                {
                    Debug.LogWarning("[Singleton] Using instance already created: " +
                        _instance.gameObject.name);
                }
            }
            return _instance;
        }
    }
    #endregion

    #region Experiment variables
    [Header("File path")]
    Experiment _experiment;


    public string inputDataPath = "Assets/Data/yourcsv.csv";
    public string outputDataPath = "Assets/Data/";

    [Space]
    [Header("Settings")]
    [SerializeField]
    bool autoStartExperiment = false;

    [Space]
    [Header("CurrentTrial")]
    public string currentUserId = "";
    public int currentTrialIndex = -1;
    public int currentTotalIndex = 0;

    [Space]
    [Header("Delay")]
    // public bool useDelay = true;
    public bool displayDelayCountdownPanel = false;
    public bool useDelayBetweenTrials = true;
    [Range(0, 10)]
    public int delayBeforeTouch = 1;
    [Range(0, 10)]
    public int delayBeforeQuestions = 1;
    #endregion

	public GameObject arousalPanel = null;
	public GameObject valencePanel = null;
    void Start ()
	{
        DontDestroyOnLoad(this);
        if(autoStartExperiment)
			StartExperiment(currentUserId+"", currentTrialIndex, 0, false);

		arousalPanel.SetActive (false);
		valencePanel.SetActive (false);
    }

    public void StartExperiment(string userID, int trialID, int startWith, bool skipTraining, bool forceAvatar = false)
	{
        if (!runExperiment)
            return;	
     
        currentUserId = userID;
        _experiment = new Experiment(inputDataPath, userID, trialID, "Subject");
        currentTotalIndex = trialID;

        _experiment.SetOutputFilePath(outputDataPath +  userID + "-data.csv");
		_experiment.SetResultsHeader(new string[4]{"Arousal", "Valence","ValenceReactionTime","ArousalReactionTime"}); // correspond to the values you want to measure
		//_experiment.SetTimersHeader(new string[1]{"ReactionTime"});
		//_experiment.PauseTimer
        Debug.Log("Start experiment with user " + userID);
		NextTrial();
    }

    #region Trial Start
    public void NextTrial()
	{
		if (!runExperiment)
			return;
		try {
			t = _experiment.LoadNextTrial ();
		} catch (AllTrialsPerformedException e) {
			Debug.Log ("Experiment finished !");
//            return;
		}

		// _experiment.StartTrial();
		currentTrialIndex = _experiment.GetCurrentTrialIndex ();

		Debug.Log ("Next Trial");
		// parse what you have in thee header
		string pattern = _experiment.GetParameterData ("pattern");
		string temperature = _experiment.GetParameterData ("temperature");
		Debug.Log ("pattern = " + pattern);
		Debug.Log ("temperature = " + temperature);

		if (temperature == "hot")
			pm.temperatureIntensity = 255;
		else if (temperature == "cold")
			pm.temperatureIntensity = 32;
		else
			pm.temperatureIntensity = 0;
		pm.TriggerPatterns (pattern, pm.motorIntensity, pm.temperatureIntensity, pm.duration, pm.startOffset);

		_experiment.AddTimer ("ReactionTime");

		_experiment.StartTrial();

		TouchStimuliFinished ();

    }

	int timer = 0;
	void Update	() {
//		if (Input.GetKeyUp (KeyCode.A)) {
//			_experiment.StopTimer ("UserInput");
//			Debug.Log ("timer stopped");
//		}
//		timer++;
//		if (500 < timer) {
//			_experiment.EndTrial ();
//			Debug.Log ("exp stopped");
//			timer = -1000;
//		}
	}
    #endregion

    #region Trial End
    // This function has to be triggerd after the stimuli is made. 
    public void TouchStimuliFinished()
    {
        if (!runExperiment)
            return;

        Invoke("DelayedStartQuestions", delayBeforeQuestions);
    }

    void DelayedStartQuestions()
    {
		Debug.Log ("display first panel");

		valencePanel.SetActive (true);
		_experiment.StartTimer ("ReactionTime");

		var unixTime = DateTime.Now.ToUniversalTime () - new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		valencet1 =(long)unixTime.TotalMilliseconds;

        // Display the panel with the questions

    }

	public void ArousalPanelClicked(int val) {
		var unixTime = DateTime.Now.ToUniversalTime () - new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		arousalt2 =(long)unixTime.TotalMilliseconds;
		arousalt = arousalt2 - arousalt1;
		_experiment.SetResultData("ArousalReactionTime" ,arousalt.ToString());
		_experiment.SetResultData("Arousal" , val.ToString());
		arousalPanel.SetActive (false);
		QuestionsFinished ();

	}


	public void ValencePanelClicked(int valence) {
		var unixTime = DateTime.Now.ToUniversalTime () - new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		valencet2 =(long)unixTime.TotalMilliseconds;
		valencet = valencet2 - valencet1;
		_experiment.SetResultData("ValenceReactionTime" ,valencet.ToString());
		_experiment.SetResultData("Valence" , valence.ToString());
		valencePanel.SetActive (false);
		arousalPanel.SetActive (true);

		unixTime = DateTime.Now.ToUniversalTime () - new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		arousalt1 =(long)unixTime.TotalMilliseconds;

	}


// This functions are triggers when you click on next
    public void SetEmotion(int value, int emotion, int emotionIntensity)
    {
    }

    public void QuestionsFinished() // When the user click on next
    {
        _experiment.EndTrial();

		/*float totalTime= float.Parse(_experiment.GetParameterData("TaskCompletionTime"));
		float valTime= float.Parse(_experiment.GetParameterData("ValenceReactionTime"));
		float arouseTime = totalTime - valTime;
		_experiment.SetResultData("ArousalReactionTime" , arouseTime.ToString());*/

         GoToNextTrial();
    }

    void GoToNextTrial()
    {
		Invoke("NextTrial", delayBeforeTouch);

		/*if (useDelayBetweenTrials)
        {
            if (displayDelayCountdownPanel)
             //   CountDown.Instance.StartTimer(delayBeforeTouch, this.NextTrial);
            else
                Invoke("NextTrial", delayBeforeTouch);
        }*/
    }


    #endregion

    public void ApplicationStop() { }
}
