using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using static Codice.CM.WorkspaceServer.DataStore.IncomingChanges.StoreIncomingChanges.FileConflicts;
using log4net.Util;
using UnityEngine.UI;
using System.Drawing.Printing;
using static Codice.CM.WorkspaceServer.WorkspaceTreeDataStore;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Specialized;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using PlasticPipe.PlasticProtocol.Messages;
using System.Linq;

namespace MyTools
{
    public class ProjectSetup_window : EditorWindow
    {
        #region Variables
        static ProjectSetup_window win;
        protected SerializedObject serializedObject;
        protected SerializedProperty currentProperty;

        private string dir = "";
        string nameAttribArr = "";
        string nameAttribObj = "";
        string nameAttrib = "";

        bool showAttribArr = true;
        bool writeData = false;
        bool reloadData = false;

        Vector2 paletteScrollPos = new Vector2(0, 0);

        //IDictionary<string, bool> showAttribObj = new Dictionary<string, bool>();

        Dictionary<string, bool> showAttribObj = new Dictionary<string, bool>();

        private bool checkBtn = false;
        JObject data = null;
        JObject curData = new();
        #endregion

        #region Main Methods
        public static void InitWindow()
        {
            win = EditorWindow.GetWindow<ProjectSetup_window>("Project Setup");
            win.Show();
        }

