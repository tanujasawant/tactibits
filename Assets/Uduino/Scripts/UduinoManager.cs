/*
 * Uduino - Arduino-Unity Library
 * Version 1.4.3, Jan 2017, Marc Teyssier
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
#if UDUINO_READY
using System.IO.Ports;
#endif


namespace Uduino
{
#region Enums
    public enum PinMode
    {
        Output,
        PWM,
        Analog,
        Input_pullup,
        Servo
    };

    public enum AnalogPin { A0 = 14, A1 = 15, A2 = 16, A3 = 17, A4 = 18, A5 = 19 };

    public enum State
    {
        LOW,
        HIGH
    };

    public enum BoardStatus
    {
        Undef,
        Open,
        Found,
        Stopping,
        Closed
    };

    public enum LogLevel
    {
        DEBUG,
        INFO,
        WARNING,
        ERROR,
        NONE
    };

    public enum Platform
    {
        Auto,
        Desktop,
        Android
    };

    public enum ConnectionMethod
    {
        Default,
        Serial,
        Network,
        Bluetooth
    };
    #endregion

    [Serializable]
     public struct UduinoExtension {
        public string name;
        public bool isPresent;
        public bool isActive;
    }

    public class UduinoManager : MonoBehaviour {
        #region Singleton
        /// <summary>
        /// UduinoManager unique instance.
        /// Create  a new instance if any UduinoManager is present on the scene.
        /// Set the Uduinomanager only on the first time.
        /// </summary>
        /// <value>UduinoManager static instance</value>
        public static UduinoManager Instance
        {
            get {
                if (_instance != null)
                    return _instance;

                UduinoManager[] uduinoManagers = FindObjectsOfType(typeof(UduinoManager)) as UduinoManager[];
                if (uduinoManagers.Length == 0 )
                {
                    Log.Warning("UduinoManager not present on the scene. Creating a new one.");
                    UduinoManager manager = new GameObject("UduinoManager").AddComponent<UduinoManager>();
                    _instance = manager;
                    return _instance;
                }
                else
                    return _instance;
            }
            set {
                if(Instance == null)
                    _instance = value;
                else
                {
                    Log.Error("You can only use one UduinoManager. Destroying the new one attached to the GameObject " + value.gameObject.name);
                    Destroy(value);
                }
            }
        }
        private static UduinoManager _instance = null;
        #endregion

        #region Variables
        /// <summary>
        /// Dictionnary containing all the connected Arduino devices
        /// </summary>
        public Dictionary<string, UduinoDevice> uduinoDevices = new Dictionary<string, UduinoDevice>();

        /// <summary>
        /// List containing all active pins
        /// </summary>
        public List<Pin> pins = new List<Pin>();

        /// <summary>
        /// List containing all the available extensions
        /// </summary>
        [SerializeField]
        private DictionaryExtensions availableExtensions = new DictionaryExtensions() {
            { "UduinoDevice_DesktopSerial", new UduinoExtension{name = "Desktop Serial", isPresent = true, isActive =  true } },
            { "UduinoDevice_AndroidSerial", new UduinoExtension{name = "Android Serial", isPresent = false, isActive =  false } }
        };
        public DictionaryExtensions AvailableExtensions
        {
            get { return availableExtensions; }
            set { if (availableExtensions == value) return; availableExtensions = value; }
        }
        /// <summary>
        /// List containing all the board types
        /// </summary>
        Dictionary<string, int> boardTypeNames = new Dictionary<string, int>();

        /// <summary>
        /// Dictionnary containing all the connected Arduino devices
        /// </summary>
        public Dictionary<string, Action<string>> autoReads = new Dictionary<string, Action<string>>();




        UduinoConnection boardConnection = null;

        /// <summary>
        /// Create a delegate event to trigger the function OnValueReceived()
        /// Takes one parameter, the returned data.
        /// </summary>
        public delegate void OnValueReceivedEvent(string data, string device);
        public event OnValueReceivedEvent OnValueReceived;

        /// <summary>
        /// Variables for the async trigger of functions
        /// </summary>
        private object _lockAsync = new object();

        private System.Action _callbacksAsync;

        /// <summary>
        /// Log level
        /// </summary>
        [SerializeField]
        public LogLevel debugLevel;

        /// <summary>
        /// BaudRate
        /// </summary>
        [SerializeField]
        private int baudRate = 9600;
        public int BaudRate {
            get { return baudRate; }
            set { baudRate = value; }
        }

        /// <summary>
        /// Enable the reading of serial port in a different Thread.
        /// Might be usefull for optimization and not block the runtime during a reading. 
        /// </summary>
        [SerializeField]
        private bool readOnThread = true;
        public bool ReadOnThread
        {
            get { return readOnThread; }
            set {
                if (Application.isPlaying && readOnThread != value)
                {
                    if (value)
                    {
                        StopAllCoroutines();
                        StartThread();
                    }
                    else
                    {
                        StopThread();
                        StartCoroutine(CoroutineRead());
                    }
                }
                readOnThread = value;
            }
        }

        /// <summary>
        /// Limitation of the send rate
        /// Packing into bundles
        /// </summary>
        [SerializeField]
        private bool limitSendRate = false;
        public bool LimitSendRate
        {
            get { return limitSendRate; }
            set {
                if (limitSendRate == value)
                    return;
               if (Application.isPlaying)
               {
                    if (value && !autoSendIsRunning)
                    {
                        Log.Debug("Start auto read");
                        StartCoroutine("AutoSendBundle");
                        autoSendIsRunning = true;
                    }
                    else
                    {
                        Log.Debug("Stop auto read");
                        StopCoroutine("AutoSendBundle");
                        autoSendIsRunning = false;
                    }
               }
                limitSendRate = value;
            }
        }
        private bool autoSendIsRunning = false;

        public int readTimeout = 100;

        public int writeTimeout = 100;

        public bool autoRead = false;

        public int defaultArduinoBoardType = 0;

		public bool useCuPort = false;

        /// <summary>
        /// SendRateSpeed
        /// </summary>
        [SerializeField]
        private int sendRateSpeed = 20;
        public int SendRateSpeed
        {
            get { return sendRateSpeed; }
            set { sendRateSpeed = value; }
        }

        /// <summary>
        /// Number of tries to discover the attached serial ports
        /// </summary>
        [SerializeField]
        private int discoverTries = 20;
        public int DiscoverTries
        {
            get { return discoverTries; }
            set { discoverTries = value; }
        }

        /// <summary>
        /// Discover serial ports on Awake
        /// </summary>
        public bool autoDiscover = true;

        /// <summary>
        /// Stop all digital/analog pin on quit
        /// </summary>
        public bool stopAllOnQuit = true;

        /// <summary>
        /// List of black listed ports
        /// </summary>
        [SerializeField]
        private List<string> blackListedPorts = new List<string>();
        public List<string> BlackListedPorts {
            get { return blackListedPorts; }
            set { blackListedPorts = value; }
        }

        Platform platformType = Platform.Auto;
        ConnectionMethod connectionMethod = ConnectionMethod.Default;
        #endregion

        #region Init
        void Awake()
        {
            #if UDUINO_READY
            Instance = this;

            FullReset();
            Log.SetLogLevel(debugLevel);

            if(autoDiscover)
                DiscoverPorts();

            //TODO:  OnValueReceived += DefaultOnValueReceived;
            StopCoroutine("AutoSendBundle");

            if (limitSendRate)
                StartCoroutine("AutoSendBundle");

#endif
        }

#endregion

#region Ports discovery

         /// <summary>
        /// Get the ports names, dependings of the current OS
        /// </summary>
        public void DiscoverPorts()
        {
            CloseAllPorts();
            if(boardConnection == null)
                boardConnection = UduinoConnection.GetFinder(this, platformType, connectionMethod);
            boardConnection.FindBoards(this);
        }

        public void AddUduinoBoard(string name, UduinoDevice board)
        {
            lock (uduinoDevices)
            {
                uduinoDevices.Add(name, board);
                Log.Warning("Board <color=#ff3355>" + name + "</color> <color=#2196F3>[" + board.getIdentity() + "]</color> added.");
            }
        }

        /// <summary>
        /// Debug ports state. TODO : Move to Editor script
        /// </summary>
        public void GetPortState()
        {
            if (uduinoDevices.Count == 0)
            {
                Log.Info("Trying to close and no port are currently open");
            }
            foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
            {
#if UDUINO_READY
                Debug.Log("todo");// string state = uduino.Value.serial.IsOpen ? "open " : "closed";
                //TODOLog.Info("" + uduino.Value.getIdentity() + " (" + uduino.Key + ")" + " is " + state);
            #endif
            }
        }

#endregion

#region BoardType 
        /// <summary>
        /// Set the board type, when one board only is connected
        /// </summary>
        /// <param name="type">Type of the board</param>
        public void SetBoardType(string type)
        {
            SetBoardType("", type);
        }

        /// <summary>
        /// Set the board type of a specific arduino board
        /// </summary>
        /// <param name="target">Target board</param>
        /// <param name="type">Board type</param>
        public void SetBoardType(string target, string type)
        {
            int boardId = BoardsTypeList.Boards.GetBoardIdFromName(type);
            SetBoardType(target, boardId);
        }

        /// <summary>
        /// Set the board type of a specific arduino board
        /// </summary>
        /// <param name="target">Target board</param>
        /// <param name="boardId">Board ID, in the BoardType registered List</param>
        void SetBoardType(string target, int boardId)
        {
            if (target == null) target = "";
            if (boardTypeNames.ContainsKey(target))
            {
                Log.Debug("You already setup the type for the board" + target);
                return;
            }
            boardTypeNames.Add(target, boardId);
            if (UduinoTargetExists(target))
                uduinoDevices[target]._boardType = boardId;
        }
        
        /// <summary>
        /// Get the pin from specific board type
        /// </summary>
        /// <param name="boardType">Board type</param>
        /// <param name="pin">Pin to find</param>
        /// <returns>Int of the pin</returns>
        public int GetPinFromBoard(string boardType, string pin)
        {
            return BoardsTypeList.Boards.GetBoardFromName(boardType).GetPin(pin);
        }

        /// <summary>
        /// Get the pin from specific board type
        /// </summary>
        /// <param name="boardType">Board type</param>
        /// <param name="pin">Pin to find</param>
        /// <returns>Int of the pin</returns>
        public int GetPinFromBoard(string boardType, int pin)
        {
            return BoardsTypeList.Boards.GetBoardFromName(boardType).GetPin(pin);
        }

        /// <summary>
        /// Get the specific pin from the current board, if the board is already set with SetBoardType
        /// </summary>
        /// <param name="pin">Pin to find</param>
        /// <returns>Int of the pin</returns>
        public int GetPinFromBoard(string pin)
        {
            var e = uduinoDevices.GetEnumerator();
            e.MoveNext();
            UduinoDevice anElement = e.Current.Value;

            int currentBoardType = anElement._boardType;
            return BoardsTypeList.Boards.GetBoardFromId(currentBoardType).GetPin(pin);
        }

        /// <summary>
        /// Get the specific pin from the current board, if the board is already set with SetBoardType
        /// </summary>
        /// <param name="pin">Pin to find</param>
        /// <returns>Int of the pin</returns>
        public int GetPinFromBoard(int pin)
        {
            return GetPinFromBoard(pin+"");
        }
#endregion

#region Simple commands : Pin setup
        /// <summary>
        /// Initialize an arduino pin
        /// </summary>
        /// <param name="pin">Pin to initialize</param>
        /// <param name="mode">PinMode to init pin</param>
        public void InitPin(int pin, PinMode mode)
        {
            InitPin(null, pin, mode);
        }

        /// <summary>
        /// Init a pin
        /// </summary>
        /// <param name="pin">Analog pin to initialize</param>
        /// <param name="mode">PinMode to init pin</param>
        public void InitPin(AnalogPin pin, PinMode mode)
        {
            InitPin(null, (int)pin, mode);
        }

        /// <summary>
        /// Create a new Pin and setup the mode if the pin is not registered.
        /// If the pin exsists, change only the mode
        /// </summary>
        /// <param name="string">Target Name</param>
        /// <param name="pin">Pin to init</param>
        /// <param name="mode">PinMode to init pin</param>
        public void InitPin(string target, int pin, PinMode mode)
        {
            if (target == null) target = "";
            bool pinExists = false;

            foreach (Pin pinTarget in pins)
            {
                if (pinTarget.PinTargetExists(target, pin) && pinTarget.pinMode != mode)
                {
                    Log.Debug("Override pinMode for the pin <color=#4CAF50>" + pin + "</color> on the arduino <color=#ff3355>" + target + "</color> from <color=#2e7d32>" + pinTarget.pinMode + "</color> to <color=#2e7d32>" + mode + "</color>.");
                    pinTarget.ChangePinMode(mode);
                    pinExists = true;
                }
            }
            if (!pinExists)
            {
                Pin newPin = new Pin(target, pin, mode);
                pins.Add(newPin);
                Log.Debug("Set pinMode of <color=#4CAF50>" + pin + "</color> on the arduino <color=#ff3355>" + target + "</color> to <color=#2e7d32>" + mode + "</color>");
                if (UduinoTargetExists(target) || (target == "" && uduinoDevices.Count != 0))
                    newPin.Init();
            }
        }

        /// <summary>
        /// Init a pin
        /// </summary>
       /// <param name="string">Target Name</param>
        /// <param name="pin">Pin to init</param>
        /// <param name="mode">PinMode to init pin</param>
        public void InitPin(string target, AnalogPin pin, PinMode mode)
        {
            InitPin((int)pin, mode);
        }

        // TODO : Test with multiple boards ! If dosnt' work, refactor that. 
        /// <summary>
        /// Init all Pins when the arduino boards are found
        /// </summary>
        public void InitAllPins()
        {
            foreach(Pin pin in pins)
            {
                pin.Init(true);
            }
            Log.Debug("Init all pins");
            SendBundle("init");
        }

        public void InitAllBoardType()
        {
            foreach (KeyValuePair<string, int> boardType in boardTypeNames)
            {
                SetBoardType(boardType.Key, boardType.Value);
            }
        }

        public void InitAllArduinos()
        {
            InitAllBoardType();
            InitAllPins();
            InitAutoRead();
        }

#endregion

#region Simple commands : Write

        /// <summary>
        /// DigitalWrite or AnalogWrite to arduino
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pin"></param>
        /// <param name="value"></param>
        public void arduinoWrite(string target, int pin, int value, string typeOfPin, string bundle = null)
        {
            bool onPinExists = false;
            foreach (Pin pinTarget in pins)
            {
                if (pinTarget.PinTargetExists(target, pin))
                {
                    pinTarget.SendPinValue(value, typeOfPin, bundle);
                    onPinExists = true;
                }
            }
            if (!onPinExists)
                Log.Info("You are trying to send a message to the pin " + pin + " but this pin is not initialized. \r\nUse the function UduinoManager.Instance.InitPin(..)");
        }

        /// <summary>
        /// Write a digital command to the arduino
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pin"></param>
        /// <param name="value"></param>
        public void digitalWrite(string target, int pin, int value, string bundle = null)
        {
            if (value <= 150) value = 0;
            else value = 255;
            arduinoWrite(target,pin,value,"d", bundle);
        }

        /// <summary>
        /// Write a command on an Arduino
        /// </summary>
        public void digitalWrite(int pin, int value, string bundle = null)
        {
            digitalWrite("", pin, value, bundle);
        }

        /// <summary>
        /// Write a command on an Arduino
        /// </summary>
        public void digitalWrite(int pin, State state = State.LOW, string bundle = null)
        {
            arduinoWrite("", pin, (int)state * 255,"d", bundle);
        }

        /// <summary>
        /// Write a command on an Arduino
        /// </summary>
        public void digitalWrite(string target, int pin, State state = State.LOW, string bundle = null)
        {
            arduinoWrite(target, pin, (int)state * 255, "d", bundle);
        }

        /// <summary>
        /// Write an analog value to Arduino
        /// </summary>
        /// <param name="pin">Arduino Pin</param>
        /// <param name="value">Value</param>
        public void analogWrite(int pin, int value, string bundle = null)
        {
            arduinoWrite(null, pin, value, "a", bundle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">Arduino board</param>
        /// <param name="pin">Arduino Pin</param>
        /// <param name="value">Value</param>
        public void analogWrite(string target, int pin, int value, string bundle = null)
        {
            arduinoWrite(target, pin, value, "a", bundle);
        }

#endregion

#region Simple commands: Read
        public int analogRead(string target, int pin, string bundle = null)
        {
            int readVal = 0;

            foreach (Pin pinTarget in pins)
            {
                if (pinTarget.PinTargetExists(target, pin))
                {
                    pinTarget.SendRead(bundle, ParseAnalogReadValue);
                    readVal = pinTarget.lastReadValue;
                    Debug.Log(pinTarget.lastReadValue);
                }
            }

            return readVal;
        }

        public int analogRead(int pin, string bundle = null)
        {
            return analogRead("", pin, bundle);
        }

        public int analogRead(AnalogPin pin, string bundle = null)
        {
            return analogRead("", (int)pin, bundle);
        }

        //TODO : Add ref to the card 
        public void ParseAnalogReadValue(string data/*, string target =null*/)
        {
            if (data == null || data == "")
                return;

            string[] parts = data.Split('-');
            int max = 0;
            if (parts.Length == 1) max = 1;
            else max = parts.Length - 1;
            try
            {
                for (int i = 0; i < max; i++)
                {
                    string[] subParts = parts[i].Split(' ');
                    if (subParts.Length != 2)
                        return;
                    int recivedPin = -1;
                    recivedPin = int.Parse(subParts[0]);

                    int value =  int.Parse(subParts[1]);
                    if (recivedPin != -1)
                        dispatchValueForPin("", recivedPin, value);
                }
            }
            catch (FormatException)
            {

            }
        }

        /// <summary>
        /// Dispatch received value for a pin
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pin"></param>
        /// <param name="message"></param>
        /// <returns>readVal value</returns>
        public int dispatchValueForPin(string target, int pin, int readVal)
        {
            foreach (Pin pinTarget in pins)
            {
                if (pinTarget.PinTargetExists(target, pin))
                {
                   pinTarget.lastReadValue = readVal;
                }
            }
            return readVal;
        }

