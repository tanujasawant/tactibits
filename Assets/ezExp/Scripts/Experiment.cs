using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEzExp;
// TODO : use emum ExperimentState 
//TODO : faire une bool "reprnedre la ou on était" qui reprend le dernier utilisateur, etc ? 

namespace UnityEzExp
{
    #region exceptions
    /// <summary>
    /// Exception triggered if the participant ID was not found when loading the data.
    /// </summary>
    public class ParticipantIDNotFoundException : Exception { public ParticipantIDNotFoundException(string msg) : base(msg) { } }
    /// <summary>
    /// Exception thrown when all trials have been performed.
    /// </summary>
    public class AllTrialsPerformedException : Exception { };
    /// <summary>
    /// Exception thrown while trying to access to a trial but none has been loaded yet.
    /// </summary>
    public class TrialNotLoadedException : Exception { };
    /// <summary>
    /// Exception thrown if the trials were not loaded yet.
    /// </summary>
    public class TrialsEmptyException : Exception { };
    /// <summary>
    /// Exception thrown if the trialId do not match the list of trials.
    /// </summary>
    public class TrialIdOutOfBoundsException : Exception { };
    /// <summary>
    /// Exception thrown if participants header was not found.
    /// </summary>
    public class ParticipantsHeaderNotFoundException : Exception { };
    /// <summary>
    /// Exception thrown if a parameter was not found in the array <see cref="UnityEzExp.Experiment._parameters"/> .
    /// </summary>
    public class ParameterNotFoundException : Exception { };
    #endregion

    #region enum
    /// <summary>
    /// Recording behavior that can be set by the user depending on its need.
    /// </summary>
    public enum RecordBehavior
    {
        SaveOnTrialEnd,
        SaveOnUserDemand
    };

    public enum FileType
    {
        CSV,
        JSON,
        XML
    };
    #endregion


    /// <summary>
    /// The class <see cref="UnityEzExp.Experiment"/> is used to Load and Save data about the experiment configuration. At the beginning, it will load data from a .csv, .xml or .json file 
    /// and will save the results in an output file 
    /// </summary>
    public class Experiment
    {
        #region attributes
        /// <summary>
        /// Parameters are the "header" of the file
        /// </summary>
        string[] _parameters;

        /// <summary>
        /// Name of the results to display in the recorded files.
        /// </summary>
        string[] _resultsHeader;
        /// <summary>
        /// Name of the timers to display in the recorded files.
        /// </summary>
        string[] _timersHeader;

        /// <summary>
        /// List of all trials for a given user
        /// </summary>
        List<Trial> _trials = new List<Trial>();

        // paths of files to load and save data
        string _inputFilePath;
        FileType _inputFileType = FileType.CSV;
        string _outputFilePath;
        FileType _outputFileType = FileType.XML;


        RecordBehavior _recordBehavior = RecordBehavior.SaveOnTrialEnd;

        // name of the column containing participant IDs
        string _participantsHeader;
        // ID of the current participant
        string _participantID;

        int _currentTrialIndex = -1;
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="UnityEzExp.Experiment"/> class.
        /// </summary>
        /// <param name="inputFilePath">Input file path to load data from.</param>
        /// <param name="userId">ID of the participant for whom we want to load experiment data.</param>
        /// <param name="trialId">ID of the trial to start from.</param>
        /// <param name="usersHeader">Name of the column containing participants IDs.</param>
        /// <param name="usersHeader">Name of the column containing participants IDs.</param>
        /// <param name="usersHeader">Name of the column containing participants IDs.</param>
        public Experiment(string inputFilePath, string userId, int trialId = -1, string usersHeader = "Participant", FileType inputFileType = FileType.CSV, FileType outputFileType = FileType.CSV)
        {
            _inputFilePath = inputFilePath;
            _participantsHeader = usersHeader;
            _participantID = userId;
            _inputFileType = inputFileType;
            _outputFileType = outputFileType;
            // prediction of next LoadNextTrial()
            _currentTrialIndex = trialId-1;

            LoadFile(Encoding.UTF8);

            if (_currentTrialIndex < -1 || _trials.Count - 1 <= _currentTrialIndex) { throw new TrialIdOutOfBoundsException(); }
        }
        #endregion