        void OnGUI()
        {

            GUILayout.Label("This is a label.", EditorStyles.boldLabel);

            dir = EditorGUILayout.TextField("Diretion: ", dir);

            EditorGUILayout.Space();

            if (GUILayout.Button("Find Data", GUILayout.Height(20), GUILayout.Width(100)))
            {
                checkBtn = true;

                if (File.Exists(dir))
                {
                    data = LoadJson(dir);
                    foreach (JProperty property in data.Properties())
                    {
                        curData.Add(property.Name, property.Value);
                    }
                }
            }
            paletteScrollPos = EditorGUILayout.BeginScrollView(paletteScrollPos, GUILayout.ExpandWidth(true));

            if (checkBtn && data != null)
            {
                foreach (JProperty property in curData.Properties())
                {
                    if (property.Value.Type.ToString() == "Object")
                    {
                        if (!showAttribObj.ContainsKey(property.Name))
                        {
                            showAttribObj.Add(property.Name, true);
                        }

                        nameAttribObj = property.Name;
                        showAttribObj[property.Name] = EditorGUILayout.Foldout(showAttribObj[property.Name], nameAttribObj);
                        if (showAttribObj[property.Name])
                        {
                            if (!Selection.activeTransform)
                            {
                                JObject newProperty = (JObject)property.Value;
                                foreach (JProperty child in newProperty.Properties())
                                {
                                    EditorGUI.indentLevel++;
                                    ExportDeepDict(child.Name, child.Value, newProperty);
                                    EditorGUI.indentLevel--;
                                }
                            }
                            else if (Selection.activeTransform)
                            {
                                nameAttribObj = property.Name;
                                showAttribObj[property.Name] = false;
                            }
                        }
                    }
                    else if (property.Value.Type.ToString() == "Array")
                    {
                        JArray childData = (JArray)property.Value;
                        nameAttribArr = property.Name;
                        showAttribArr = EditorGUILayout.Foldout(showAttribArr, nameAttribArr);
                        if (showAttribArr)
                        {
                            if (!Selection.activeTransform)
                            {
                                for (int i = 0; i < childData.Count; i++)
                                {
                                    ExportDeepList(childData[i], childData, i);
                                }
                            }

                            if (Selection.activeTransform)
                            {
                                nameAttribArr = property.Name;
                                showAttribArr = false;
                            }
                        }
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        GUILayout.BeginHorizontal();
                        nameAttrib = property.Name;
                        nameAttrib = EditorGUILayout.TextField(nameAttrib, property.Value.ToString(), GUILayout.Width(200));

                        GUILayout.EndHorizontal();

                        if (EditorGUI.EndChangeCheck())
                        {
                            if (curData[property.Name].Type.ToString() == "Integer")
                            {
                                try
                                {
                                    curData[property.Name] = Int32.Parse(nameAttrib);
                                }
                                catch (FormatException)
                                {
                                    Debug.Log("Error");
                                }
                            }
                            else
                            {
                                curData[property.Name] = nameAttrib;
                            }
                        }
                    }
                }
            }

            if (checkBtn && data != null)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Save Data", GUILayout.Height(30), GUILayout.Width(200)))
                {
                    writeData = true;
                }

                if (GUILayout.Button("Reload Data", GUILayout.Height(30), GUILayout.Width(200)))
                {
                    reloadData = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (writeData)
            {
                WriteToJson(curData, dir);
                data = LoadJson(dir);
                writeData = false;
            }
            if (reloadData)
            {
                data = LoadJson(dir);
                curData = new();
                foreach (JProperty property in data.Properties())
                {
                    curData.Add(property.Name, property.Value);
                }
                reloadData = false;
            }

            EditorGUILayout.EndScrollView();
        }
        #endregion

        public JObject LoadJson(string filePath)
        {
            //JsonDictionaryAttribute jsonDictionaryAttribute = new JsonDictionaryAttribute();
            JObject data = JObject.Parse(File.ReadAllText(filePath));
            return data;
        }
        public void WriteToJson(JObject data, string pathFile)
        {
            using (StreamWriter file = File.CreateText(@pathFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, data);
            }
        }

        public void ExportDeepDict(string dataName, JToken dataObject, JObject parentDict)
        {
            if (dataObject.Type.ToString() == "Object")
            {
                if (!showAttribObj.ContainsKey(dataName))
                {
                    showAttribObj.Add(dataName, true);
                }
                JObject dataCallBack = (JObject)dataObject;
                nameAttribObj = dataName;
                showAttribObj[dataName] = EditorGUILayout.Foldout(showAttribObj[dataName], nameAttribObj);
                if (showAttribObj[dataName])
                {
                    if (!Selection.activeTransform)
                    {
                        foreach (JProperty child in dataCallBack.Properties())
                        {
                            EditorGUI.indentLevel++;
                            ExportDeepDict(child.Name, child.Value, dataCallBack);
                            EditorGUI.indentLevel--;
                        }
                    }
                    else if (Selection.activeTransform)
                    {
                        nameAttribObj = dataName;
                        showAttribObj[dataName] = false;
                    }
                }
            }
            else if (dataObject.Type.ToString() == "Array")
            {
                if (!showAttribObj.ContainsKey(dataName))
                {
                    showAttribObj.Add(dataName, true);
                }

                JArray childData = (JArray)dataObject;

                nameAttribObj = dataName;
                showAttribObj[dataName] = EditorGUILayout.Foldout(showAttribObj[dataName], nameAttribObj);
                if (showAttribObj[dataName])
                {
                    if (!Selection.activeTransform)
                    {
                        EditorGUILayout.BeginVertical("box");
                        for (int i = 0; i < childData.Count; i++)
                        {
                            EditorGUI.indentLevel++;
                            ExportDeepList(childData[i], childData, i);
                            EditorGUI.indentLevel--;
                        }
                        EditorGUILayout.EndVertical();
                    }
                    else if (Selection.activeTransform)
                    {
                        nameAttribObj = dataName;
                        showAttribObj[dataName] = false;
                    }
                }
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.indentLevel++;
                nameAttribObj = EditorGUILayout.TextField($"{dataName}: ", dataObject.ToString());
                if (EditorGUI.EndChangeCheck())
                {
                    if (parentDict[dataName].Type.ToString() == "Integer")
                    {
                        try
                        {
                            parentDict[dataName] = Int32.Parse(nameAttribObj);
                            Debug.Log(dataName);
                            Debug.Log(data.Property(dataName));
                        }
                        catch (FormatException e)
                        {
                            Debug.Log(e);
                        }
                    }
                    else
                    {
                        parentDict[dataName] = nameAttribObj;
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        public void ExportDeepList(JToken data, JArray parentData, int index)
        {
            if (data.Type.ToString() == "Object")
            {
                JObject dataCallBack = (JObject)data;
                EditorGUILayout.BeginVertical("box");
                foreach (JProperty child in dataCallBack.Properties())
                {
                    EditorGUI.indentLevel++;
                    ExportDeepDict(child.Name, child.Value, dataCallBack);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.indentLevel++;
                nameAttribArr = EditorGUILayout.TextField(data.ToString());
                if (EditorGUI.EndChangeCheck())
                {
                    if (parentData[index].Type.ToString() == "Integer")
                    {
                        try
                        {
                            parentData[index] = Int32.Parse(nameAttribArr);
                        }
                        catch (FormatException)
                        {
                            Debug.Log("Error");
                        }
                    }
                    else
                    {
                        parentData[index] = nameAttribArr;
                    }
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}