#endregion

#region Commands
        /// <summary>
        /// Send a read command to a specific arduino.
        /// A read command will be returned in the OnValueReceived() delegate function
        /// </summary>
        /// <param name="target">Target device name. Not defined means read everything</param>
        /// <param name="variable">Variable watched, if defined</param>
        /// <param name="timeout">Read Timeout, if defined </param>
        /// <param name="callback">Action callback</param>
        public void Read(string target = null, string message = null, int timeout = 0, Action<string> action = null, string bundle = null)
        {
            if (bundle != null)
            {
                if (UduinoTargetExists(target))
                {
                    uduinoDevices[target].callback = action;
                    uduinoDevices[target].AddToBundle(message, bundle);
                }
                else
                    foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                    {
                        uduino.Value.callback = action;
                        uduino.Value.AddToBundle(message, bundle);
                    }
            }
            else
            {
                if (UduinoTargetExists(target))
                {
                    uduinoDevices[target].callback = action;
                    uduinoDevices[target].ReadFromArduino(message, timeout);
                }
                else
                {
                    foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                    {
                        uduino.Value.callback = action;
                        uduino.Value.ReadFromArduino(message, timeout);
                    }
                }
            }
        }


        public void DirectReadFromArduino(string target = null, string message = null, int timeout = 0, Action<string> action = null, string bundle = null)
        {
            if (bundle != null)
            {
                if (UduinoTargetExists(target))
                    uduinoDevices[target].AddToBundle(message, bundle);
                else
                    foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                        uduino.Value.AddToBundle(message, bundle);
            }
            else
            {
                if (UduinoTargetExists(target))
                {
                    uduinoDevices[target].callback = action;
                    uduinoDevices[target].ReadFromArduino(message);
                }
                else
                {
                    foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                    {
                        uduino.Value.callback = action;
                        uduino.Value.ReadFromArduino(message, timeout);
                    }
                }
            }
        }

        //TODO : Too much overload ? Bundle ? 
        public void Read(int pin, string target = null, Action<string> action = null) //TODO : ref timeout ? 
        {
            DirectReadFromArduino(action: action);
        }

        public void Read(int pin, Action<string> action = null)
        {
            DirectReadFromArduino(action: action);
        }

        public void AlwaysRead(string target = null, Action<string> action = null)
        {
            if (target == null) target = "allBoards";
            autoReads.Add(target, action);
            InitAutoRead();
        }

        public void InitAutoRead()
        {
            foreach (KeyValuePair<string, Action<string>> autoReadElem in autoReads)
            {
                string target = autoReadElem.Key;
                if (UduinoTargetExists(target))
                {
                    uduinoDevices[target].autoRead = true;
                    uduinoDevices[target].callback = autoReadElem.Value;
                }
                else if(target == "allBoards")
                {
                    Log.Debug("Init auto read on all boards");
                    foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                    {
                        uduino.Value.autoRead = true;
                        uduino.Value.callback = autoReadElem.Value;
                    }
                }
            }
        }

