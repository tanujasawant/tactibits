﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UDUINO_READY
using System.IO.Ports;
#endif

namespace Uduino
{
    public class UduinoConnection_DesktopSerial : UduinoConnection
    {
        public UduinoConnection_DesktopSerial() : base() { }

        public override void FindBoards(UduinoManager manager)
        {
            base.FindBoards(manager);
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
	     Discover(GetUnixPortNames());
#else
            Discover(GetWindowsPortNames());
#endif
        }

    /// <summary>
    /// Get the ports names if the system is on unix
    /// </summary>
        private string[] GetWindowsPortNames()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// Get the ports names if the system is on unix
        /// </summary>
        private string[] GetUnixPortNames()
        {
            int p = (int)System.Environment.OSVersion.Platform;
            List<string> serial_ports = new List<string>();

            if (p == 4 || p == 128 || p == 6)
            {
                string[] ttys = System.IO.Directory.GetFiles("/dev/", "tty.*");
                foreach (string dev in ttys)
                {
                    if ((!_manager.useCuPort && dev.StartsWith("/dev/tty.usb")) || (UduinoManager.Instance.useCuPort && dev.StartsWith("/dev/cu.usb")))
                    { // TODO : Test if (portName.StartsWith ("/dev/tty.usb") || portName.StartsWith ("/dev/ttyUSB"))
                        serial_ports.Add(dev);
                    }
                }
            }
            return serial_ports.ToArray();
        }

        /// <summary>
        /// Discover all active serial ports connected.
        /// When a new serial port is connected, send the IDENTITY request, to get the name of the arduino
        /// </summary>
        /// <param name="portNames">All Serial Ports names, dependings of the current OS</param>
        void Discover(string[] portNames)
        {
            if (portNames.Length == 0) Log.Error("Found 0 ports open. Are you sure your arduino is connected ?");
            List<string> tmpPortOpen = new List<string>();

            foreach (string portName in portNames)
            {
                if (!_manager.BlackListedPorts.Contains(portName))
                {
                    if (!tmpPortOpen.Contains(portName))
                    {
                        tmpPortOpen.Add(portName);
                        UduinoDevice tmpDevice = OpenUduinoDevice(portName);
                        tmpDevice.Open();
                        _manager.StartCoroutine(DetectUduino(tmpDevice));
                    }
                }
                else
                    Log.Info("Port <color=#2196F3>[" + portName + "]</color> is blacklisted.");
            }
        }
        
        
        public override UduinoDevice OpenUduinoDevice(string id)
        {
            return new UduinoDevice_DesktopSerial(id, _manager.BaudRate, _manager.readTimeout, _manager.writeTimeout, _manager.defaultArduinoBoardType);
        }

    }
}