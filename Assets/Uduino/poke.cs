using System.Collections;
using UnityEngine;
using Uduino; // adding Uduino NameSpace 

public class poke : MonoBehaviour {

	UduinoManager u; // The instance of Uduino is initialized here
	void Start () {
		Poke(100);
	}

	void Poke(int motorStrength)
	{
		StartCoroutine(TriggerPeltierStuff(38, 0 , 255,0f, 0.0f));
		StartCoroutine(TriggerArduinoStuff(38, motorStrength , 255,1f, 0.2f));
	}
	// Update is called once per frame
	IEnumerator TriggerArduinoStuff(int moduleId, int motorIntensity, int temperatureIntensity, float duration, float startOffset) // 0 - 255
	{
		yield return new WaitForSeconds(startOffset);
		string message = moduleId + " " + motorIntensity + " " + temperatureIntensity;
		Uduino.UduinoManager.Instance.Write(message :  "TriggerModule " + message);
		yield return new WaitForSeconds(duration);
		message = moduleId+" 0 0";
		Uduino.UduinoManager.Instance.Write(message :  "TriggerModule " + message);
	}

	IEnumerator TriggerPeltierStuff(int moduleId, int motorIntensity, int temperatureIntensity, float duration, float startOffset) // 0 - 255
	{
		yield return new WaitForSeconds(startOffset);
		string message = moduleId + " " + motorIntensity + " " + temperatureIntensity;
		Uduino.UduinoManager.Instance.Write(message :  "TriggerModule " + message);
	}

}
