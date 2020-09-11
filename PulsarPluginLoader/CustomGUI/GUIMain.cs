using HarmonyLib;
using PulsarPluginLoader.Utilities;
using UnityEngine;

namespace PulsarPluginLoader.CustomGUI
{
    /*armonyPatch(typeof(PLLevelSync), "LateUpdate")] //free the cursor when GUI is active
    class FreeCursor
    {
        static void Transpiler()
        {

        }
    }
    [HarmonyPatch(typeof(PLMouseLook), "Update")] //Keep the mouselook locked when GUI is active
    class LockMouselook
    {
        static void Transpiler()
        {

        }
    }*/
    [HarmonyPatch(typeof(PLServer), "Start")] //Create menu
    class CreateMenu
    {
        static void Postfix()
        {
            if (GUIMain.Instance == null)
            {
                UnityEngine.GameObject GUI = new UnityEngine.GameObject();
                UnityEngine.Object.DontDestroyOnLoad(GUI);
                GUI.AddComponent<GUIMain>();
                GUIMain.Instance = GUI.GetComponent<GUIMain>();
            }
        }
    }

    class GUIMain : PLMonoBehaviour
    {//to free mouse, line 58 in PLLevelSync.LateUpdate && PLMouseLook.Update line 71
        public static GUIMain Instance = null;
        public bool GUIActive = false;
        Rect Window = new Rect(500, 200, 500, 500);
        int TabSelectedID = 0;
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                GUIActive = !GUIActive;
            }
        }
        void OnGUI()
        {
            if (GUIActive)
            {
                Window = GUI.Window(999910, Window, WindowFunction, "ModManager");
            }
        }
        void WindowFunction(int WindowID)
        {
            if (GUI.Button(new Rect(0, 20, 80, 20), "Tab1"))//new GUIStyle("Toolbar")
            {
                TabSelectedID = 0;
            }
            if (GUI.Button(new Rect(100, 20, 80, 20), "Tab2"))//new GUIStyle("Toolbar")
            {
                TabSelectedID = 1;
                Messaging.Notification("Button2");
            }
            if (TabSelectedID == 0)
            {
                GUI.Label(new Rect (55, 0, 50, 20), "Filler1");
            }
            if (TabSelectedID == 1)
            {
                GUI.Label(new Rect (55, 0, 50, 20), "Filler2");
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }
    }
}
