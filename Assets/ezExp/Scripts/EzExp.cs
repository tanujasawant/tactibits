using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* TODO :
 * - Timer qui va du début a la fin  
 * - Un système de "pause" automatique entre les Trials
 * */

namespace UnityEzExp
{
    //TODO : Mettre ailleurs
    public enum LogLevel
    {
        DEBUG,
        INFO,
        WARNING,
        ERROR,
        NONE
    };

    public enum SaveType
    {
        ALL, // All data stored in one signe file
        USER, // Foreach new user, we save 
        TRIAL
    };

//    public enum ExperimentState
//    {
//        EMPTY, // All data stored in one signe file
//        HEADERLOADED, // Foreach new user, we save 
//        DATALOADED,
//        ENDED
//    };

    public enum TimeFormat
    {
        MILLISECONDS, 
        SECONDS,
        MINUTES
    };

	public enum TemporalState
	{
		NotStarted,
		Started,
		Ended
	}

    /// <summary>
    /// The <see cref="UnityEzExp.EzExp"/> class is used as an interface to the EzExp package. This only class has to be managed to use all functions. 
    /// </summary>
    public class EzExp : MonoBehaviour
    {

        #region Exceptions
        public class ExperimentNotCreatedException : Exception { };
        #endregion

        #region Singleton
        /// <summary>
        /// EzExp unique instance.
        /// Create  a new instance if any EzExp is present on the scene.
        /// Set the EzExp only on the first time.
        /// </summary>
        /// <value>EzExp static instance</value>
        public static EzExp Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                EzExp[] ezExpManager = FindObjectsOfType(typeof(EzExp)) as EzExp[];
                if (ezExpManager.Length == 0)
                {
                    Log.Warning("EzExp not present on the scene. Creating a new one.");
                    ezExpManager = new EzExp[1] { new GameObject("EzExp").AddComponent<EzExp>() };
                }

                // instanciate the new instance and return the value
                _instance = ezExpManager[0];
                return _instance;
            }
            set
            {
                if (EzExp.Instance == null)
                    _instance = value;
                else
                {
                    Log.Error("You can only use one EzExp. Destroying the new one attached to the GameObject " + value.gameObject.name);
                    Destroy(value);
                }
            }
        }
        private static EzExp _instance = null;
        #endregion

        #region variables
		/// <summary>
		/// Current experiment managed by the API.
		/// </summary>
        Experiment _currentExperiment = null;

        public LogLevel logLevel = LogLevel.DEBUG;

        public SaveType saveType = SaveType.ALL;

        public string inputFile;

        public string outputFolder;

        public bool useStartScreen; // Si on est sur la scène de base, on charge le truc de base
        #endregion


        #region Experiment
        void Awake()
        {
            DontDestroyOnLoad(this);
        }

