﻿using UnityEngine;
using System;
using System.Net;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Uduino;

#if UDUINO_READY
#region Editor Pin
[SerializeField]
public class EditorPin : Pin
{
    UduinoManagerEditor editorManager = null;

    public static string[] arduinoUnoPins = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "A0", "A1", "A2", "A3", "A4", "A5" };

    private int prevPin = -1;

    public EditorPin(string arduinoParent, int pin, PinMode mode, UduinoManagerEditor m)
            : base(arduinoParent, pin, mode)
    {
        editorManager = m;
        arduinoName = arduinoParent;
        currentPin = pin;
        ChangePinMode(mode);
    }

    public override void SendRead(string bundle = null, System.Action<string> action = null, string digital = "")
    {
        if (editorManager != null)
        {
            editorManager.Read(arduinoName, "r" + digital + " " + currentPin, action: editorManager.ParseReadData);
        }
    }

    public override void CheckChanges()
    {
        if (Application.isPlaying)
        {
            foreach (Pin pinTarget in Manager.pins)
            {
                if (pinTarget.PinTargetExists(arduinoName, currentPin))
                {
                    if (pinMode != prevPinMode)
                        pinTarget.ChangePinMode(pinMode);
                }
            }
        }

        if (currentPin != prevPin && currentPin != -1)
        {
            WriteMessage("s " + currentPin + " " + (int)pinMode);
            prevPin = currentPin;
        }

        if (pinMode != prevPinMode)
        {
            WriteMessage("s " + currentPin + " " + (int)pinMode);
            prevPinMode = pinMode;
        }
    }

    public override void WriteMessage(string message, string bundle = null)
    {
        if (editorManager != null)
        {
            editorManager.WriteMessage(arduinoName, message);
        }
    }
}

#endregion

[CustomEditor(typeof(UduinoManager))]
public class UduinoManagerEditor : Editor
{
    public static UduinoManagerEditor Instance { get; private set; }

    #region Variables
    private UduinoManager manager = null;
    public UduinoManager Manager
    {
        get
        {
            if (Application.isPlaying) return UduinoManager.Instance;
            else return manager;
        }
        set
        {
            manager = value;
        }
    }
    string message = "";
    string messageValue = "";
    string newBlackListedPort = "";
    string checkVersion = "";

    LogLevel debugLevel;

    bool defaultPanel = true;
    bool arduinoPanel = true;
    bool advancedPanel = false;
    bool blacklistedFoldout = false;

    //Style-relatedx
    Color headerColor = new Color(0.65f, 0.65f, 0.65f, 1);
    //Color backgroundColor = new Color(0.75f, 0.75f, 0.75f);
    Color defaultButtonColor;

    GUIStyle boldtext = null;
    GUIStyle olLight = null;
    GUIStyle olInput = null;
    GUIStyle customFoldtout = null;

    bool isUpToDate = false;
    bool isUpToDateChecked = false;

    // Settings
    public string[] baudRates = new string[] { "4800", "9600", "19200", "38400", "57600", "115200" };
    int prevBaudRateIndex = 1;
    public int baudRateIndex = 1;

    #endregion

    void OnEnable()
    {
        if (manager == null)
            manager = (UduinoManager)target;

        FindExistingExtensions();
        Instance = this;
    }

    #region Styles
    void SetColorAndStyles()
    {
        if (boldtext == null)
        {
            //Color and GUI
            defaultButtonColor = GUI.backgroundColor;
            if (!EditorGUIUtility.isProSkin)
            {
                headerColor = new Color(165 / 255f, 165 / 255f, 165 / 255f, 1);
                //  backgroundColor = new Color(193 / 255f, 193 / 255f, 193 / 255f, 1);
            }
            else
            {
                headerColor = new Color(41 / 255f, 41 / 255f, 41 / 255f, 1);
                //    backgroundColor = new Color(56 / 255f, 56 / 255f, 56 / 255f, 1);
            }

            boldtext = new GUIStyle(GUI.skin.label);
            boldtext.fontStyle = FontStyle.Bold;
            boldtext.alignment = TextAnchor.UpperCenter;

            olLight = new GUIStyle("OL Titleleft");
            olLight.fontStyle = FontStyle.Normal;
            olLight.font = GUI.skin.button.font;
            olLight.fontSize = 9;
            olLight.alignment = TextAnchor.MiddleCenter;

            olInput = new GUIStyle("TE toolbar");
            olInput.fontStyle = FontStyle.Bold;
            olInput.fontSize = 10;
            olInput.alignment = TextAnchor.MiddleLeft;

            customFoldtout = new GUIStyle(EditorStyles.foldout);
            customFoldtout.fontStyle = FontStyle.Bold;

        }
    }

    void SetGUIBackgroundColor(string hex)
    {
        Color color = new Color();
        ColorUtility.TryParseHtmlString(hex, out color);
        GUI.backgroundColor = color;
    }
    void SetGUIBackgroundColor(Color color)
    {
        GUI.backgroundColor = color;
    }
    void SetGUIBackgroundColor()
    {
        GUI.backgroundColor = defaultButtonColor;
    }

    public void DrawLogo()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace(); // Nededed for lastRect
        EditorGUILayout.EndHorizontal();

        Texture tex = (Texture)EditorGUIUtility.Load("Assets/Uduino/Editor/Resources/uduino-logo.png");
        Texture tex2 = (Texture)EditorGUIUtility.Load("Assets/Uduino/Editor/Resources/arduino-bg.png");
        GUILayout.Space(0);
        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUI.Box(new Rect(1, lastRect.y + 4, Screen.width, 27), tex);
        lastRect = GUILayoutUtility.GetLastRect();

