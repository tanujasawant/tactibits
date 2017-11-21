using UnityEngine;
using System.Collections;
using Uduino; // adding Uduino NameSpace 

public class uMasterManySlavesLed : MonoBehaviour {
	UduinoManager u; // The instance of Uduino is initialized here

	// Use this for initialization
	void Start () {
		UduinoManager.Instance.OnValueReceived += OnValueReceived; // Create the Delegate
		Stroke();
	}

	void Stroke(){
		TriggerArduinoStuff(100 , 255);
	}
	// Update is called once per frame
	IEnumerator TriggerArduinoStuff(int motorIntensity, int temperatureIntensity) // 0 - 255
	{
		string message = motorIntensity + " " + temperatureIntensity;
		Uduino.UduinoManager.Instance.Write("TriggerModule", message);
		yield return new WaitForSeconds(0);
	}


	// Update is called once per frame
	void Update () {
		UduinoManager.Instance.Read("uduinoButton", "TriggerModule"); // Read every frame the value of the "myCommand" function on our board. 
	}

	void OnValueReceived(string data, string device)
	{
		Debug.Log(data); // Use the data as you want !
	}
}