        #region getters/setters
        /// <summary>
        /// Feed the array given in argument of the parameters contained in <see cref="UnityEzExp.Experiment"/>.
        /// </summary>
        /// <param name="parameters">Array in which all parameters will be copied.</param>
        public void GetParameters(out string[] parameters)
        {
            parameters = new string[_parameters.Length];
            Array.Copy(_parameters, parameters, _parameters.Length);
        }
        /// <summary>
        /// Sets the parameters.
        /// </summary>
        /// <param name="parameters">Parameters.</param>
        public void SetParameters(string[] parameters)
        {
            _parameters = new string[parameters.Length];
            Array.Copy(parameters, _parameters, parameters.Length);
        }

        /// <summary>
        /// Gets data for a given parameter from the current trial.
        /// </summary>
        /// <param name="parameter">Name of the parameter.</param>
        /// <returns>Data about the parameter.</returns>
        public string GetParameterData(string parameter)
        {
            if (_currentTrialIndex < 0) { throw new TrialNotLoadedException(); }
            else if (_trials.Count <= _currentTrialIndex) { throw new AllTrialsPerformedException(); }
            return _trials[_currentTrialIndex].GetParameterData(parameter);
        }

        /// <summary>
        /// Gets the index of the given parameter in the array <see cref="UnityEzExp.Experiment._parameters"/>
        /// </summary>
        /// <returns>The parameter index.</returns>
        /// <param name="parameter">Name of the parameter to find.</param>
        public int GetParameterIndex(string parameter)
        {
            int index = Array.IndexOf(_parameters, parameter);
            if (index < 0) { throw new ParameterNotFoundException(); }
            return index;
        }

        /// <summary>
        /// Returns the results name.
        /// </summary>
        /// <returns>The list of results name to an array.</returns>
        public string[] GetResults() { return _resultsHeader; }

        /// <summary>
        /// Set the results header for record. This function should be called before saving trials data to ensure good formatting of the output data.
        /// </summary>
        /// <param name="results">Names of the results data.</param>
        public void SetResultsHeader(string[] results)
        {
            _resultsHeader = new string[results.Length];
            Array.Copy(results, _resultsHeader, results.Length);
        }

        /// <summary>
        /// Set the timers header for record. If this function is never called, timers are never saved (except the main timer of each trial).
        /// </summary>
        /// <param name="results">Names of the timers as saved in trials.</param>
        public void SetTimersHeader(string[] timers)
        {
            _timersHeader = new string[timers.Length];
            Array.Copy(timers, _timersHeader, timers.Length);
        }

        /// <summary>
        /// Add a new timestamp field in the results table based on the main timer current time.
        /// </summary>
        /// <param name="results">Names of the timers as saved in trials.</param>
        /// <returns>Whether the timestamp was added to the result table.</returns>
        public bool SetTimestamp(string name)
        {
            if (_currentTrialIndex < 0) { throw new TrialNotLoadedException(); }
            else if (_trials.Count <= _currentTrialIndex) { throw new AllTrialsPerformedException(); }

            return SetResultData(name, _trials[_currentTrialIndex].GetMainRawDuration()+"");
        }

        /// <summary>
        /// Gets the index of the current trial.
        /// </summary>
        /// <returns>The current trial index.</returns>
        public int GetCurrentTrialIndex() { return _currentTrialIndex; }

        /// <summary>
        /// Add a trial to the list.
        /// </summary>
        /// <param name="trials">One or several trials to add.</param>
        public void SetTrial(params Trial[] trials)
        {
            foreach (Trial parameter in trials)
                _trials.Add(parameter);
        }

        /// <summary>
        /// Sets the trials.
        /// </summary>
        /// <param name="trials">Trials.</param>
        void SetTrials(List<Trial> trials)
        {
            _trials = new List<Trial>();
            foreach (Trial t in trials) { _trials.Add(t); }
        }

        /// <summary>
        /// Gets the number of trials.
        /// </summary>
        /// <returns>Overall number of trials for a participant.</returns>
        public int GetTrialsCount() { return _trials.Count; }