//        public Experiment NewExperiment(string filePath = null)
//        {
//            currentExperiment = new Experiment();
//            if (filePath != null)
//            {
//                currentExperiment.LoadFile(filePath);
//            } else
//            {
//
//            }
//            return currentExperiment;
//        }
        
        public Experiment GetExperiment()
        {
			if (_currentExperiment == null) { throw new ExperimentNotCreatedException (); }

            return _currentExperiment;
        }
        /// <summary>
        /// Load the variables file to prepare the experiment and create an <see cref="UnityEzExp.Experiment"/> instance to store them.</summary>
        /// <param name="filepath">File path to load data from.</param>
        /// <param name="userId">ID of the participant to load experiment data for.</param>
        /// <param name="trialId">ID of the trial to start from.</param>
        /// <param name="usersHeader">Name of the column where users ids can be found.</param>
		public void InitExperiment(string filepath, string userId, int trialId, string usersHeader = "Participant", FileType inputFileType = FileType.CSV, FileType outputFileType = FileType.CSV)
        {
			_currentExperiment = new Experiment(filepath, userId, trialId, usersHeader, inputFileType, outputFileType);
        }

        /// <summary>
        /// Launch at the very beginning of the experiment. Should load files containing exp data, prepare timers, get ready for recording
        /// </summary>
        public Trial StartExperiment(bool autoGetUserId = false )
        {
           return LoadNextTrial();
        }

        /// <summary>
        /// Should be called when the experiment is over to check recording files (and display/throw some messages/events?)
        /// </summary>
        public void EndExperiment()
        {
            /* TODO 
             * - write the file
             * - End all timers
             * - clear trials
             */

        }

		/// <summary>
		/// Gets the parameters of the experiment.
		/// </summary>
		public void GetParameters(out string[] parameters)
		{
			if(_currentExperiment == null) { throw new ExperimentNotCreatedException(); }
			else { _currentExperiment.GetParameters(out parameters); }
		}

		/// <summary>
		/// Sets the results header in order to format saved data into trial correctly.
		/// </summary>
		/// <param name="header">Header of the saved data.</param>
		public void SetResultsHeader(string[] header)
		{
			if(_currentExperiment == null) { throw new ExperimentNotCreatedException(); }
			else { _currentExperiment.SetResultsHeader(header); }
		}

		/// <summary>
		/// Sets the timers header to show choosed timers on the output file.
		/// </summary>
		/// <param name="header">Header of the saved data.</param>
		public void SetTimersHeader(string[] header)
		{
			if(_currentExperiment == null) { throw new ExperimentNotCreatedException(); }
			else { _currentExperiment.SetTimersHeader(header); }
		}

		/// <summary>
		/// Sets the record file path.
		/// </summary>
		/// <param name="path">File path to record data into.</param>
		public void SetRecordFilePath(string path)
		{
			if(_currentExperiment == null) { throw new ExperimentNotCreatedException(); }
			else { _currentExperiment.SetOutputFilePath(path); }
		}
        #endregion


        #region User

        #endregion

        #region Trial
        /// <summary>
        /// Loads the next trial in the list loaded from the init file
        /// </summary>
        public Trial LoadNextTrial()
        {
			return _currentExperiment.LoadNextTrial();
        }

        /// <summary>
        /// Loads the trial.
        /// </summary>
        /// <param name="trialIndex">Index of the trial to load</param>
        /// <returns>The trial.</returns>
        public Trial LoadTrial(int trialIndex)
        {
            /*
            if (0 <= trialIndex && trialIndex < trials.Count)
            {
                currentTrialIndex = trialIndex;
                return (Trial)trials[currentTrialIndex];
            }
            else
            {
                throw new System.IndexOutOfRangeException();
            }*/
                return null;
        }


        /// <summary>
        /// Starts the loaded trial.
        /// </summary>
        public void StartTrial()
        {
			_currentExperiment.StartTrial();
        }

        /// <summary>
        /// Ends the loaded trial.
        /// </summary>
        public void EndTrial()
        {
            _currentExperiment.EndTrial();
        }


        /// <summary>
        /// Get data for a given parameter of the experiment.
        /// </summary>
        /// <param name="parameter">Name of the parameter.</param>
        /// <returns>The data associated.</returns>
        public string GetParameterData(string parameter)
        {
			if(_currentExperiment == null) { throw new ExperimentNotCreatedException(); }
			Trial t = _currentExperiment.GetCurrentTrial();
			return t.GetParameterData(parameter);
        }


		/// <summary>
		/// Saves some results data in the current <see cref="UnityEzExp.Trial"/>.
		/// </summary>
		/// <param name="name">Name of the result.</param>
		/// <param name="value">Value of the result.</param>
		public void SetResultData(string name, string value)
		{
			if(_currentExperiment == null) { throw new ExperimentNotCreatedException(); }
			_currentExperiment.SetResultData(name, value);
		}

		/// <summary>
		/// Gets data for a given result.
		/// </summary>
		/// <returns>The result data.</returns>
		/// <param name="name">Name of the result.</param>
		public string GetResultData(string name)
		{
			return _currentExperiment.GetResultData(name);
		}

        /// <summary>
		/// Return the <see	cref="UnityEzExp.Trial"/> currently loaded in the <see cref="UnityEzExp.Experiment"/> class.
        /// </summary>
        /// <returns></returns>
        public Trial GetCurrentTrial()
        {
			if(_currentExperiment == null) { throw new ExperimentNotCreatedException(); }
			return _currentExperiment.GetCurrentTrial();
        }
        #endregion


        // If the input csv is not made, build one custom
        #region Simple 
        /// <summary>
        /// Define all parameters (columns) we want to use
        /// </summary>
        /// <param name="values">Udefined numer of values (columns)</param>
        public void SetParameters(params string[] values)
        {
            for(int i=0; i< values.Length;i++)
            {
                Log.Debug("The value " + values[i] + " should be added as a new column");
            }
        }

        /// <summary>
        /// Set a value
        /// </summary>
        /// <param name="paramName">Param name (in column)</param>
        /// <param name="value">Value to update</param>
        public void SetValue(string paramName, object value)
        {
            Debug.Log("Should cast value " + value + " to a string, but with a reformating of the comas ',' and add it in the column 'paramName'");
            //TODO what happend in case of override ?
        }
        #endregion


        #region Timers 
		/// <summary>
		/// Adds a timer to the current trial.
		/// </summary>
		/// <param name="name">Name of the timer.</param>
		public void AddTimer(string name)
		{
			if(_currentExperiment == null) { throw new ExperimentNotCreatedException(); }
			_currentExperiment.AddTimer(name);
		}

		/// <summary>
		/// Removes a timer previously added to the current trial.
		/// </summary>
		/// <param name="name">Name of the timer.</param>
		public void RemoveTimer(string name)
		{
			if(_currentExperiment == null) { throw new ExperimentNotCreatedException(); }
			_currentExperiment.RemoveTimer(name);
		}

        /// <summary>
        /// Starts a timer previously added to the current trial.
        /// </summary>
        /// <param name="name">Name of the timer.</param>
        public void StartTimer(string name)
        {
			if(_currentExperiment == null) { throw new ExperimentNotCreatedException(); }
			_currentExperiment.StartTimer(name);
        }

		/// <summary>
		/// Pauses a timer previously added to the current trial.
		/// </summary>
		/// <param name="name">Name of the timer.</param>
		public void PauseTimer(string name)
		{
			if(_currentExperiment == null) { throw new ExperimentNotCreatedException(); }
			_currentExperiment.PauseTimer(name);
		}

		/// <summary>
		/// Resumes a timer previously added to the current trial.
		/// </summary>
		/// <param name="name">Name of the timer.</param>
		public void ResumeTimer(string name)
		{
			if(_currentExperiment == null) { throw new ExperimentNotCreatedException(); }
			_currentExperiment.ResumeTimer(name);
		}

		/// <summary>
		/// Starts a timer previously added to the current trial.
		/// </summary>
		/// <param name="name">Name of the timer.</param>
		public void StopTimer(string name)
		{
			if(_currentExperiment == null) { throw new ExperimentNotCreatedException(); }
			_currentExperiment.StopTimer(name);
		}
        #endregion
    }
}