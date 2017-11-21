using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PatternManager))]
public class CustomInterface : Editor {
	public override void OnInspectorGUI(){
		//DrawDefaultInspector ();
		PatternManager myPatternManager = (PatternManager)target;
		//myPatternManager.motorIntensity= EditorGUILayout.IntField ("Vib Intensity", myPatternManager.motorIntensity);
		//myPatternManager.motorIntensity = EditorGUILayout.IntSlider (myPatternManager.motorIntensity, 0, 255);

		EditorGUILayout.BeginHorizontal ();
		GUILayout.Label("Vib Intensity");
		myPatternManager.motorIntensity = EditorGUILayout.Slider (myPatternManager.motorIntensity, 0, 255);
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		GUILayout.Label("Temp Intensity");
		myPatternManager.temperatureIntensity = EditorGUILayout.Slider (myPatternManager.temperatureIntensity, 0, 255);
		EditorGUILayout.EndHorizontal ();


		if (GUILayout.Button ("Stroke")) {
			myPatternManager.type = "stroke";
		} else if (GUILayout.Button ("Poke")) {
			myPatternManager.type = "poke";
		} else if (GUILayout.Button ("Hit")) {
			myPatternManager.type = "hit";
		}


		EditorGUILayout.BeginHorizontal ();
		GUILayout.Label("duration (s)");
		myPatternManager.duration = EditorGUILayout.Slider (myPatternManager.duration, 0.0f, 3.0f);
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		GUILayout.Label("start offset (s)");
		myPatternManager.startOffset = EditorGUILayout.Slider (myPatternManager.startOffset, 0.0f, 3.0f);
		EditorGUILayout.EndHorizontal ();


	}

}
