using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEzExp
{
	/// <summary>
	/// Customized timer used for trials time recording. The time is for now only computed relative to a base time.
	/// </summary>
    class EzTimer
    {
		#region exceptions
		public class TimerNotStartedException: Exception {}
		public class TimerAlreadyStartedException: Exception {}
		public class TimerEndedException: Exception {}
		#endregion

		#region attributes
		TemporalState _temporalState;
		/// <summary>
		/// Starting time of the timer in milliseconds
		/// </summary>
		float _startTime;
		// user can pause and resume the timer which is recorded as marks in time
		List<float> _breaks;
		/// <summary>
		/// Ending time of the timer in milliseconds
		/// </summary>
        float _endTime;
		/// <summary>
		/// Duration of the timer in milliseconds. Takes into account the pauses and resumes actions taken upon this timer.
		/// </summary>
		float _duration;
		#endregion

		#region constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="UnityEzExp.EzTimer"/> class.
		/// </summary>
		public EzTimer() 
		{
			_temporalState = TemporalState.NotStarted;
			_startTime = 0f;
			_breaks = new List<float>();
			_endTime = 0f;
			_duration = 0f;
		}

		/// <summary>
		/// Copy an instance of the <see cref="UnityEzExp.EzTimer"/>.
		/// </summary>
		/// <param name="toCopy">Instance to copy.</param>
		public EzTimer(EzTimer toCopy): this()
        {
			_temporalState = toCopy._temporalState;
			_startTime = toCopy._startTime;
			foreach(float b in toCopy._breaks) { _breaks.Add(b); }
			_endTime = toCopy._endTime;
			// forces duration to be computed before assigning it
			_duration = toCopy.GetRawDuration();
        }
		#endregion

		#region getters/setters
//		public double GetRawBaseTime() { return _baseTime; }
//		public TimeSpan GetBaseTime() { return TimeSpan.FromMilliseconds(_baseTime); }

		/// <summary>
		/// Gets starting time not formatted.
		/// </summary>
		/// <returns>Starting time.</returns>
		public float GetRawStartTime() 
		{ 
			if(_temporalState == TemporalState.NotStarted) { Log.Warning("Timer not started yet"); }
			return _startTime; 
		}
		/// <summary>
		/// Gets starting time formatted.
		/// </summary>
		/// <returns>Starting time.</returns>
		public TimeSpan GetStartTime() 
		{ 
			if(_temporalState == TemporalState.NotStarted) { Log.Warning("Timer not started yet"); }
			return TimeSpan.FromMilliseconds(_startTime); // WARNING: cast from float to double
		} 

		/// <summary>
		/// Gets ending time not formatted.
		/// </summary>
		/// <returns>Ending time.</returns>
		public float GetRawEndTime() 
		{ 
			if(_temporalState != TemporalState.Ended) { Log.Warning("Timer not ended yet"); } 
			return _endTime; 
		}
		/// <summary>
		/// Gets ending time formatted.
		/// </summary>
		/// <returns>Ending time.</returns>
		public TimeSpan GetEndime() 
		{ 
			if(_temporalState != TemporalState.Ended) { Log.Warning("Timer not ended yet"); } 
			return TimeSpan.FromMilliseconds(_endTime); // WARNING: cast from float to double
		} 

		/// <summary>
		/// Gets duration not formatted.
		/// </summary>
		/// <returns>Duration.</returns>
		public float GetRawDuration() { return _duration = ComputeDuration(); } // strange to change variable in get...
		/// <summary>
		/// Gets ending time formatted.
		/// </summary>
		/// <returns>Duration.</returns>
		public TimeSpan GetDuration() { return TimeSpan.FromMilliseconds(_duration = ComputeDuration()); } // WARNING: cast from float to double

		/// <summary>
		/// Gets timer state.
		/// </summary>
		/// <returns>State of the timer.</returns>
		public TemporalState GetState() { return _temporalState; }

		// TimeSpan already does that
//        public string GetTime(TimeFormat format = TimeFormat.MINUTES)
//        {
//			TimeSpan formattedTime = TimeSpan.FromMilliseconds(_duration);
//            string formattedValue = "";
//            switch (format) // TODO  : Do something with format
//            {
//                case TimeFormat.MILLISECONDS:
//					formattedValue = _duration+"";
//                    break;
//                case TimeFormat.SECONDS:
//					formattedValue = formattedTime.TotalMinutes;	
//					break;
//                case TimeFormat.MINUTES:
//                    formattedValue = total.Minutes  +  ":" + total.Seconds + "." + total.Milliseconds;
//                    break;
//            }
//            return formatedValue;
//        }
		#endregion

		#region functions
		/// <summary>
		/// Starts the timer and sets its starting time to the current time.
		/// </summary>
        public void Start()
        {
			if(_temporalState == TemporalState.Started) { throw new TimerAlreadyStartedException(); }
			else if(_temporalState == TemporalState.Ended) { throw new TimerEndedException(); }
			_startTime = Time.time*1000f; 
			_temporalState = TemporalState.Started;
        }

		/// <summary>
		/// Pauses the timer. It can be resumed using <see cref="UnityEzExp.EzTimer.Resume"/>.
		/// </summary>
		public void Pause()
		{
			if(_temporalState == TemporalState.NotStarted) { throw new TimerNotStartedException(); }
			else if(_temporalState == TemporalState.Ended) { throw new TimerEndedException(); }

			// odd number of breaks -> timer paused
			if(_breaks.Count % 2 == 1) { Log.Warning("Timer already paused"); }
			else { _breaks.Add(Time.time*1000f); }
		}

		/// <summary>
		/// Resumes the timer. It can be paused using <see cref="UnityEzExp.EzTimer.Pause"/>.
		/// </summary>
		public void Resume()
		{
			if(_temporalState == TemporalState.NotStarted) { throw new TimerNotStartedException(); }
			else if(_temporalState == TemporalState.Ended) { throw new TimerEndedException(); }

			// even number of breaks -> timer resumed
			if(_breaks.Count % 2 == 0) { Log.Warning("Timer already resumed"); }
			else { _breaks.Add(Time.time*1000f); }
		}

		/// <summary>
		/// Stops the timer and sets its ending time to the current time.
		/// </summary>
        public void Stop()
        {
			_endTime = Time.time*1000f;
			_temporalState = TemporalState.Ended;
			_duration = ComputeDuration();
        }

        /// <summary>
        /// Resets the timer to its initial settings (i.e. settings defined in its constructors).
        /// </summary>
        public void Reset()
        {
            _temporalState = TemporalState.NotStarted;
            _startTime = 0f;
            _breaks = new List<float>();
            _endTime = 0f;
            _duration = 0f;
        }

		/// <summary>
		/// Computes the duration based on starting time. If the timer is not yet ended, the computation is done anyway.
		/// </summary>
		/// <returns>The duration.</returns>
		float ComputeDuration()
		{
			if(_temporalState == TemporalState.Ended) { _duration = _endTime - _startTime; } 
			else 									  { _duration = Time.time*1000f - _startTime; }

			int count = _breaks.Count;
			if(count > 0) {
				// timer was resumed correctly after pausing
				if(count % 2 == 0) { 
					// remove from duration the time between pauses and resumes 
					for(int i = 0; i < count; i=i+2) { _duration -= _breaks[i+1] - _breaks[i]; } 
				}
				// timer is stilled paused
				else {
					// don't take into account last pause
					for(int i = 0; i < count-1; i=i+2) { _duration -= _breaks[i+1] - _breaks[i]; } 
				}
			}
			return _duration;
		}

//        public float GetTimeSeconds()
//        {
//            endTime = _originalStartTime.TotalMilliseconds;
//
//            TimeSpan total = TimeSpan.FromMilliseconds(endTime - startTime);
//
//            return (float)total.TotalSeconds;
//        }
		#endregion
    }
}