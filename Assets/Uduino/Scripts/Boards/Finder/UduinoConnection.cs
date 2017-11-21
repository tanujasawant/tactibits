using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Uduino
{
    public class UduinoConnection
    {
        public UduinoManager _manager = null;
       
        public static T GetInstance<T>()
        {
            return (T)System.Activator.CreateInstance(typeof(T));
        }


        public static UduinoConnection GetFinder(UduinoManager manager, Platform p, ConnectionMethod m)
        {
            UduinoConnection connection = null;

#if UNITY_ANDROID
    #if UNITY_EDITOR //IF it's on the editor
            UduinoExtension u;
            if (manager.AvailableExtensions["UduinoDevice_AndroidSerial"].isActive)
                Log.Info("Uduino for Android Serial is active but you are in the editor. Switching platform.");
            connection = new UduinoConnection_DesktopSerial();
#else //Get the  Android Serial Plugin
         if (UduinoManager.Instance.AvailableExtensions["UduinoDevice_AndroidSerial"].isPresent) {
             if (UduinoManager.Instance.AvailableExtensions["UduinoDevice_AndroidSerial"].isActive)
                connection = new UduinoConnection_AndroidSerial();
            else 
                Log.Error("Uduino for Android Serial is not active ! Activate it in the Inspector Panel");
         } else {
           Log.Error("Uduino for Android Serial is not present ! Are you sure it's imported in your project ?");
          }
#endif

#else // default 
          connection = new UduinoConnection_DesktopSerial();
#endif
            return connection;
        }

        public UduinoConnection(UduinoManager manager = null)
        {
            _manager = manager;
        }

        virtual protected void Setup() { }

        public virtual void FindBoards(UduinoManager manager)
        {
            _manager = manager;
        }

        public virtual UduinoDevice OpenUduinoDevice(string id = null)
        {
            Log.Debug("No Uduino board type setup");
            return null;
        }

        /// <summary>
        /// Find a board connected to a specific port
        /// </summary>
        /// <param name="portName">Port open</param>
        public virtual IEnumerator DetectUduino(UduinoDevice uduinoDevice)
        {
            int tries = 0;
            do
            {
                if (uduinoDevice.getStatus() == BoardStatus.Open)
                {
                    string reading = uduinoDevice.ReadFromArduino("identity", instant: true);
                    if (reading != null && reading.Split(new char[0])[0] == "uduinoIdentity")
                    {
                        string name = reading.Split(new char[0])[1];
                        uduinoDevice.name = name;
                        _manager.AddUduinoBoard(name, uduinoDevice);
                        uduinoDevice.UduinoFound();

                        if (!_manager.ReadOnThread)
                            _manager.StartCoroutine(_manager.CoroutineRead(name)); // Initiate the Async reading of variables 
                        else
                            _manager.StartThread();

                        uduinoDevice.WriteToArduino("connected");
                        _manager.InitAllArduinos();
                        break;
                    }
                    else
                    {
                        Log.Debug("Impossible to get name on <color=#2196F3>[" + uduinoDevice.identity + "]</color>. Retrying.");
                    }
                }
                yield return new WaitForSeconds(0.05f);    //Wait one frame. Todo : use yield return new WaitForSeconds(0.5f); ?
                // yield return null;    //Wait one frame. Todo : use yield return new WaitForSeconds(0.5f); ?
            } while (uduinoDevice.getStatus() != BoardStatus.Undef && tries++ < _manager.DiscoverTries);

            if (uduinoDevice.getStatus() != BoardStatus.Found)
            {
                Log.Warning("Impossible to get name on <color=#2196F3>[" + uduinoDevice.identity + "]</color>. Closing.");
                uduinoDevice.Close();
                uduinoDevice = null;
            }
        }

        public virtual void PluginReceived(string message) { }
        public virtual void PluginWrite(string message) { }
        public virtual void CloseDevices() { }
    }
}