        /// <summary>
        /// Sets the output file path.
        /// </summary>
        /// <param name="outputFilePath">Output file path.</param>
        public void SetOutputFilePath(string outputFilePath) { _outputFilePath = outputFilePath; }

        /// <summary>
        /// Sets the record behavior.
        /// </summary>
        /// <param name="behavior">Behavior to adopt.</param>
        public void SetRecordBehavior(RecordBehavior behavior) { _recordBehavior = behavior; }

        // FIXME should not allow that kind of behavior -> parameters are fixed in the init file
        //		/// <summary>
        //		/// Add a list of undefined number of parameters manually
        //		/// </summary>
        //		/// <param name="parameters">Undefined number of parameters</param>
        //		public void AddParameter(params string[] parameters)
        //		{
        //			foreach (string parameter in parameters)
        //				_parameters.Add(parameter);
        //		}

        //		/// <summary>
        //		/// Set the parameters from list
        //		/// </summary>
        //		/// <param name="parameters">List of parameters</param>
        //		void SetParameters(List<string> parameters)
        //		{
        //			_parameters = parameters;
        //		}
        #endregion

        #region load/save
        /// <summary>
        /// Load data from a file, with the provided format. Possibility to specify the separation character in .csv files. Parameters header will be instantiated here.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <param name="fileType">Format of the file to parse.</param>
        /// <param name="encoding">Encoding of the file to parse.</param>
        /// <param name="separation">Separation character used in .csv files.</param>
        public void LoadFile(Encoding encoding, char separation = ',')
        {
            switch (_inputFileType)
            {
                case FileType.CSV:
                    LoadCSV(encoding, separation);
                    break;
                case FileType.JSON:
                    LoadJSON(encoding);
                    break;
                case FileType.XML:
                    LoadXML(encoding);
                    break;
            }
        }

        /// <summary>
        /// Load the config file as CSV. All trials for a given participant will be added to the list of trials.
        /// </summary>
        /// <returns>Return the first trial</returns>
		void LoadCSV(Encoding encoding, char separation)
        {
            _trials = new List<Trial>();

            // read init file until the end
            StreamReader reader = new StreamReader(_inputFilePath);
            int lineIndex = 0;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                // lines starting with # are considered comments
                if (line.StartsWith("#") || line.Trim() == "") { continue; }
                else
                {
                    // this should be the HEADER
                    if (lineIndex == 0)
                    {
                        _parameters = line.Split(separation);
                        // remove useless spaces
                        for (int i = 0; i < _parameters.Length; i++) { _parameters[i] = _parameters[i].Trim(); }
                    }
                    // this should be a TRIAL
                    else
                    {
                        string[] values = line.Split(separation);
                        // check if the trial corresponds to the user id 
                        int headerCol = Array.IndexOf<string>(_parameters, _participantsHeader);
                        if (headerCol < 0) { throw new ParticipantsHeaderNotFoundException(); }
                        if (values[headerCol] == _participantID)
                        {
                            _trials.Add(new Trial(this, values));
                        }
                    }
                    lineIndex++;
                }
            }

            // participant ID wasn't found
            if (_trials.Count == 0) { throw new ParticipantIDNotFoundException(_participantID + " was not found in " + _participantsHeader + " section in the loaded file. (" + _inputFilePath + ")"); }
        }

        void LoadJSON(Encoding encoding)
        {
            // TODO
            Log.Warning("Not supported");
        }


        void LoadXML(Encoding encoding)
        {
            // TODO
            Log.Warning("Not supported");
        }


        /// <summary>
        /// Records data about the current trial.
        /// </summary>
        /// <param name="encoding">Encoding of the file.</param>
        /// <param name="separation">Separation characters use for .csv format.</param>
        public void SaveCurrentTrial(Encoding encoding, char separation = ',')
        {
            if (_outputFilePath == null || _outputFilePath == "") { throw new IOException("No output file path specified: impossible to record data."); }

            if (_currentTrialIndex < 0) { throw new TrialNotLoadedException(); }
            else if (_trials.Count <= _currentTrialIndex) { throw new AllTrialsPerformedException(); }

            // warn in case the results header is not set
            if (_resultsHeader == null) { Log.Warning("The results header has not been set. Trials saved data might be wrongly formatted."); }

            switch (_outputFileType)
            {
                case FileType.CSV:
                    SaveCurrentTrialCSV(encoding, separation);
                    break;
                case FileType.JSON:
                    SaveCurrentTrialJSON(encoding);
                    break;
                case FileType.XML:
                    SaveCurrentTrialXML(encoding);
                    break;
            }
        }

