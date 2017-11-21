using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;

public class ChangeBoardType : MonoBehaviour {

    int customPinAnalog = 0;
    int customPinDigital = 0;

    void Start ()
	{
        //Set the board, to display  display in the editor
        UduinoManager.Instance.SetBoardType("Arduino Mega"); //If you have one board connected
        UduinoManager.Instance.SetBoardType("uduinoBoard", "Arduino Mega"); // If you have several Boards connected

        //Get the pin for a custom board
        customPinAnalog = UduinoManager.Instance.GetPinFromBoard("Arduino Mega", "A14");

        // If the board is already set with SetBoardType, you can get the Pin iD by usong
        customPinAnalog = UduinoManager.Instance.GetPinFromBoard("A14");

        UduinoManager.Instance.InitPin(customPinAnalog, PinMode.Analog);
        Debug.Log("The pin A14 pinout for Arduino Mega is " + customPinAnalog);

        //Get the pin for a custom board
        customPinDigital = BoardsTypeList.Boards.GetBoardFromName("Arduino Mega").GetPin("42"); // returns 42
        UduinoManager.Instance.InitPin(customPinDigital, PinMode.Output);
    }

}