        Color bgColor = new Color();
        ColorUtility.TryParseHtmlString("#ffffff", out bgColor);

        EditorGUI.DrawRect(new Rect(lastRect.x - 15, lastRect.y + 5f, Screen.width + 1, 60f), bgColor);
        GUI.DrawTexture(new Rect(Screen.width / 2 - tex.width / 2 - 20, lastRect.y + 5, tex2.width, tex2.height), tex2, ScaleMode.ScaleToFit);

        GUI.DrawTexture(new Rect(Screen.width / 2 - tex.width / 2, lastRect.y + 10, tex.width, tex.height), tex, ScaleMode.ScaleToFit);

        GUI.color = Color.white;
        GUILayout.Space(60f);
    }

    public void DrawLine(int marginTop, int marginBottom, int height)
    {
        EditorGUILayout.Separator();
        GUILayout.Space(marginTop);
        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUI.Box(new Rect(0f, lastRect.y + 4, Screen.width, height), "");
        GUILayout.Space(marginBottom);
    }

    public bool DrawHeaderTitle(string title, bool foldoutProperty, Color backgroundColor)
    {
        GUILayout.Space(0);
        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUI.Box(new Rect(1, lastRect.y + 4, Screen.width, 27), "");
        lastRect = GUILayoutUtility.GetLastRect();
        EditorGUI.DrawRect(new Rect(lastRect.x - 15, lastRect.y + 5f, Screen.width + 1, 25f), headerColor);
        GUI.Label(new Rect(lastRect.x, lastRect.y + 10, Screen.width, 25), title);
        GUI.color = Color.clear;
        if (GUI.Button(new Rect(0, lastRect.y + 4, Screen.width, 27), ""))
        {
            foldoutProperty = !foldoutProperty;
        }
        GUI.color = Color.white;
        GUILayout.Space(30);
        if (foldoutProperty) { GUILayout.Space(5); }
        return foldoutProperty;
    }
    #endregion

    #region CheckCompatibility and CheckUpdate
    public void CheckCompatibility()
    {
#if UNITY_5_6
        if (PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone) == ApiCompatibilityLevel.NET_2_0_Subset)
#else
        if (PlayerSettings.apiCompatibilityLevel == ApiCompatibilityLevel.NET_2_0_Subset)