        /// <summary>
        /// Saves the current trial in a .csv file. If the file does not exist yet, it will be created and the header added at the beginning.
        /// </summary>
        /// <param name="encoding">Encoding of the file.</param>
        void SaveCurrentTrialCSV(Encoding encoding, char separation)
        {
            Trial t = _trials[_currentTrialIndex];

            bool created = !File.Exists(_outputFilePath);
            // let exception flows if needs be
            StreamWriter writer = new StreamWriter(_outputFilePath, true);
            // write header at beginning of the file
            if (created)
            {
                string first = string.Join(separation + "", _parameters);
                if (_resultsHeader != null) { first += separation + string.Join(separation + "", _resultsHeader); }
                first += separation + "TaskCompletionTime";
                if (_timersHeader != null) { first += separation + string.Join(separation + "", _timersHeader); }
                writer.WriteLine(first);
            }

            // save parameters of this trial first
            string toRecord = string.Join(separation + "", t.GetAllData());
            // save all results saved for this trial
            Dictionary<string, string> savedData = t.GetResultsData();
            if (savedData.Count > 0)
            {

                if (_resultsHeader != null)
                {
                    foreach (string rh in _resultsHeader) { toRecord += separation + t.GetResultData(rh); }
                    //					// need to order data according to _results header
                    //					string[] toWrite = new string[_resultsHeader.Count];
                    //					// order elements to write them in a formatted order in the record file
                    //					int index = 0;
                    //					foreach(KeyValuePair<string, string> p in savedData) {
                    //						toWrite[_resultsHeader.IndexOf(p.Key)] = p.Value;
                    //						toRecord += separation+"{"+(index++)+"}";
                    //					}
                    //					toRecord = string.Format(toRecord, toWrite);
                }
                // order does not matter if the results header was provided
                else
                {
                    foreach (string v in savedData.Values) { toRecord += separation + v; }
                }
            }
            // always save main timer
            toRecord += separation + "" + t.GetMainRawDuration();
            // save all timers at the end
            if (_timersHeader != null)
            {
                foreach (string th in _timersHeader) { toRecord += separation + "" + t.GetTimerRawDuration(th); }
            }

            writer.WriteLine(toRecord);
            writer.Close();
        }

        void SaveCurrentTrialJSON(Encoding encoding)
        {
            // TODO 
        }

        void SaveCurrentTrialXML(Encoding encoding)
        {
            Trial t = _trials[_currentTrialIndex];

            StreamWriter writer = new StreamWriter(_outputFilePath, true);

            string toRecord = "<trial";
            // first record all parameters
            foreach (string p in _parameters) { toRecord += " " + p + "=\"" + t.GetParameterData(p) + "\""; }

            Dictionary<string, string> savedData = t.GetResultsData();
            if (savedData.Count > 0)
            {
                if (_resultsHeader != null)
                {
                    foreach (string rh in _resultsHeader) { toRecord += " " + rh + "=\"" + t.GetResultData(rh) + "\""; }
                }
                //					// need to order data according to _results header
                //					string[] toWrite = new string[_results.Count];
                //					// order elements to write them in a formatted order in the record file
                //					foreach(KeyValuePair<string, string> p in savedData) { toWrite[_results.IndexOf(p.Key)] = p.Value; }
                //					// prepare formatted string to receive the data
                //					for(int i = 0; i < _results.Count; i++) { toRecord += " "+_results[i]+"=\"{"+i+"}\""; }
                //					toRecord = string.Format(toRecord, toWrite);
                //				} 
                // order does not matter if the results header was provided
                else { foreach (KeyValuePair<string, string> p in savedData) { toRecord += " " + p.Key + "=\"" + p.Value + "\""; } }
            }
            // always save main timer (should be able to rename it)
            toRecord += " TaskCompletionTime=\"" + t.GetMainRawDuration() + "\"";
            // save all timers at the end
            if (_timersHeader != null) { foreach (string th in _timersHeader) { toRecord += " " + th + "=\"" + t.GetTimerRawDuration(th) + "\""; } }

            toRecord += "/>";
            writer.WriteLine(toRecord);
            writer.Close();
        }
        #endregion

