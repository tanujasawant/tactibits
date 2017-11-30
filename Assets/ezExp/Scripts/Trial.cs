using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEzExp
{
    /// <summary>
    /// Contains information about a trial. All class attributes have to be added at runtime to keep an abstract level.
    /// </summary>
    public class Trial
    {

        #region Exceptions
        /// <summary>
        /// Exception thrown when attributes and values array have different sizes.
        /// </summary>
        public class DifferentSizeException : Exception { public DifferentSizeException(string msg) : base(msg) { } };
        /// <summary>
        /// Exception thrown when trying to end a trial that was not started
        /// </summary>
        public class TemporalStateException : Exception { public TemporalStateException(string msg) : base(msg) { } };
		/// <summary>
		/// Exception thrown if the trial was not bound to an experiment.
		/// </summary>
		public class NotExperimentBoundException: Exception {};
		/// <summary>
		/// Exception thrown when the user tries to add an already added timer.
		/// </summary>
		public class TimerAlreadyExistsException: Exception {};
        #endregion

        #region Attributes
        /// <summary>
        /// Array of values associated to parameters referenced in <see cref="UnityEzExp.Experiment"/> .
        /// </summary>
		string[] _parametersData;

		/// <summary>
		/// Dictionary used to save data about the trial dynamically.
		/// </summary>
		Dictionary<string, string> _savedData = new Dictionary<string, string>();

		/// <summary>
		/// Main timer used to time the overall trial.
		/// </summary>
		EzTimer _mainTimer = new EzTimer();

        /// <summary>
        /// Dictionary containing pairs representing starting and ending time of timers named as the dictionary key.
        /// This dictionary will always at least contain one main timer for the trial.
        /// </summary>
        Dictionary<string, EzTimer> _timers = new Dictionary<string, EzTimer>();

        TemporalState _trialState = TemporalState.NotStarted;

        Experiment _parentExperiment = null;

        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor that only adds a main timer to the timers dictionary.
        /// </summary>
        public Trial(Experiment experiment, string[] parametersData)
        {
            _parentExperiment = experiment;
            _parametersData = parametersData;
			// FIXME timer should be started on StartTrial()
            // StartTimer("main");
        }

        /*
        /// <summary>
        /// Create a new <see cref="Trial"/> by copying the given attributes dictionary (name -> value)
        /// </summary>
        /// <param name="attributes">Dictionary of attributes.</param>
        public Trial(Dictionary<string, string> attributes) : this()
        {
            foreach (KeyValuePair<string, string> pair in attributes)
            {
                _attributes.Add(pair.Key, pair.Value);
            }
        }
        */
        #endregion

		#region getters/setters
		/// <summary>
		/// Gets the data for a given parameter.
		/// </summary>
		/// <param name="parameter">Name of the parameter associated to the data.</param>
		/// <returns>The data.</returns>
		public string GetParameterData(string parameter)
		{
			if(_parentExperiment == null) { throw new NotExperimentBoundException(); }
			else {
				int index = _parentExperiment.GetParameterIndex(parameter);
				return _parametersData[index];
			}
		}

		/// <summary>
		/// Gets all parameters data based on header contained in <see cref="UnityEzExp.Experiment"/>
		/// </summary>
		/// <returns>All parameters data for this trial.</returns>
		public string[] GetAllData()
		{
			string[] copy = new string[_parametersData.Length];
			Array.Copy(_parametersData, copy, _parametersData.Length);
			return copy;
		}


		/// <summary>
		/// Saves data about the trial in the Dictionary <see cref="UnityEzExp.Trial._savedData"/>.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <param name="value">Value of the data.</param>
		/// <returns>Whether a new entry was added to the dictionary</returns>
		public bool SetResultData(string name, string value)
		{
			bool added = _savedData.ContainsKey(name);
			_savedData[name] = value;
			return added;
		}


		/// <summary>
		/// Gets data of a given result.
		/// </summary>
		/// <returns>The result data.</returns>
		/// <param name="name">Name of the result.</param>
		public string GetResultData(string name) {
            try { return _savedData[name]; }
            catch(KeyNotFoundException exn) { Debug.LogError("Key not found: " + name); return null; }
        }


		/// <summary>
		/// Get all results data saved for this trial.
		/// </summary>
		/// <returns>All results data in a dictionary.</returns>
		public Dictionary<string, string> GetResultsData() 
		{
			Dictionary<string, string> copy = new Dictionary<string, string>();
			foreach(KeyValuePair<string, string> p in _savedData) { copy.Add(p.Key, p.Value); }
			return copy;
		}

		/// <summary>
		/// Gets a copy of the timer associated with this name.
		/// </summary>
		/// <returns>A copy of the timer instance.</returns>
		/// <param name="name">Name of the timer.</param>
		// public EzTimer GetTimer(string name) { return new EzTimer(_timers[name]); }
		#endregion

        #region timers
		/// <summary>
		/// Adds a new timer.
		/// </summary>
		/// <param name="name">Name of the timer.</param>
		public void AddTimer(string name)
		{
			if(_timers.ContainsKey(name)) { throw new TimerAlreadyExistsException(); }
			_timers.Add(name, new EzTimer());
		}

		/// <summary>
		/// Used to 
		/// </summary>
		/// <param name="timerName"></param>
		/// <returns></returns>
		public bool RemoveTimer(string name) { return _timers.Remove(name); }

        /// <summary>
        /// Starts a timer present in the list.
        /// </summary>
        /// <param name="name">Name of the timer to start.</param>
        public void StartTimer(string name) { _timers[name].Start(); }

		/// <summary>
		/// Pauses a timer present in the list.
		/// </summary>
		/// <param name="name">Name of the timer to pause.</param>
		public void PauseTimer(string name) { _timers[name].Pause(); }

		/// <summary>
		/// Pauses a timer present in the list.
		/// </summary>
		/// <param name="name">Name of the timer to pause.</param>
		public void ResumeTimer(string name) { _timers[name].Resume(); }

        /// <summary>
        /// Stops the timer with the given name.
        /// </summary>
        /// <param name="name">Name of the timer.</param>
        public void StopTimer(string name) { _timers[name].Stop(); }

        /// <summary>
        /// Gets the duration of the timer without formatting.
        /// </summary>
        /// <param name="name">Name of the timer.</param>
        public float GetTimerRawDuration(string name) { return _timers[name].GetRawDuration(); }

		/// <summary>
		/// Gets the duration of the timer formatted.
		/// </summary>
		/// <returns>The timer duration.</returns>
		/// <param name="name">Name of the timer.</param>
		public TimeSpan GetTimerDuration(string name) { return _timers[name].GetDuration(); }

		/// <summary>
		/// Gets the main duration of the trial without formatting.
		/// </summary>
		public float GetMainRawDuration() { return _mainTimer.GetRawDuration(); }

		/// <summary>
		/// Gets the main duration of the trial formatted.
		/// </summary>
		public TimeSpan GetMainDuration() { return _mainTimer.GetDuration(); }

        /// <summary>
        /// Gets the timers names.
        /// </summary>
        /// <returns>The timers names.</returns>
        public string[] GetTimersNames()
        {
            string[] res = new string[_timers.Count];
            _timers.Keys.CopyTo(res, 0);
            return res;
        }
		#endregion 

		#region functions
        /// <summary>
        /// Starts this trial (starts all timers by default and set state to started).
        /// </summary>
        public void StartTrial()
        {
            if (_trialState == TemporalState.Started) { throw new TemporalStateException("The trial has already been started"); }
			else if (_trialState == TemporalState.Ended) { throw new TemporalStateException("The trial has already been ended"); }
            // StartAllTimers();
			// StartTimer("main");
            _trialState = TemporalState.Started;
			_mainTimer.Start();
        }

		/// <summary>
		/// Ends the trial. The main timer is stopped along with all timers not stopped already.
		/// </summary>
        public void EndTrial()
        {
            if (_trialState == TemporalState.NotStarted) { throw new TemporalStateException("The trial was not started yet."); }
			else if (_trialState == TemporalState.Ended) { throw new TemporalStateException("The trial has already been ended."); }
            // EndTimer("main");
            _trialState = TemporalState.Ended;
			_mainTimer.Stop();
			foreach(EzTimer timer in _timers.Values) { if(timer.GetState() == TemporalState.Started) { timer.Stop(); } }
        }


        /// <summary>
        /// Cancels the trial. The main timer is reset to 0 and the trial can be started again.
        /// </summary>
        public void ResetTrial()
        {
            _trialState = TemporalState.NotStarted;
            _mainTimer.Reset();
            foreach (EzTimer timer in _timers.Values) { if (timer.GetState() == TemporalState.Started) { timer.Reset(); } }
        }
        
        /// <summary>
        /// Returns a string concatenating all attributes values.
        /// </summary>
        /// <param name="showTimers">Used to concatenate timers values at the end of the string.</param>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="Trial"/>.</returns>
		public string ToString(string separation = ";", bool showResults = true, bool showTimers = true)
        {
            // TODO should take output file format into account

            string res = "";
			string[] parametersNames;
			_parentExperiment.GetParameters(out parametersNames);
			for (int i = 0; i < _parametersData.Length; i++)
            {
				res += parametersNames[i] +"="+ _parametersData[i] + separation;
            }

			// show the data recorded during the trial
			if(showResults) {
				foreach(KeyValuePair<string, string> p in _savedData) {
					res += p.Key +"="+ p.Value+separation;
				}
			}

			if(showTimers) {
				res += "mainTimer="+_mainTimer.GetRawDuration() + separation;
				foreach(KeyValuePair<string, EzTimer> p in _timers) {
					 res += p.Key +"="+ p.Value.GetRawDuration() + separation;
				}
			}

            return res.Substring(0, res.Length - 1);
        }
        #endregion
    }
}