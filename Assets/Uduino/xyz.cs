using UnityEngine;
using System.Collections;
using Uduino; // adding Uduino NameSpace 

public class xyz : MonoBehaviour {
	UduinoManager u; // The instance of Uduino is initialized here

	void Start()
	{
		UduinoManager.Instance.OnValueReceived += OnValueReceived; // Create the Delegate
	}

	void Update()
	{
		UduinoManager.Instance.Read("myMasterArduinoName", "myCommand"); // Read every frame the value of the "myCommand" function on our board. 
	}

	void OnValueReceived(string data, string device)
	{
		Debug.Log(data); // Use the data as you want !
	}
}
