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
        bool showAttribObj = true;
        bool writeData = false;
        private string Dir
        {
            get => dir;
            set
            {
                dir = value;
                Debug.Log("Do something");
            }
        }

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

            if (checkBtn && data != null)
            {
                foreach (JProperty property in curData.Properties())
                {
                    if (property.Value.ToString().StartsWith("{") && property.Value.ToString().EndsWith("}") && property.Value.ToString().Contains(":"))
                    {
                        JObject dataTrans = (JObject) property.Value;
                        ExportDeepDict(curData, property.Name, dataTrans);

                    } else if(property.Value.ToString().StartsWith("[") && property.Value.ToString().EndsWith("]"))
                    {
                        JArray childData = (JArray)property.Value;
                        nameAttribArr = property.Name;
                        showAttribArr = EditorGUILayout.Foldout(showAttribArr, nameAttribArr);
                        if (showAttribArr)
                        {
                            if (!Selection.activeTransform)
                            {
                                EditorGUI.BeginChangeCheck();

                                for (int i = 0; i < childData.Count; i++)
                                {
                                    EditorGUI.indentLevel++;
                                    nameAttribArr = EditorGUILayout.TextField(childData[i].ToString());
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        curData[property.Name][i] = nameAttribArr;
                                    }
                                    EditorGUI.indentLevel--;
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
                        nameAttrib = EditorGUILayout.TextField(nameAttrib, property.Value.ToString());
                        GUILayout.EndHorizontal();

                        if (EditorGUI.EndChangeCheck())
                        {
                            curData[property.Name] = nameAttrib;
                        }
                    }
                }
            }
            if (checkBtn && data != null)
            {
                if (GUILayout.Button("Save Data", GUILayout.Height(20), GUILayout.Width(100)))
                {
                    writeData = true;
                }
            }
            if (writeData)
            {
                WriteToJson(curData, dir);
                writeData = false;
            }
        }
        #endregion

        public JObject LoadJson(string filePath)
        {
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

        public void ExportDeepDict(JObject curData, string dataName, JObject data)
        {
            foreach(JProperty property in data.Properties())
            {
                string dataValue = property.Value.ToString();
                if (dataValue.StartsWith("{") && dataValue.EndsWith("}") && dataValue.Contains(":"))
                {
                    JObject dataCallBack = (JObject)property.Value;
                    ExportDeepDict(curData, dataName, dataCallBack);
                }
                else
                {
                    nameAttribObj = dataName;
                    showAttribObj = EditorGUILayout.Foldout(showAttribObj, nameAttribObj);
                    if (showAttribObj)
                    {
                        if (!Selection.activeTransform)
                        {
                            //EditorGUI.BeginChangeCheck();
                            foreach (JProperty child in data.Properties())
                            {
                                EditorGUI.indentLevel++;
                                nameAttribObj = EditorGUILayout.TextField($"{child.Name}: ", dataValue);

                                /*if (EditorGUI.EndChangeCheck())
                                {
                                    curData[dataName][child.Name] = nameAttribObj;
                                }*/
                                EditorGUI.indentLevel--;

                            }
                        }
                        if (Selection.activeTransform)
                        {
                            nameAttribObj = dataName;
                            showAttribObj = false;
                        }
                    }
                }
            }
            
        }
    }
}