#endregion

#region Write advanced commands
        /// <summary>
        /// Write a command on an Arduino
        /// </summary>
        /// <param name="target">Target device</param>
        /// <param name="message">Message to write in the serial</param>
        public void Write(string target = null, string message = null, string bundle = null)
        {
            if (bundle != null || limitSendRate)
            {
                if (limitSendRate) bundle = "LimitSend";
                if (UduinoTargetExists(target))
                    uduinoDevices[target].AddToBundle(message, bundle);
                else
                    foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                        uduino.Value.AddToBundle(message, bundle);
            }
            else
            {
                if (UduinoTargetExists(target))
                    uduinoDevices[target].WriteToArduino(message);
                else
                    foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                        uduino.Value.WriteToArduino(message);
            }
        }

        /// <summary>
        /// Write a command on an Arduino with a specific value  
        /// </summary>
        /// <param name="target">Target device</param>
        /// <param name="message">Message to write in the serial</param>
        /// <param name="value">Optional value</param>
        public void Write(string target, string message, int value) {
            if (UduinoTargetExists(target))
                uduinoDevices[target].WriteToArduino(message, value);
            else
                foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                    uduino.Value.WriteToArduino(message,value);
        }


        /// <summary>
        /// Verify if the target exists when we want to get a value
        /// </summary>
        /// <param name="target">Target Uduino Name</param>
        /// <returns>Re</returns>
        private bool UduinoTargetExists(string target)
        {
            if (target == "" || target == null) return false;
            if (uduinoDevices.ContainsKey(target))
                return true;
            else
            {
                if(target != null && target != "" && target != "allBoards")
                    Log.Warning("The object " + target + " cannot be found. Are you sure it's connected and correctly detected ?");
                return false;
            }
        }

