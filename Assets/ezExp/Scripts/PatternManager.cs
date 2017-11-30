using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternManager : MonoBehaviour {

	public float motorIntensity;
	public float temperatureIntensity;

	public float duration=1f;
	public float startOffset=1.2f;//time duration for which first peltier warms up
	public string type;
	//public bool s,p,h;
	// Use this for initialization
	void Start () {
		TriggerPatterns (type, motorIntensity, temperatureIntensity, duration, startOffset);
	}

	public void TriggerPatterns (string type, float motorIntensity, float temperatureIntensity, float duration, float startOffset){
		switch (type) {
		case "stroke": 
			Stroke (motorIntensity, temperatureIntensity, duration, startOffset);
			break;
		case "poke":
			Poke (motorIntensity, temperatureIntensity, duration, startOffset);
			break;
		case "hit":
			Poke (motorIntensity, temperatureIntensity, duration, startOffset);
			break;
		default:
			break;
		}
	}

	public void Stroke(float motorStrength, float temperatureIntensity, float duration, float startOffset)
	{
		StartCoroutine(TriggerPeltierStuff(38, 0 , temperatureIntensity, 0f, 0.0f));
		StartCoroutine(TriggerPeltierStuff(39, 0 , temperatureIntensity, 0f, duration));
		StartCoroutine(TriggerPeltierStuff(40, 0 , temperatureIntensity, 0f, 2*duration));

		StartCoroutine(TriggerArduinoStuff(38, motorStrength , temperatureIntensity, duration, startOffset));
		startOffset += duration;
		StartCoroutine(TriggerArduinoStuff(39, motorStrength , temperatureIntensity, duration, startOffset));
		startOffset += duration;
		StartCoroutine(TriggerArduinoStuff(40, motorStrength , temperatureIntensity, duration, startOffset));
	}

	public void Poke(float motorStrength,float temperatureIntensity, float duration, float startOffset)
	{
		StartCoroutine(TriggerPeltierStuff(38, 0 , temperatureIntensity,0f, 0.0f));
		StartCoroutine(TriggerArduinoStuff(38, motorStrength , temperatureIntensity,duration, startOffset));
	}
		
	public IEnumerator TriggerArduinoStuff(int moduleId, float motorIntensity, float temperatureIntensity, float duration, float startOffset) // 0 - 255
	{
		yield return new WaitForSeconds(startOffset);
		string message = moduleId + " " + motorIntensity + " " + temperatureIntensity;
		Uduino.UduinoManager.Instance.Write(message :  "TriggerModule " + message);
		yield return new WaitForSeconds(duration);
		message = moduleId+" 0 0";
		Uduino.UduinoManager.Instance.Write(message :  "TriggerModule " + message);
	}

	public IEnumerator TriggerPeltierStuff(int moduleId, float motorIntensity, float temperatureIntensity, float duration, float startOffset) // 0 - 255
	{
		yield return new WaitForSeconds(startOffset);
		string message = moduleId + " " + motorIntensity + " " + temperatureIntensity;
		Uduino.UduinoManager.Instance.Write(message :  "TriggerModule " + message);
	}

}
