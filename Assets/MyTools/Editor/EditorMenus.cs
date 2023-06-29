using UnityEngine;
using UnityEditor;
using MyTools;

public class EditorMenus : MonoBehaviour
{
    [MenuItem("MyTools/Project/Project Setup Tool")]
    public static void initProjectSetupTool()
    {
        ProjectSetup_window.InitWindow();
    }
}