#endregion

#region Bundle
        /// <summary>
        /// Send an existing message bundle to Arduino
        /// </summary>
        /// <param name="target">Target arduino</param>
        /// <param name="bundleName">Bundle name</param>
        public void SendBundle(string target, string bundleName)
        {
            if (UduinoTargetExists(target))
                uduinoDevices[target].SendBundle(bundleName);
            else
                foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                    uduino.Value.SendBundle(bundleName);
        }

        /// <summary>
        /// Send an existing message bundle to Arduino
        /// </summary>
        /// <param name="bundleName">Bundle name</param>
        public void SendBundle(string bundleName)
        {
            SendBundle(null, bundleName);
        }

        /// <summary>
        /// Automatically send bundles
        /// </summary>
        /// <returns>Delay before next sending</returns>
        IEnumerator AutoSendBundle()
        {
            while (true)
            {
                if (!LimitSendRate)
                    yield return null;

                yield return new WaitForSeconds(sendRateSpeed / 1000.0f);
                List<string> keys = new List<string>(uduinoDevices.Keys);
                foreach (string key in keys)
                    uduinoDevices[key].SendAllBundles();
            }
        }

#endregion

#region Hardware reading
        /// <summary>
        /// Threading variables
        /// </summary>
        private Thread _thread = null;
        private bool threadRunning = true;

        /// <summary>
        /// Initialisation of the Thread reading on Awake()
        /// </summary>
        public void StartThread()
        {
            if (Application.isPlaying && _thread == null && readOnThread)
            {
                Log.Debug("Starting read/write thread");
                try
                {
                    _thread = new Thread(new ThreadStart(ReadPorts));
                    threadRunning = true;
                    _thread.Start();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void StopThread()
        {
            threadRunning = false;
        }

        public bool IsRunning()
        {
            return threadRunning;
        }
        
        void Update()
        {
            //Async Call
            Action tmpAction = null;
            lock (_lockAsync)
            {
                if (_callbacksAsync != null)
                {
                    tmpAction = _callbacksAsync;
                    _callbacksAsync = null;
                }
            }
            if (tmpAction != null) tmpAction();

            // Threading Loop
            if (_thread != null && !isApplicationQuiting && _thread.ThreadState == ThreadState.Stopped)
            {
                Log.Warning("Resarting Thread");
                StartThread();
            }
        }

        /// <summary>
        ///  Read the Serial Port data in a new thread.
        /// </summary>
        public void ReadPorts()
        {
#if UNITY_ANDROID
            if (availableExtensions["UduinoDevice_AndroidSerial"].isPresent && availableExtensions["UduinoDevice_AndroidSerial"].isActive)
                AndroidJNI.AttachCurrentThread(); // Sepcific android related code
#endif
            while (IsRunning() && !isApplicationQuiting)
            {
                lock (uduinoDevices)
                {
                    foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                    {
                        uduino.Value.WriteToArduinoLoop();
                        uduino.Value.ReadFromArduinoLoop();
                        Thread.Sleep(16);
                    }
                }
                if (limitSendRate) Thread.Sleep((int)sendRateSpeed / 2);
            }
        }

        /// <summary>
        /// Used for Editor
        /// </summary>
        /// <param name="target"></param>
        public void ReadWriteArduino(string target)
        {
            foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
            {
                uduino.Value.ReadFromArduinoLoop();
                uduino.Value.WriteToArduinoLoop();
            }
        }

        /// <summary>
        /// Retreive the Data from the Serial Prot using Unity Coroutines
        /// </summary>
        /// <param name="target"></param>
        /// <returns>null</returns>
        public IEnumerator CoroutineRead(string target = null)
        {
            while (true)
            {
                UduinoDevice uduino = null;

                if (target != null && uduinoDevices.TryGetValue(target, out uduino))
                {
                    uduino.WriteToArduinoLoop();
                    if (uduino.read != null)
                    {
                        uduino.ReadFromArduino(uduino.read);
                        uduino.ReadFromArduinoLoop();
                        yield return null;
                    }
                    else
                    {
                        yield return null;
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, UduinoDevice> uduinoDevice in uduinoDevices)
                    {
                        uduinoDevice.Value.ReadFromArduinoLoop();
                        uduinoDevice.Value.WriteToArduinoLoop();
                    }
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Trigger an async event, from the thread read to the main thread
        /// </summary>
        /// <param name="data">Message received</param>
        /// <param name="device">Device who receive the message</param>
        public void TriggerEvent(string data, string device)
        {
            InvokeAsync(() =>
            {
                if (OnValueReceived != null)
                    OnValueReceived(data, device);
            });
        }

        /// <summary>
        /// Invoke a function from a read thead to the main thread
        /// </summary>
        /// <param name="callback">Callback functions</param>
        public void InvokeAsync(Action callback)
        {
            lock (_lockAsync)
            {
                _callbacksAsync += callback;
            }
        }
#endregion

#region Close Ports
        /// <summary>
        /// Close all opened serial ports
        /// </summary>
        public void CloseAllPorts()
        {
            if (uduinoDevices.Count == 0)
            {
                Log.Debug("Ports already closed.");
                return;
            }

            if(stopAllOnQuit)
            {
                foreach (Pin pinTarget in pins)
                    pinTarget.Destroy();
            }

            foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
            {
                uduino.Value.SendBundle("destroy");
                uduino.Value.Stopping();
            }

            lock (uduinoDevices)
            {
                foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                    uduino.Value.Close();

                uduinoDevices.Clear();
            }
        }

        bool isApplicationQuiting = false;
        void OnApplicationQuit()
        {
            isApplicationQuiting = true;
            FullReset();
        }

        void OnDisable()
        {
            FullReset();
        }

        public void FullReset()
        {
            if (uduinoDevices.Count != 0)
                CloseAllPorts();
            StopAllCoroutines();
            DisableThread();
        }

        void DisableThread()
        {
            StopThread();
            if (_thread != null)
            {
                _thread.Join();
            }
            _thread = null;
        }
#endregion
    }

#region Version
    public static class UduinoVersion
    {
        static int major = 1;
        static int minor = 4;
        static int patch = 3;
        static string update = "October 2017";

        public static string getVersion()
        {
            return major + "." + minor + "." + patch;
        }

        public static string lastUpdate()
        {
            return update;
        }
    }
    #endregion

#region Utils 
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys = new List<TKey>();

        [SerializeField]
        private List<TValue> values = new List<TValue>();

        // save the dictionary to lists
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        // load dictionary from lists
        public void OnAfterDeserialize()
        {
            this.Clear();

            if (keys.Count != values.Count)
                throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

            for (int i = 0; i < keys.Count; i++)
                this.Add(keys[i], values[i]);
        }
    }

    [Serializable] public class DictionaryExtensions : SerializableDictionary<string, UduinoExtension> { }
    #endregion
}