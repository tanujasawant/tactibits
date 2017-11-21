using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextButtonSystem : MonoBehaviour {
	public Toggle p7,p6,p5,p4,p3,p2,p1;
	public Toggle e1,e2,e3,e4,e5,e6,e7;

	public void ActiveToggle(){

		if (p1.isOn) {
			Debug.Log ("Participant selected p1");
		} else if (p2.isOn) {
			Debug.Log ("Participant selected p2");
		} else if (p3.isOn) {
			Debug.Log ("Participant selected p3");
		} else if (p4.isOn) {
			Debug.Log ("Participant selected p4");
		} else if (p5.isOn) {
			Debug.Log ("Participant selected p5");
		} else if (p6.isOn) {
			Debug.Log ("Participant selected p6");
		} else if (p7.isOn) {
			Debug.Log ("Participant selected p7");
		}

		if (e1.isOn) {
			Debug.Log ("Participant selected e1");
		} else if (e2.isOn) {
			Debug.Log ("Participant selected e2");
		} else if (e3.isOn) {
			Debug.Log ("Participant selected e3");
		} else if (e4.isOn) {
			Debug.Log ("Participant selected e4");
		} else if (e5.isOn) {
			Debug.Log ("Participant selected e5");
		} else if (e6.isOn) {
			Debug.Log ("Participant selected e6");
		} else if (e7.isOn) {
			Debug.Log ("Participant selected e7");
		}

	}

	public void onNext(){
		Debug.Log ("Next clicked");
		ActiveToggle ();
	}


}
