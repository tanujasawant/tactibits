﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Uduino
{
    // We use a class Pin to optimize
    public class Pin
    {
        private UduinoManager manager = null;
        public UduinoManager Manager
        {
            get
            {
                if (Application.isPlaying) return UduinoManager.Instance;
                else return manager;
            }
            set
            {
                manager = value;
            }
        }
        public string arduinoName = null;

        public PinMode pinMode;
        public PinMode prevPinMode;

        public int currentPin = -1;

        [SerializeField]
        public int sendValue = 0;
        public int prevSendValue = 0;

        public int lastReadValue = 0;

        public Pin(string arduinoParent, int pin, PinMode mode)
        {
            Manager = UduinoManager.Instance;
            arduinoName = arduinoParent;
            currentPin = pin;
            pinMode = mode;
        }

        public void Init(bool useInit = false)
        {
            ChangePinMode(pinMode, useInit? "init" : null);
        }

        public virtual void WriteReadMessage(string message)
        {
            Manager.Write(arduinoName, message);
            //TODO : ref to bundle? 
            //TODO : Add ref to arduinocard
        }

        public virtual void WriteMessage(string message, string bundle = null)
        {
            Manager.Write(arduinoName, message, bundle);
        }

        public bool PinTargetExists(string parentArduinoTarget, int currentPinTarget)
        {
            if ((arduinoName == null || arduinoName == "" || parentArduinoTarget == null || parentArduinoTarget == "" || parentArduinoTarget == arduinoName) && currentPinTarget == currentPin )
                return true;
            else
                return false;
        }

        /// <summary>
        /// Change Pin mode
        /// </summary>
        /// <param name="mode">Mode</param>
        public void ChangePinMode(PinMode mode, string bundle = null)
        {
            pinMode = mode;
            WriteMessage("s " + currentPin + " " + (int)pinMode, bundle);
        }

        /// <summary>
        /// Send OptimizedValue
        /// </summary>
        /// <param name="sendValue">Value to send</param>
        public virtual void SendRead(string bundle = null, System.Action<string> action = null, string digital = "")
        {
            string cmd = "r" + digital;
            if (bundle != null) cmd = "br";
            Manager.Read(arduinoName, cmd + " " + currentPin, action: action, bundle: bundle);
        }

        /// <summary>
        /// Send OptimizedValue
        /// </summary>
        /// <param name="sendValue">Value to send</param>
        public void SendPinValue(int sendValue, string typeOfPin, string bundle = null)
        {
            if (sendValue != prevSendValue)
            {
                this.sendValue = sendValue;
                WriteMessage(typeOfPin + " " + currentPin + " " + sendValue, bundle);
                prevSendValue = sendValue;
            }
        }

        public void Destroy()
        {
            if(pinMode == PinMode.Output)
                WriteMessage("d " + currentPin + " 0","destroy");
            else if (pinMode == PinMode.PWM || pinMode == PinMode.Analog)
                WriteMessage("a " + currentPin + " 0", "destroy");
        }

        public virtual void Draw()
        {
            //Function overrided by the Editor
        }

        public virtual void CheckChanges() { }
    }
}