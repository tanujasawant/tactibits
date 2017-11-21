using UnityEngine;
using System.Collections;
using Uduino;

public class ButtonTrigger : MonoBehaviour
{
    public GameObject button;
    UduinoManager u;

    void Start()
    {
        // Solution 1
      //UduinoManager.Instance.OnValueReceived += OnValueReceived; //Create the Delegate
      //UduinoManager.Instance.AlwaysRead("uduinoButton");

        //Solution 2
        UduinoManager.Instance.AlwaysRead("uduinoButton", ButtonTriggerEvt);
    }

    void PressedDown()
    {
        button.GetComponent<Renderer>().material.color = Color.red;
        button.transform.Translate(Vector3.down / 10);
    }

    void PressedUp()
    {
        button.GetComponent<Renderer>().material.color = Color.green;
        button.transform.Translate(Vector3.up / 10);
    }

    void OnValueReceived(string data, string device)
    {
        if (data == "1")
            PressedDown();
        else if (data == "0")
            PressedUp();
    }
    
    void ButtonTriggerEvt(string data)
    {
        if (data == "1")
            PressedDown();
        else if (data == "0")
            PressedUp();
    }
}