        #region trials
        /// <summary>
        /// Return the next trial in the list and increase the <see cref="UnityEzExp.Experiment._currentTrialIndex"/>. This or <see cref="UnityEzExp.Experiment.LoadTrial"/> should be called before <see cref="UnityEzExp.Experiment.StartTrial"/>.
        /// </summary>
        /// <returns>The trial.</returns>
        public Trial LoadNextTrial()
        {
            _currentTrialIndex++;
            if (_trials.Count <= _currentTrialIndex) { throw new AllTrialsPerformedException(); }
            else
            {
                return _trials[_currentTrialIndex];
            }
        }


        /// <summary>
        /// Loads the trial at the given index. This or <see cref="UnityEzExp.Experiment.LoadNextTrial"/> should be called before <see cref="UnityEzExp.Experiment.StartTrial"/>.
        /// </summary>
        /// <returns>The trial.</returns>
        /// <param name="index">Index.</param>
        public Trial LoadTrial(int index)
        {
            if (index < 0 || _trials.Count <= index) { throw new IndexOutOfRangeException(); }
            _currentTrialIndex = index - 1;
            return LoadNextTrial();
        }


        /// <summary>
        /// Gets the current trial.
        /// </summary>
        public Trial GetCurrentTrial()
        {
            if (_currentTrialIndex < 0) { throw new TrialNotLoadedException(); }
            else if (_trials.Count <= _currentTrialIndex) { throw new AllTrialsPerformedException(); }
            else { return _trials[_currentTrialIndex]; }
        }

        /// <summary>
        /// Get all trials loaded for the given participant.
        /// </summary>
        /// <returns>Array of all trials.</returns>
        public Trial[] GetAllParticipantTrials()
        {
            if (_trials == null || _trials.Count == 0) { throw new TrialsEmptyException(); }
            return _trials.ToArray();
        }

        /// <summary>
        /// Starts the current trial. A trial has to be loaded before calling this function (<see cref="UnityEzExp.Experiment.LoadNextTrial"/>).
        /// </summary>
        public void StartTrial()
        {
            if (_currentTrialIndex < 0) { throw new TrialNotLoadedException(); }
            else if (_trials.Count <= _currentTrialIndex) { throw new AllTrialsPerformedException(); }
            else
            {
                Trial t = _trials[_currentTrialIndex];
                t.StartTrial();
            }
        }


        /// <summary>
        /// Ends the current trial. A trial has to be started before calling this function (<see cref="UnityEzExp.Experiment.StartTrial"/>).
        /// Depending on <see cref="UnityEzExp.RecordBehavior"/>, the trial results might be recorded before ended.
        /// </summary>
        public void EndTrial()
        {
            if (_currentTrialIndex < 0) { throw new TrialNotLoadedException(); }
            else if (_trials.Count <= _currentTrialIndex) { throw new AllTrialsPerformedException(); }
            else
            {
                Trial t = _trials[_currentTrialIndex];
                t.EndTrial();

                if (_recordBehavior == RecordBehavior.SaveOnTrialEnd) { SaveCurrentTrial(Encoding.UTF8); }
            }
        }
        
        /// <summary>
        /// Resets the current trial to its default settings before it was started. 
        /// </summary>
        public void ResetTrial()
        {
            if (_currentTrialIndex < 0) { throw new TrialNotLoadedException(); }
            else if (_trials.Count <= _currentTrialIndex) { throw new AllTrialsPerformedException(); }
            else
            {
                _trials[_currentTrialIndex].ResetTrial();
            }
        }
        
