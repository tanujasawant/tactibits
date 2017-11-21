using System.Collections;
using UnityEngine;
using Uduino; // adding Uduino NameSpace 

public class stroke : MonoBehaviour {

	UduinoManager u; // The instance of Uduino is initialized here
	void Start () {
		Stroke(100);
	}

	void Stroke(int motorStrength)
	{
		StartCoroutine(TriggerPeltierStuff(38, 0 , 255,0f, 0.0f));
		StartCoroutine(TriggerPeltierStuff(39, 0 , 255, 0f, 0.5f));
		StartCoroutine(TriggerPeltierStuff(40, 0 , 255, 0f, 1.0f));

		StartCoroutine(TriggerArduinoStuff(38, motorStrength , 255,1f, 1.2f));
		StartCoroutine(TriggerArduinoStuff(39, motorStrength , 255, 1f, 2.2f));
		StartCoroutine(TriggerArduinoStuff(40, motorStrength , 255, 1f, 3.2f));
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
