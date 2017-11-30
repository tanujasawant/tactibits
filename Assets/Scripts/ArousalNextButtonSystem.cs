using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArousalNextButtonSystem : MonoBehaviour {
	public Toggle e1,e2,e3,e4,e5,e6,e7;
	int value=0;
	public void ActiveToggle(){

		if (e1.isOn) {
			Debug.Log ("Participant selected e1");
			value = 1;
		} else if (e2.isOn) {
			Debug.Log ("Participant selected e2");
			value = 2;
		} else if (e3.isOn) {
			Debug.Log ("Participant selected e3");
			value = 3;
		} else if (e4.isOn) {
			Debug.Log ("Participant selected e4");
			value = 4;
		} else if (e5.isOn) {
			Debug.Log ("Participant selected e5");
			value = 5;
		} else if (e6.isOn) {
			Debug.Log ("Participant selected e6");	
			value = 6;
		} else if (e7.isOn) {
			Debug.Log ("Participant selected e7");
			value = 7;
		}
	}

	public void setAllTogglesOff(){
		e1.isOn = false;
		e2.isOn = false;
		e3.isOn = false;
		e4.isOn = false;
		e5.isOn = false;
		e6.isOn = false;
		e7.isOn = false;
	}

	public void onNext(){
		Debug.Log ("Next clicked");
		ActiveToggle ();	
		setAllTogglesOff ();
		ExperienceController.Instance.ArousalPanelClicked(value);
	}


}