        /// <summary>
        /// Records data about the trial. A trial has to be loaded before calling this function (<see cref="UnityEzExp.Experiment.LoadNextTrial"/>).
        /// </summary>
        /// <param name="name">Name of the data.</param>
        /// <param name="value">Value of the data.</param>
        /// <returns>Whether the timestamp was added to the results table.</returns>
        public bool SetResultData(string name, string value)
        {
            if (_currentTrialIndex < 0) { throw new TrialNotLoadedException(); }
            else if (_trials.Count <= _currentTrialIndex) { throw new AllTrialsPerformedException(); }

            return _trials[_currentTrialIndex].SetResultData(name, value);
        }


        /// <summary>
        /// Set multiple results data at the same time.
        /// </summary>
        /// <param name="results">Keys = result header and Values = results' data</param>
        public void SetResultsData(ref Dictionary<string, string> results)
        {
            if (_currentTrialIndex < 0) { throw new TrialNotLoadedException(); }
            else if (_trials.Count <= _currentTrialIndex) { throw new AllTrialsPerformedException(); }
             
            foreach (KeyValuePair<string, string> pair in results) {
                bool added = _trials[_currentTrialIndex].SetResultData(pair.Key, pair.Value);
                if(added) { Log.Debug("Added results on trial ready: " + pair.Key); }
            }
        }


        /// <summary>
        /// Gets data of a given result data.
        /// </summary>
        /// <returns>Data associated to the result</returns>
        /// <param name="name">Name of the result.</param>
        public string GetResultData(string name)
        {
            if (_currentTrialIndex < 0) { throw new TrialNotLoadedException(); }
            else if (_trials.Count <= _currentTrialIndex) { throw new AllTrialsPerformedException(); }

            return _trials[_currentTrialIndex].GetResultData(name);
        }

        /// <summary>
        /// Adds a timer to the current trial.
        /// </summary>
        /// <param name="name">Name of the timer.</param>
        public void AddTimer(string name)
        {
            if (_currentTrialIndex < 0) { throw new TrialNotLoadedException(); }
            else if (_trials.Count <= _currentTrialIndex) { throw new AllTrialsPerformedException(); }

            _trials[_currentTrialIndex].AddTimer(name);
        }

        /// <summary>
        /// Removes a previously added timer from the current trial.
        /// </summary>
        /// <param name="name">Name of the timer.</param>
        public void RemoveTimer(string name)
        {
            if (_currentTrialIndex < 0) { throw new TrialNotLoadedException(); }
            else if (_trials.Count <= _currentTrialIndex) { throw new AllTrialsPerformedException(); }

            _trials[_currentTrialIndex].RemoveTimer(name);
        }

        /// <summary>
        /// Starts a timer owned by the current trial.
        /// </summary>
        /// <param name="name">Name of the timer.</param>
        public void StartTimer(string name)
        {
            if (_currentTrialIndex < 0) { throw new TrialNotLoadedException(); }
            else if (_trials.Count <= _currentTrialIndex) { throw new AllTrialsPerformedException(); }

            _trials[_currentTrialIndex].StartTimer(name);
        }

        /// <summary>
        /// Pauses a timer owned by the current trial.
        /// </summary>
        /// <param name="name">Name of the timer.</param>
        public void PauseTimer(string name)
        {
            if (_currentTrialIndex < 0) { throw new TrialNotLoadedException(); }
            else if (_trials.Count <= _currentTrialIndex) { throw new AllTrialsPerformedException(); }

            _trials[_currentTrialIndex].PauseTimer(name);
        }

        /// <summary>
        /// Resumes a timer owned by the current trial.
        /// </summary>
        /// <param name="name">Name of the timer.</param>
        public void ResumeTimer(string name)
        {
            if (_currentTrialIndex < 0) { throw new TrialNotLoadedException(); }
            else if (_trials.Count <= _currentTrialIndex) { throw new AllTrialsPerformedException(); }

            _trials[_currentTrialIndex].ResumeTimer(name);
        }

        /// <summary>
        /// Stops a timer owned by the current trial.
        /// </summary>
        /// <param name="name">Name of the timer.</param>
        public void StopTimer(string name)
        {
            if (_currentTrialIndex < 0) { throw new TrialNotLoadedException(); }
            else if (_trials.Count <= _currentTrialIndex) { throw new AllTrialsPerformedException(); }

            _trials[_currentTrialIndex].StopTimer(name);
        }
        #endregion
    }
}
