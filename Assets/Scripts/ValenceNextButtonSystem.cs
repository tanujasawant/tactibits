using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValenceNextButtonSystem : MonoBehaviour {
	public Toggle p7,p6,p5,p4,p3,p2,p1;
	int value=0;
	public void ActiveToggle(){

		if (p1.isOn) {
			Debug.Log ("Participant selected p1");
			value = 1;
		} else if (p2.isOn) {
			Debug.Log ("Participant selected p2");
			value = 2;
		} else if (p3.isOn) {
			Debug.Log ("Participant selected p3");
			value = 3;
		} else if (p4.isOn) {
			Debug.Log ("Participant selected p4");
			value = 4;
		} else if (p5.isOn) {
			Debug.Log ("Participant selected p5");
			value = 5;
		} else if (p6.isOn) {
			Debug.Log ("Participant selected p6");
			value = 6;
		} else if (p7.isOn) {
			Debug.Log ("Participant selected p7");
			value = 7;
		}
	}

	public void setAllTogglesOff(){
		p1.isOn = false;
		p2.isOn = false;
		p3.isOn = false;
		p4.isOn = false;
		p5.isOn = false;
		p6.isOn = false;
		p7.isOn = false;
	}

	public void onNext(){
		Debug.Log ("Next clicked");
		ActiveToggle ();
		setAllTogglesOff ();//to have unselected toggles before next trial
		ExperienceController.Instance.ValencePanelClicked(value);
	}


}
