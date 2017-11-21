using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Uduino
{
    /*
    public interface IDevice
    {
        void Open(string port = null);
        bool WriteToArduino(string message, object value = null, bool instant = false);
        string ReadFromArduino(string message = null, int timeout = 0, bool instant = false);
        void ReadFromArduinoLoop();

        void Stopping();
        void Close();
    }
    */
    public class UduinoDevice //: IDevice
    {

        public string name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name == "")
                    _name = value;
            }
        }
        private string _name = "defaultBoard";

        public int _boardType = 0;

        public bool continuousRead = false;
        public string read = null;
        public string lastRead = null;
        public string lastWrite = null;
        private Dictionary<string, List<string>> bundles = new Dictionary<string, List<string>>();

        public System.Action<string> callback = null;

        public BoardStatus boardStatus = BoardStatus.Undef;

        //Messages reading
        public Queue readQueue, writeQueue, messagesToRead;
        public int maxQueueLength = 10;
        public bool autoRead = false;

        public int _baudrate = 9600;
        public string identity = "";

        virtual public int writeTimeout { get; set; }
        virtual public int readTimeout { get; set; }


        public UduinoDevice(int baudrate = 9600, int boardType = 0)
        {
            _baudrate = baudrate;
            _boardType = boardType;
            readQueue = Queue.Synchronized(new Queue());
            writeQueue = Queue.Synchronized(new Queue());
            messagesToRead = Queue.Synchronized(new Queue());
        }

        public virtual void Open()
        {
            boardStatus = BoardStatus.Open;
        }


        /// <summary>
        /// Return port status 
        /// </summary>
        /// <returns>BoardStatus</returns>
        public BoardStatus getStatus()
        {
            return boardStatus;
        }

        /// <summary>
        /// Return Identity 
        /// </summary>
        public string getIdentity()
        {
            return identity;
        }


        /// <summary>
        /// Add a message to the bundle
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="bundle">Bundle Name</param>
        public void AddToBundle(string message, string bundle)
        {
            List<string> existing;
            if (!bundles.TryGetValue(bundle, out existing))
            {
                existing = new List<string>();
                bundles[bundle] = existing;
            }
            existing.Add("," + message);
            //  Log.Debug("Message <color=#4CAF50>" + message + "</color> added to the bundle " + bundle);
        }

        /// <summary>
        /// Send a Bundle to the arduino
        /// </summary>
        /// <param name="bundleName">Name of the bundle to send</param>
        public void SendBundle(string bundleName)
        {
            List<string> bundleValues;
            if (bundles.TryGetValue(bundleName, out bundleValues))
            {
                string fullMessage = "b " + bundleValues.Count;

                if (bundleValues.Count == 1) // If there is one message
                {
                    string message = bundleValues[0].Substring(1, bundleValues[0].Length - 1);
                    if (message.Contains("r")) ReadFromArduino(message);
                    else WriteToArduino(message);
                    return;
                }

                for (int i = 0; i < bundleValues.Count; i++)
                    fullMessage += bundleValues[i];

                if (fullMessage.Contains("r")) ReadFromArduino(fullMessage);
                else WriteToArduino(fullMessage);

                if (fullMessage.Length >= 120)  /// TODO : Max Length, matching avec arduino
                    Log.Warning("The bundle message is too big. Try to not send too many messages or increase UDUINOBUFFER in Uduino library.");

                bundles.Remove(bundleName);
            }
            else
            {
                if (bundleName != "init" && bundleName != "destroy")
                    Log.Info("You are tring to send the bundle \"" + bundleName + "\" but it seems that it's empty.");
            }
        }

        public void SendAllBundles()
        {
            Log.Debug("Send all bundles");
            List<string> bundleNames = new List<string>(bundles.Keys);
            foreach (string key in bundleNames)
                SendBundle(key);
        }


        /* Read Write */
        public virtual bool WriteToArduino(string message, object value = null, bool instant = false)
        {
            if (message == null || message == "")
                return false;

            if (value != null)
                message = message + " " + value.ToString();

            message += "\r\n";

            lock (writeQueue)
            {
                if (!writeQueue.Contains(message) && writeQueue.Count < maxQueueLength)
                {
                    writeQueue.Enqueue(message);
                }
                if (message == "disconnected")
                {
                    writeQueue.Clear();
                    writeQueue.Enqueue(message);
                    instant = true;
                }
            }

            if (instant)
                return WriteToArduinoLoop();
            return true;

        }

        public virtual bool WriteToArduinoLoop() { return false; }
        public virtual string ReadFromArduino(string message = null, int timeout = 0, bool instant = false)
        {
            lock (messagesToRead)
            {
                if (message != null && messagesToRead.Count < maxQueueLength)
                {
                    messagesToRead.Enqueue(message);
                }
            }

            if (instant)
                ReadFromArduinoLoop();

            lock (readQueue)
            {
                if (readQueue.Count == 0)
                    return null;

                string finalMessage = (string)readQueue.Dequeue();
                return finalMessage;
            }
        }

        public virtual void ReadFromArduinoLoop()
        {
            lock (messagesToRead)
            {
                if (messagesToRead.Count > 0)
                {
                    if (!WriteToArduino((string)messagesToRead.Dequeue(), instant: true))
                        return;
                }
                else if (autoRead) { }
                else
                {
                    // Log.Debug("TODO BUG : It read a message only if a message r is sent ? Incompatible with alwaysread ?");
                    return;
                }
            }

        }

        public virtual void Stopping()
        {
            WriteToArduino("disconnected", instant: true);
            boardStatus = BoardStatus.Stopping;
        }

        public virtual void Close() { }


        public virtual void UduinoFound() {
            boardStatus = BoardStatus.Found;
#if UNITY_EDITOR
            if (Application.isPlaying) EditorUtility.SetDirty(UduinoManager.Instance);
#endif
        }


        /* Reading / Writing success */
        public virtual void MessageReceived(string message)
        {
            ReadingSuccess(message);
            if (message != null && readQueue.Count < maxQueueLength)
            {
                lock (readQueue)
                {
                    readQueue.Enqueue(message);
                }
            }
        }

        public virtual void WritingSuccess(string message)
        {
            lastWrite = message;
        }

        public virtual void ReadingSuccess(string message)
        {
            if (lastRead == null)
            {
                lastRead = message;
                return; // If the previous message was empty it's meaning that it was the first one; so we don't transmit it. 
            }
            else
                lastRead = message;

            if (UduinoManager.Instance)
            {
                UduinoManager.Instance.InvokeAsync(() =>
                {
                    if (message.Split(' ')[0] == "uduinoIdentity")
                        return;

                    if (callback != null)
                        callback(message);
                    UduinoManager.Instance.TriggerEvent(message, _name);
                    #if UNITY_EDITOR
                    if (Application.isPlaying) EditorUtility.SetDirty(UduinoManager.Instance);
                    #endif
                });
            }
            else if (!Application.isPlaying && callback != null) //if it's the editor
                callback(message);
        }



        /// Specal Handler when application quit;
        private bool isApplicationQuitting = false;

        void OnDisable()
        {
            if (isApplicationQuitting) return;
            Close();
        }

        public void ClearQueues()
        {
            WriteToArduinoLoop();

            lock (readQueue)
                readQueue.Clear();
            lock (writeQueue)
                writeQueue.Clear();
            lock (messagesToRead)
                messagesToRead.Clear();
        }

        void OnApplicationQuit()
        {
            isApplicationQuitting = true;
        }

    }
}