#endif
        {
            SetGUIBackgroundColor("#ef5350");
            EditorGUILayout.HelpBox("Uduino works only with .NET 2.0 (not Subset).", MessageType.Error, true);
            if (GUILayout.Button("Fix Now", GUILayout.ExpandWidth(true)))
            {
#if UNITY_5_6
                PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_2_0);
#else
                PlayerSettings.apiCompatibilityLevel = ApiCompatibilityLevel.NET_2_0;
#endif
                PlayerSettings.runInBackground = true;
                Debug.LogWarning("Reimporting all assets.");
                AssetDatabase.ImportAsset("Assets/Uduino/Scripts", ImportAssetOptions.ImportRecursive);
                AssetDatabase.Refresh();
            }
            SetGUIBackgroundColor();
        }
    }

    string destinationFolder = "";

    public void CheckUpdate()
    {
        if (AssetDatabase.IsValidFolder("Assets/Uduino/Arduino"))
        {
            EditorGUILayout.HelpBox("Uduino has been updated! To continue, select the Arduino libraries folder to add Uduino library.", MessageType.None, true);

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            EditorGUILayout.HelpBox("The Arduino libraries folder is located under: ~/Documents/Arduino/libraries ", MessageType.Info, true);
#else
            EditorGUILayout.HelpBox("The Arduino libraries folder is located under: C:/Users/<username>/Documents/Arduino/libraries", MessageType.Info, true);
#endif

            EditorGUILayout.LabelField("Select Arduino libraries Folder");

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            destinationFolder = EditorGUILayout.TextField(destinationFolder);
            GUILayout.EndVertical();
            GUILayout.BeginVertical();

            if (GUILayout.Button("Select path", GUILayout.ExpandWidth(true)))
            {
                GUI.FocusControl("");
                string path = EditorUtility.OpenFolderPanel("Set Arduino path", "", "");
                if (path.Length != 0)
                {
                    destinationFolder = path;
                }
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();


            DrawLine(12, 0, 45);
            SetGUIBackgroundColor("#4FC3F7");

            if (GUILayout.Button("Update Uduino's Arduino library", GUILayout.ExpandWidth(true)))
            {
                if (destinationFolder == "NOT SET")
                {
                    if (EditorUtility.DisplayDialog("Move library folder", "You have to set a valid folder path", "Ok", "Cancel"))
                    { }
                }
                else if (EditorUtility.DisplayDialog("Move library folder", "Are you sure the Arduino libraries folder is " + destinationFolder + " ?", "Move", "Cancel"))
                {
                    MoveArduinoFiles(destinationFolder);
                }
            }
            SetGUIBackgroundColor();
        }
        EditorGUILayout.Separator();
    }

    
    void MoveArduinoFiles(string dest)
    {
        string sourceDirectory = Application.dataPath + "/Uduino/Arduino/libraries";
        string destinationDirectory = FirstToLower(dest);
        MoveDirectory(sourceDirectory, destinationDirectory);
    }

    public static void MoveDirectory(string source, string target)
    {
        var stack = new Stack<Folders>();
        stack.Push(new Folders(source, target));

        while (stack.Count > 0)
        {
            var folders = stack.Pop();
            Directory.CreateDirectory(folders.Target);
            foreach (var file in Directory.GetFiles(folders.Source, "*.*"))
            {
                string targetFile = Path.Combine(folders.Target, Path.GetFileName(file));
                if (Path.GetExtension(file) != ".meta")
                {
                    if (File.Exists(targetFile))
                        File.Delete(targetFile);
                    File.Move(file, targetFile);
                }
            }

            foreach (var folder in Directory.GetDirectories(folders.Source))
            {
                stack.Push(new Folders(folder, Path.Combine(folders.Target, Path.GetFileName(folder))));
            }
        }
        Directory.Delete(source, true);
        FileUtil.DeleteFileOrDirectory("Assets/Uduino/Arduino");
    }

    public class Folders
    {
        public string Source { get; private set; }
        public string Target { get; private set; }

        public Folders(string source, string target)
        {
            Source = source;
            Target = target;
        }
    }
    #endregion

    public string FirstToLower(string s)
    {
        if (string.IsNullOrEmpty(s))
            return string.Empty;

        char[] a = s.ToCharArray();
        a[0] = char.ToLower(a[0]);
        return new string(a);
    }


#region Detect Plugins


    public static System.Type[] GetAllSubTypes(System.Type aBaseClass)
    {
        var result = new System.Collections.Generic.List<System.Type>();
        System.Reflection.Assembly[] AS = System.AppDomain.CurrentDomain.GetAssemblies();
        foreach (var A in AS)
        {
            System.Type[] types = A.GetTypes();
            foreach (var T in types)
            {
                if (T.IsSubclassOf(aBaseClass))
                    result.Add(T);
            }
        }
        return result.ToArray();
    }


    List<string> GetExtensionsSubTypes()
    {
        List<string> extensionsNames = new List<string>();
        foreach (var T in GetAllSubTypes(typeof(UduinoDevice)))
            extensionsNames.Add(T.Name);
        return extensionsNames;
    }

    public void FindExistingExtensions()
    {
        List<string> subTypes = GetExtensionsSubTypes();
        Dictionary<string, UduinoExtension> availableExtensions = new Dictionary<string, UduinoExtension>(Manager.AvailableExtensions);

        foreach (KeyValuePair<string, UduinoExtension> u in availableExtensions)
        {
            if (subTypes.Contains(u.Key))
            {
                UduinoExtension a = u.Value;
                if (!a.isPresent)
                {
                    a.isPresent = true;
                    Manager.AvailableExtensions.Remove(u.Key);
                    Manager.AvailableExtensions.Add(u.Key, a);
                }
            }
        }
    }
#endregion

    public override void OnInspectorGUI()
    {
        if (manager == null)
            manager = (UduinoManager)target;

        baudRateIndex = System.Array.IndexOf(baudRates, Manager.BaudRate.ToString());

        Log.SetLogLevel(manager.debugLevel);
        SetColorAndStyles();

        DrawLogo();

        defaultPanel = DrawHeaderTitle("Uduino Settings", defaultPanel, headerColor);

        if (defaultPanel)
        {

            CheckCompatibility();

            GUILayout.Label("Connection type", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            Dictionary<string, UduinoExtension> availableExtensions = new Dictionary<string, UduinoExtension>(Manager.AvailableExtensions);
            foreach (KeyValuePair<string, UduinoExtension> u in availableExtensions)
            {
                if (u.Value.isPresent)
                {
                    GUILayout.BeginHorizontal();
                    bool isActiveValue = EditorGUILayout.Toggle(u.Value.name, u.Value.isActive);
                    if(isActiveValue != u.Value.isActive)
                    {
                        UduinoExtension changedExtension = u.Value;
                        changedExtension.isActive = isActiveValue;
                        Manager.AvailableExtensions.Remove(u.Key);
                        Manager.AvailableExtensions.Add(u.Key, changedExtension);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            EditorGUI.indentLevel--;


            GUILayout.Label("General", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            LogLevel tmpLogLevel = (LogLevel)EditorGUILayout.EnumPopup("Log Level", Manager.debugLevel);
            if (tmpLogLevel != Manager.debugLevel)
            {
                Manager.debugLevel = tmpLogLevel;
                EditorUtility.SetDirty(target);
            }

            EditorGUI.indentLevel--;
            GUILayout.Label("Arduino", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            baudRateIndex = EditorGUILayout.Popup("Baud Rate", baudRateIndex, baudRates);
            if (prevBaudRateIndex != baudRateIndex)
            {
                int result = 9600;
                int.TryParse(baudRates[baudRateIndex], out result);
                manager.BaudRate = result;
                prevBaudRateIndex = baudRateIndex;
            }

            Manager.ReadOnThread = EditorGUILayout.Toggle("Read on threads", Manager.ReadOnThread);
            EditorGUI.indentLevel--;

            GUILayout.Label("Messages", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            if (manager.LimitSendRate = EditorGUILayout.Toggle("Limit Send Rate", Manager.LimitSendRate))
                if (manager.LimitSendRate)
                {
                    Manager.SendRateSpeed = EditorGUILayout.IntField("Send Rate speed", Manager.SendRateSpeed);
                    EditorGUILayout.Separator();
                }

            EditorGUI.indentLevel--;

            EditorGUILayout.Separator();
            CheckUpdate();
        }

        arduinoPanel = DrawHeaderTitle("Adruino", arduinoPanel, headerColor);
        if (arduinoPanel)
        {
            ArduinoSettings();
        }

        advancedPanel = DrawHeaderTitle("Advanced", advancedPanel, headerColor);
        if (advancedPanel)
        {
            AdvancedSettings();
        }


        //TODO : We add that here beacause we the values serialized are not updated
        if (!Application.isPlaying)
            EditorUtility.SetDirty(target);
    }

    public void ArduinoSettings()
    {
        if (Manager.uduinoDevices.Count == 0)
        {
            SetGUIBackgroundColor("#ef5350");
            GUILayout.BeginVertical("Box", GUILayout.ExpandWidth(true));
            GUILayout.Label("No Arduino connected", boldtext);
            GUILayout.EndVertical();
            SetGUIBackgroundColor();
        }
        else
        {
            foreach (KeyValuePair<string, UduinoDevice> uduino in Manager.uduinoDevices)
            {
                SetGUIBackgroundColor("#4FC3F7");
                GUILayout.BeginVertical("Box", GUILayout.ExpandWidth(true));
                GUILayout.Label(uduino.Key, boldtext);
                GUILayout.EndVertical();
                SetGUIBackgroundColor();

                GUILayout.Label("Board informations", EditorStyles.boldLabel);

                GUILayout.BeginVertical("Box");
                EditorGUILayout.TextField("Last read message", uduino.Value.lastRead);
                EditorGUILayout.TextField("Last sent value", uduino.Value.lastWrite);
                GUILayout.EndVertical();

                #region Pin Active
                if (uduino.Key == "uduinoBoard" && Application.isPlaying)
                {
                    GUILayout.Label("Scripted pins", EditorStyles.boldLabel);
                    GUILayout.BeginVertical("Box");

                    if (Manager.pins.Count != 0) // If a pin is active
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Pin", "OL Titleleft", GUILayout.MaxWidth(40f));
                        GUILayout.Label("Mode", "OL Titlemid", GUILayout.MaxWidth(55f));
                        GUILayout.Label("Status", "OL Titlemid", GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                        foreach (Pin pin in Manager.pins)
                            DrawPin(pin, arduinoBoard: uduino.Value._boardType);

                    }
                    else // if no pins are active
                    {
                        GUILayout.Label("No arduino pins are currently setup by code.");
                    }

                    GUILayout.EndVertical();
                }
                #endregion

                #region Send Command

                GUILayout.Label("Send commands", EditorStyles.boldLabel);
                GUILayout.BeginVertical("Box");
                if (uduino.Key == "uduinoBoard") // Display the informations for default Uduino Board
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Pin", "OL Titleleft", GUILayout.MaxWidth(40f));
                    GUILayout.Label("Mode", "OL Titlemid", GUILayout.MaxWidth(55f));
                    GUILayout.Label("Action", "OL Titlemid", GUILayout.ExpandWidth(true));
                    GUILayout.Label("×", "OL Titleright", GUILayout.Width(22f));
                    GUILayout.EndHorizontal();

                    foreach (Pin pin in Manager.pins.ToArray())
                        DrawPin(pin, true, uduino.Value._boardType);

                    if (GUILayout.Button("Add a pin", "TE toolbarbutton", GUILayout.ExpandWidth(true)))
                        Manager.pins.Add(new EditorPin(uduino.Key, 13, PinMode.Output, this));
                }
                else // If it's a "Normal" Arduino
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Command", "OL Titleleft");
                    //   GUILayout.Label("Value (optional)", "OL Titlemid");
                    GUILayout.EndHorizontal();
                    //  Rect scale = GUILayoutUtility.GetLastRect();

                    GUILayout.BeginHorizontal();
                    message = EditorGUILayout.TextField("", message, GUILayout.ExpandWidth(true));
                    // messageValue = EditorGUILayout.TextField("", messageValue, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("Send command", "TE toolbarbutton", GUILayout.ExpandWidth(true)))
                    {
                        if (messageValue != "") Manager.Write(uduino.Key, message + " " + messageValue);
                        else Manager.Write(uduino.Key, message);
                        Manager.Read(uduino.Key);
                        Manager.ReadWriteArduino(uduino.Key);
                    }
                }
                GUILayout.EndVertical();
                #endregion

                #region Board settings
                //More setings
                //  EditorGUILayout.Separator();
                bool foldout = EditorPrefs.GetBool(uduino.Key);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.GetControlRect(true, 16f, EditorStyles.foldout);
                Rect foldRect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.MouseUp && foldRect.Contains(Event.current.mousePosition))
                {
                    foldout = !foldout;
                    EditorPrefs.SetBool(uduino.Key, foldout);
                    GUI.changed = true;
                    Event.current.Use();
                }

                foldout = EditorGUI.Foldout(foldRect, foldout, "Other settings", customFoldtout);
                if (foldout)
                {
                    EditorGUI.indentLevel++;
                    uduino.Value._boardType = EditorGUILayout.Popup("Board Type", uduino.Value._boardType, BoardsTypeList.Boards.ListToNames());
                    uduino.Value.readTimeout = EditorGUILayout.IntField("Read timeout", uduino.Value.readTimeout);
                    uduino.Value.writeTimeout = EditorGUILayout.IntField("Write timeout", uduino.Value.writeTimeout);
                    uduino.Value.continuousRead = EditorGUILayout.Toggle("Auto read", uduino.Value.continuousRead);
                    EditorGUI.indentLevel--;

                }
                GUILayout.Space(5f);

                if (uduino.Value.autoRead)
                    EditorUtility.SetDirty(target);
                #endregion
            }
        }

        #region Discover/Close
        DrawLine(12, 0, 45);

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        SetGUIBackgroundColor("#4FC3F7");
        if (GUILayout.Button("Discover ports"))
        {
            Manager.DiscoverPorts();
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        SetGUIBackgroundColor("#ef5350");
        if (GUILayout.Button("Close ports"))
        {
            Manager.FullReset();
            Manager.StopAllCoroutines();
            Manager.pins.Clear();
        }
        SetGUIBackgroundColor();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        EditorGUILayout.Separator();
        #endregion
    }

    public void AdvancedSettings()
    {

        GUILayout.Label("General Serial settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        Manager.defaultArduinoBoardType = EditorGUILayout.Popup("Board Type", Manager.defaultArduinoBoardType, BoardsTypeList.Boards.ListToNames());
        Manager.readTimeout = EditorGUILayout.IntField("Read timeout", Manager.readTimeout);
        Manager.writeTimeout = EditorGUILayout.IntField("Write timeout", Manager.writeTimeout);
        Manager.autoRead = EditorGUILayout.Toggle("Auto read", Manager.autoRead);

        EditorGUI.indentLevel--;

        GUILayout.Label("Discovery settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        Manager.autoDiscover = EditorGUILayout.Toggle("Discover on play", Manager.autoDiscover);
        Manager.DiscoverTries = EditorGUILayout.IntField("Discovery tries", Manager.DiscoverTries);

        blacklistedFoldout = EditorGUI.Foldout(GUILayoutUtility.GetRect(40f, 40f, 16f, 16f, EditorStyles.foldout), blacklistedFoldout, "Blacklisted ports", true, EditorStyles.foldout);
        if (blacklistedFoldout)
        {

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * 15 + 4); ;

            GUILayout.Label("Serial port", "OL Titleleft");
            GUILayout.Label("", "OL Titleright", GUILayout.MaxWidth(35));
            GUILayout.EndHorizontal();

            foreach (string blackList in Manager.BlackListedPorts)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 15 + 4);
                GUILayout.Label(blackList, olLight);
                if (GUILayout.Button("×", "OL Titleright", GUILayout.MaxWidth(35)))
                {
                    Manager.BlackListedPorts.Remove(blackList);
                    EditorUtility.SetDirty(target);
                    return;
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * 15 + 4);
            EditorGUI.indentLevel--;
            newBlackListedPort = EditorGUILayout.TextField("", newBlackListedPort, olInput, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Add", "TE Toolbarbutton", GUILayout.MaxWidth(35)))
            {
                if (newBlackListedPort != "")
                    Manager.BlackListedPorts.Add(newBlackListedPort);
                newBlackListedPort = "";
                EditorUtility.SetDirty(target);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            EditorGUI.indentLevel++;

        }

        GUILayout.Label("On disconnect", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        Manager.stopAllOnQuit = EditorGUILayout.Toggle("Reset pins", Manager.stopAllOnQuit);
        GUILayout.EndHorizontal();


        GUILayout.Label("Debug", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        //  SetGUIBackgroundColor("#4FC3F7");
        if (GUILayout.Button("Get port state"))
        {
            Manager.GetPortState();
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        //  SetGUIBackgroundColor("#ef5350");
        if (GUILayout.Button("Clear console"))
        {
            var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }
        // SetGUIBackgroundColor();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.Label("Update Uduino", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;
        EditorGUILayout.HelpBox("Current version: " + UduinoVersion.getVersion(), MessageType.None);

        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        // SetGUIBackgroundColor("#4FC3F7");
        if (GUILayout.Button("Check for update"))
        {
            string url = "http://marcteyssier.com/uduino/version";
            WebRequest myRequest = WebRequest.Create(url);
            WebResponse myResponse = myRequest.GetResponse();
            Stream myStream = myResponse.GetResponseStream();
            StreamReader myReader = new StreamReader(myStream);
            checkVersion = myReader.ReadToEnd();
            myReader.Close();
            isUpToDateChecked = true;
            if (checkVersion == UduinoVersion.getVersion()) isUpToDate = true;
            else isUpToDate = false;
        }

        // SetGUIBackgroundColor();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        if (isUpToDateChecked)
        {
            if (isUpToDate) EditorGUILayout.HelpBox("Uduino is up to date (" + checkVersion + ")", MessageType.Info, true);
            else EditorGUILayout.HelpBox("Uduino is not up to date. Download the version  (" + checkVersion + ") on the Asset Store.", MessageType.Error, true);
        }

        EditorGUILayout.Separator();
    }

    public void DrawPin(Pin pin, bool editorPin = false, int arduinoBoard = 0)
    {
        GUILayout.BeginHorizontal();

        // TODO : get les type de pin que 1 fois et pas faire tout le temps en refresh
        string[] boardPins = BoardsTypeList.Boards.GetBoardFromId(arduinoBoard).GetPins();
        pin.currentPin = EditorGUILayout.Popup(pin.currentPin, boardPins, "ToolbarDropDown", GUILayout.MaxWidth(40));

        //old version for arduino
        // pin.currentPin = EditorGUILayout.Popup(pin.currentPin, EditorPin.arduinoUnoPins, "ToolbarDropDown", GUILayout.MaxWidth(40));

        pin.pinMode = (PinMode)EditorGUILayout.EnumPopup(pin.pinMode, "ToolbarDropDown", GUILayout.MaxWidth(55));
        pin.CheckChanges();
        GUILayout.BeginHorizontal();

        EditorGUIUtility.fieldWidth -= 22;
        serializedObject.ApplyModifiedProperties();

        switch (pin.pinMode)
        {
            case PinMode.Output:
                if (GUILayout.Button("HIGH", "toolbarButton"))
                    pin.SendPinValue(255, "d");
                if (GUILayout.Button("LOW", "toolbarButton"))
                    pin.SendPinValue(0, "d");
                break;
            case PinMode.Input_pullup:
                if (GUILayout.Button("Read", "toolbarButton", GUILayout.MaxWidth(55)))
                    pin.SendRead(digital: "d");
                GUILayout.Label(pin.lastReadValue.ToString(), "TE Toolbarbutton");
                UpdateReadPins(pin.arduinoName, pin.currentPin, pin.lastReadValue);
                break;
            case PinMode.PWM:
                GUILayout.BeginHorizontal("TE Toolbarbutton");
                pin.sendValue = EditorGUILayout.IntSlider(pin.sendValue, 0, 255);
                pin.SendPinValue(pin.sendValue, "a");
                GUILayout.EndHorizontal();
                break;
            case PinMode.Servo:
                GUILayout.BeginHorizontal("TE Toolbarbutton");
                pin.sendValue = EditorGUILayout.IntSlider(pin.sendValue, 0, 180);
                pin.SendPinValue(pin.sendValue, "a");
                GUILayout.EndHorizontal();
                break;
            case PinMode.Analog:
                if (GUILayout.Button("Read", "toolbarButton", GUILayout.MaxWidth(55)))
                    pin.SendRead(action: ParseReadData);
                GUILayout.Label(pin.lastReadValue.ToString(), "TE Toolbarbutton");
                UpdateReadPins(pin.arduinoName, pin.currentPin, pin.lastReadValue);
                break;
        }
        EditorGUIUtility.fieldWidth += 22;

        if (editorPin)
        {
            if (GUILayout.Button("-", "toolbarButton", GUILayout.Width(22)))
            {
                UduinoManagerEditor.Instance.RemovePin(pin);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.EndHorizontal();
    }
    #region Read and write values to Pins
    public void ParseReadData(string data)
    {
        //  Debug.Log(""); //This yield is cool !
        int recivedPin = -1;
        int.TryParse(data.Split(' ')[0], out recivedPin);

        int value = 0;
        int.TryParse(data.Split(' ')[1], out value);

        if (recivedPin != -1)
        {
            foreach (Pin pinTarget in Manager.pins)
            {
                if (pinTarget.PinTargetExists("", recivedPin))
                {
                    pinTarget.lastReadValue = value;
                }
            }
        }
    }

    /// <summary>
    /// Update the state of a read pin
    /// </summary>
    /// <param name="target"></param>
    /// <param name="pin"></param>
    /// <param name="value"></param>
    void UpdateReadPins(string target, int pin, int value)
    {
        foreach (Pin pinTarget in Manager.pins)
        {
            if (pinTarget.PinTargetExists(target, pin))
            {
                pinTarget.lastReadValue = value;
            }
        }
    }

    public void WriteMessage(string targetBoard, string message)
    {
        Manager.Write(targetBoard, message);
        Manager.ReadWriteArduino(targetBoard);
    }

    public void Read(string target = null, string variable = null, System.Action<string> action = null)
    {
        Manager.DirectReadFromArduino(target, variable, action: action);
        Manager.ReadWriteArduino(target);
    }
    #endregion

    public void RemovePin(Pin pin)
    {
        pin.Destroy();
        Manager.pins.Remove(pin);
    }

}

#else

/*
[InitializeOnLoad]
public class Startup
{
    static Startup()
    {
        Debug.Log("Uduino is up and running");
    }
}*/

[CustomEditor(typeof(UduinoManager))]
public class UduinoManagerEditor : Editor
{

    public static UduinoManagerEditor Instance { get; private set; }

#region Variables
    UduinoManager manager = null;

    //Style-relatedx
    Color headerColor = new Color(0.65f, 0.65f, 0.65f, 1);
    //Color backgroundColor = new Color(0.75f, 0.75f, 0.75f);
    Color defaultButtonColor;

    GUIStyle boldtext = null;
    GUIStyle olLight = null;
    GUIStyle olInput = null;
    GUIStyle customFoldtout = null;

#endregion

    const string define = "UDUINO_READY";

    bool defaultPanel = true;
    bool firstDone = false;
    bool secondDone = false;

    void OnEnable()
    {
        Instance = this;
    }

#region Styles
    void SetColorAndStyles()
    {
        if (boldtext == null)
        {
            //Color and GUI
            defaultButtonColor = GUI.backgroundColor;
            if (!EditorGUIUtility.isProSkin)
            {
                headerColor = new Color(165 / 255f, 165 / 255f, 165 / 255f, 1);
                //  backgroundColor = new Color(193 / 255f, 193 / 255f, 193 / 255f, 1);
            }
            else
            {
                headerColor = new Color(41 / 255f, 41 / 255f, 41 / 255f, 1);
                //    backgroundColor = new Color(56 / 255f, 56 / 255f, 56 / 255f, 1);
            }

            boldtext = new GUIStyle(GUI.skin.label);
            boldtext.fontStyle = FontStyle.Bold;
            boldtext.alignment = TextAnchor.UpperCenter;

            olLight = new GUIStyle("OL Titleleft");
            olLight.fontStyle = FontStyle.Normal;
            olLight.font = GUI.skin.button.font;
            olLight.fontSize = 9;
            olLight.alignment = TextAnchor.MiddleCenter;

            olInput = new GUIStyle("TE toolbar");
            olInput.fontStyle = FontStyle.Bold;
            olInput.fontSize = 10;
            olInput.alignment = TextAnchor.MiddleLeft;

            customFoldtout = new GUIStyle(EditorStyles.foldout);
            customFoldtout.fontStyle = FontStyle.Bold;

        }
    }

    void SetGUIBackgroundColor(string hex)
    {
        Color color = new Color();
        ColorUtility.TryParseHtmlString(hex, out color);
        GUI.backgroundColor = color;
    }
    void SetGUIBackgroundColor(Color color)
    {
        GUI.backgroundColor = color;
    }
    void SetGUIBackgroundColor()
    {
        GUI.backgroundColor = defaultButtonColor;
    }

    public void DrawLogo()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace(); // Nededed for lastRect
        EditorGUILayout.EndHorizontal();

        Texture tex = (Texture)EditorGUIUtility.Load("Assets/Uduino/Editor/Resources/uduino-logo.png");
        Texture tex2 = (Texture)EditorGUIUtility.Load("Assets/Uduino/Editor/Resources/arduino-bg.png");
        GUILayout.Space(0);
        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUI.Box(new Rect(1, lastRect.y + 4, Screen.width, 27), tex);
        lastRect = GUILayoutUtility.GetLastRect();

        Color bgColor = new Color();
        ColorUtility.TryParseHtmlString("#ffffff", out bgColor);

        EditorGUI.DrawRect(new Rect(lastRect.x - 15, lastRect.y + 5f, Screen.width + 1, 60f), bgColor);
        GUI.DrawTexture(new Rect(Screen.width / 2 - tex.width / 2 - 20, lastRect.y + 5, tex2.width, tex2.height), tex2, ScaleMode.ScaleToFit);

        GUI.DrawTexture(new Rect(Screen.width / 2 - tex.width / 2, lastRect.y + 10, tex.width, tex.height), tex, ScaleMode.ScaleToFit);

        GUI.color = Color.white;
        GUILayout.Space(60f);
    }

    public void DrawLine(int marginTop, int marginBottom, int height)
    {
        EditorGUILayout.Separator();
        GUILayout.Space(marginTop);
        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUI.Box(new Rect(0f, lastRect.y + 4, Screen.width, height), "");
        GUILayout.Space(marginBottom);
    }

    public bool DrawHeaderTitle(string title, bool foldoutProperty, Color backgroundColor)
    {
        GUILayout.Space(0);
        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUI.Box(new Rect(1, lastRect.y + 4, Screen.width, 27), "");
        lastRect = GUILayoutUtility.GetLastRect();
        EditorGUI.DrawRect(new Rect(lastRect.x - 15, lastRect.y + 5f, Screen.width + 1, 25f), headerColor);
        GUI.Label(new Rect(lastRect.x, lastRect.y + 10, Screen.width, 25), title);
        GUI.color = Color.clear;
        if (GUI.Button(new Rect(0, lastRect.y + 4, Screen.width, 27), ""))
        {
            foldoutProperty = !foldoutProperty;
        }
        GUI.color = Color.white;
        GUILayout.Space(30);
        if (foldoutProperty) { GUILayout.Space(5); }
        return foldoutProperty;
    }


#endregion
    public void FindExistingExtensions()
    {

    }

    public void CheckCompatibility()
    {
#if UNITY_5_6
        if (PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone) == ApiCompatibilityLevel.NET_2_0_Subset)
#else
        if (PlayerSettings.apiCompatibilityLevel == ApiCompatibilityLevel.NET_2_0_Subset)
#endif

        {
            SetGUIBackgroundColor("#ef5350");
            EditorGUILayout.HelpBox("Uduino works only with .NET 2.0 (not Subset).", MessageType.Error, true);
            SetGUIBackgroundColor();

            DrawLine(12, 0, 45);

            SetGUIBackgroundColor("#4FC3F7");
            if (GUILayout.Button("Fix Now", GUILayout.ExpandWidth(true)))
            {
#if UNITY_5_6
                PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_2_0);
#else
                PlayerSettings.apiCompatibilityLevel = ApiCompatibilityLevel.NET_2_0;
#endif
                PlayerSettings.runInBackground = true;
            }
            SetGUIBackgroundColor();
        } else
        {
         //   EditorGUILayout.HelpBox("Project settings is already set to .NET 2.0", MessageType.Info, true);
            StepDone();
            secondDone = true;
        }
    }
    string destinationFolder = "NOT SET";

    public override void OnInspectorGUI()
    {
        SetColorAndStyles();

        DrawLogo();

        DrawHeaderTitle("Uduino Setup", defaultPanel, headerColor);

		//EditorGUILayout.HelpBox("Before getting started, you need to set-up Uduino.", MessageType.None);
        //EditorGUILayout.Space();


        DrawHeaderTitle("1. Add Uduino library in the Arduino folder", defaultPanel, headerColor);

        if (!AssetDatabase.IsValidFolder("Assets/Uduino/Arduino"))
        {
            firstDone = true;
            StepDone();
        }

        if (AssetDatabase.IsValidFolder("Assets/Uduino/Arduino"))
        {

            EditorGUILayout.HelpBox("To use Uduino you will need the dedicated Arduino library. Select the Arduino libraries folder to add Uduino library.", MessageType.None, true);

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            EditorGUILayout.HelpBox("The Arduino libraries folder is located under: ~/Documents/Arduino/libraries ", MessageType.Info, true);
#else
           EditorGUILayout.HelpBox("The Arduino libraries folder is located under: C:/Users/<username>/Documents/Arduino/libraries", MessageType.Info, true);
#endif

			EditorGUILayout.LabelField ("Select Arduino libraries Folder");

            GUILayout.BeginHorizontal();

			GUILayout.BeginVertical();
            destinationFolder = EditorGUILayout.TextField(destinationFolder);
			GUILayout.EndVertical();
			GUILayout.BeginVertical();

            if (GUILayout.Button("Select path", GUILayout.ExpandWidth(true)))
            {
                GUI.FocusControl("");
                string path = EditorUtility.OpenFolderPanel("Set Arduino path", "", "");
                if (path.Length != 0)
                {
                    destinationFolder = path;
                }
            }
			GUILayout.EndVertical();

            GUILayout.EndHorizontal();


            DrawLine(12, 0, 45);
            SetGUIBackgroundColor("#4FC3F7");

            if (GUILayout.Button("Add Uduino library to Arduino", GUILayout.ExpandWidth(true)))
            {
				if(destinationFolder == "NOT SET")
				{
					if(EditorUtility.DisplayDialog("Move library folder", "You have to set a valid folder path", "Ok", "Cancel"))
					{ }
				} else if (EditorUtility.DisplayDialog("Move library folder", "Are you sure the Arduino libraries folder is " + destinationFolder + " ?", "Move", "Cancel"))
                {
                    MoveArduinoFiles(destinationFolder);
                }
            }
            SetGUIBackgroundColor();
        }

        EditorGUILayout.Separator();

        DrawHeaderTitle("2. Change project settings", defaultPanel, headerColor);
        CheckCompatibility();

        EditorGUILayout.Separator();


        DrawHeaderTitle("3. Start using Uduino!", defaultPanel, headerColor);

        if(firstDone && secondDone)
        {
            DrawLine(12, 0, 45);
            GUILayout.BeginVertical();
            SetGUIBackgroundColor("#4FC3F7");
            if (GUILayout.Button("I'm ready !"))
            {
                AddProjectDefine();
                AssetDatabase.Refresh();
            }

            GUILayout.EndVertical();
            EditorGUILayout.Separator();

        }
        else
        {
            if(!firstDone)
                EditorGUILayout.HelpBox("Add Uduino library into your Arduino folder", MessageType.Warning, true);
            if (!secondDone)
                EditorGUILayout.HelpBox("Modify the project settings", MessageType.Warning, true);

            DrawLine(12, 0, 45);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Skip setup", GUILayout.MaxWidth(90)))
            {
                AddProjectDefine();
                AssetDatabase.Refresh();
            }
            EditorGUI.BeginDisabledGroup(true);
            if (GUILayout.Button("Not ready yet...")){

            }
            EditorGUI.EndDisabledGroup();

            GUILayout.EndHorizontal();

      
            GUILayout.EndVertical();
            EditorGUILayout.Separator();

        }

        EditorGUILayout.Separator();
    }

    void StepDone()
    {
        SetGUIBackgroundColor("#00f908");
        EditorGUILayout.HelpBox("Step done !", MessageType.Info, true);
        SetGUIBackgroundColor();
    }

    void AddProjectDefine()
    {
        //TODO : Fix if the build target is changed !

        // Get defines.
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

        // Append only if not defined already.
        if (defines.Contains(define))
        {
            Debug.LogWarning("Selected build target (" + EditorUserBuildSettings.activeBuildTarget.ToString() + ") already contains <b>" + define + "</b> <i>Scripting Define Symbol</i>.");
            return;
        }
        // Append.
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, (defines + ";" + define));
        Debug.LogWarning("<b>" + define + "</b> added to <i>Scripting Define Symbols</i> for selected build target (" + EditorUserBuildSettings.activeBuildTarget.ToString() + ").");
    }

    void MoveArduinoFiles(string dest)
    {
        string sourceDirectory = Application.dataPath + "/Uduino/Arduino/libraries";
        string destinationDirectory = FirstToLower(dest);
        MoveDirectory(sourceDirectory, destinationDirectory);
        AssetDatabase.Refresh();
    }

    public static void MoveDirectory(string source, string target)
    {
        var stack = new Stack<Folders>();
        stack.Push(new Folders(source, target));

        while (stack.Count > 0)
        {
            var folders = stack.Pop();
            Directory.CreateDirectory(folders.Target);
            foreach (var file in Directory.GetFiles(folders.Source, "*.*"))
            {
                string targetFile = Path.Combine(folders.Target, Path.GetFileName(file));
                if (File.Exists(targetFile)) File.Delete(targetFile);
                File.Move(file, targetFile);
            }

            foreach (var folder in Directory.GetDirectories(folders.Source))
            {
                stack.Push(new Folders(folder, Path.Combine(folders.Target, Path.GetFileName(folder))));
            }
        }
        Directory.Delete(source, true);
        FileUtil.DeleteFileOrDirectory("Assets/Uduino/Arduino");
    }

    public class Folders
    {
        public string Source { get; private set; }
        public string Target { get; private set; }

        public Folders(string source, string target)
        {
            Source = source;
            Target = target;
        }
    }

    public string FirstToLower(string s)
    {
        if (string.IsNullOrEmpty(s))
            return string.Empty;

        char[] a = s.ToCharArray();
        a[0] = char.ToLower(a[0]);
        return new string(a);
    }
}
#endif
