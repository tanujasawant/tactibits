using UnityEngine;
using System;
using System.Collections;
using System.IO.Ports;

namespace Uduino {

    public class UduinoDevice_DesktopSerial : UduinoDevice
    {
        //Serial status
        public SerialPort serial = null;

        private string _port;

        private int _writeTimeout = 50;
        private int _readTimeout = 50;
        public override int readTimeout
        {
            get
            {
                if (serial != null) return serial.ReadTimeout;
                else return _readTimeout;
            }
            set
            {
                if (serial != null)
                {
                    try
                    {
                        serial.ReadTimeout = value;
                    }
                    catch (Exception e)
                    {
                        Log.Error("Impossible to set ReadTimeout on <color=#2196F3>[" + _port + "]</color> : " + e);
                        Close();
                    }
                }
                else
                {
                    _readTimeout = value;
                }
            }
        }

        //Timeout
        public override int writeTimeout
        {
            get
            {
                if (serial != null) return serial.WriteTimeout;
                else return _writeTimeout;
            }
            set
            {
                if (serial != null)
                {

                    try
                    {
                        serial.WriteTimeout = value;
                    }
                    catch (Exception e)
                    {
                        Log.Error("Error on port <color=#2196F3>[" + _port + "]</color> : " + e);
                        Close();
                    }
                } else
                {
                    _writeTimeout = value;
                }
            }
        }


        //TODO : faire les fonctions set Rea
        public UduinoDevice_DesktopSerial(int baudrate = 9600) : base() { }

        public UduinoDevice_DesktopSerial(string port, int baudrate = 9600, int readTimeout = 100, int writeTimeout = 100, int boardType = 0) : base()
        {
            this._baudrate = baudrate;
            this.readTimeout = readTimeout;
            this.writeTimeout = writeTimeout;
            _boardType = boardType;
            this.identity = port;
            _port = port;
        }


        /// <summary>
        /// Open a specific serial port
        /// </summary>
        public override void Open()
        {
            try
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                _port = "\\\\.\\" + _port;
#endif
                serial = new SerialPort(_port, _baudrate, Parity.None, 8, StopBits.One);
                serial.ReadTimeout = _readTimeout;
                serial.WriteTimeout = _writeTimeout;
                serial.Close();
                serial.Open();
                boardStatus = BoardStatus.Open;
                Log.Info("Opening stream on port <color=#2196F3>[" + _port + "]</color>");
            }
            catch (Exception e)
            {
                Log.Error("Error on port <color=#2196F3>[" + _port + "]</color> : " + e);
                Close();
            }
        }

        #region Public functions
        /// <summary>
        /// Return serial port 
        /// </summary>
        /// <returns>Current opened com port</returns>
        public string getPort()
        {
            return _port;
        }

        #endregion

        #region Commands
        /// <summary>
        /// Loop every thead request to write a message on the arduino (if any)
        /// </summary>
        public override bool WriteToArduinoLoop()
        {
            if (serial == null || !serial.IsOpen)
                return false;

            lock (writeQueue)
            {
                if (writeQueue.Count == 0)
                    return false;

                string message = (string)writeQueue.Dequeue();
                try
                {
                    try
                    {
                        serial.WriteLine(message);
                        serial.BaseStream.Flush();
                        Log.Info("<color=#4CAF50>" + message + "</color> sent to <color=#2196F3>[" + _port + "]</color>");
                    }
                    catch (Exception e)
                    {
                        writeQueue.Enqueue(message);
                        Log.Warning("Impossible to send the message " + message + " to <color=#2196F3>[" + _port + "]</color>," + e);
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Error on port <color=#2196F3>[" + _port + "]</color> : " + e);
                    // Close();
                    return false;
                }
                WritingSuccess(message);
            }
            return true;
        }

        /// <summary>
        /// Read Arduino serial port
        /// </summary>
        /// <param name="message">Write a message to the serial port before reading the serial</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <param name="instant">Read the message value now and not in the thread loop</param>
        /// <returns>Read data</returns>
        public override string ReadFromArduino(string message = null, int timeout = 0, bool instant = false)
        {
            if (serial == null || !serial.IsOpen || boardStatus == BoardStatus.Stopping)
                return null;

            if (timeout > 0 && timeout != serial.ReadTimeout)             // TODO : supprimer toute référence a un read timeout !!! (ça sert a rien et ça fait de la merde) 
                readTimeout = timeout;

            return base.ReadFromArduino(message, timeout, instant);
        }

        public override void ReadFromArduinoLoop()
        {
            if (serial == null || !serial.IsOpen || boardStatus == BoardStatus.Stopping)
                return;

            base.ReadFromArduinoLoop();

            serial.DiscardOutBuffer();
            serial.DiscardInBuffer();

            try
            {
                try
                {
                    string readedLine = serial.ReadLine();
                    MessageReceived(readedLine);
                }
                catch (TimeoutException e)
                {
                    if (boardStatus == BoardStatus.Found && !autoRead)
                        Log.Debug("ReadTimeout. Are you sure someting is written in the serial of the board ? \n" + e);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                Close();
            }
        }
        #endregion

        #region Close
        /// <summary>
        /// Close Serial port 
        /// </summary>
        public override void Close()
        {
            ClearQueues();

            if (serial != null && serial.IsOpen)
            {
                Log.Warning("Closing port : <color=#2196F3>[" + _port + "]</color>");
                serial.Close();
                boardStatus = BoardStatus.Closed;
                serial = null;
            }
            else
            {
                Log.Info(_port + " already closed.");
            }
        }
        #endregion
    }
}