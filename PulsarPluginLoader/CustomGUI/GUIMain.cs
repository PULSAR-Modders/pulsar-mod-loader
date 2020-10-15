using HarmonyLib;
using PulsarPluginLoader.Utilities;
using UnityEngine;

namespace PulsarPluginLoader.CustomGUI
{
    /*[HarmonyPatch(typeof(PLLevelSync), "LateUpdate")] //free the cursor when GUI is active
    class FreeCursor
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                
            };

            List<CodeInstruction> injectedSequence = new List<CodeInstruction>()
            {
                
            };

            return HarmonyHelpers.PatchBySequence(instructions, targetSequence, injectedSequence);
        }
    }
    [HarmonyPatch(typeof(PLMouseLook), "Update")] //Keep the mouselook locked when GUI is active
    class LockMouselook
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                
            };

            List<CodeInstruction> injectedSequence = new List<CodeInstruction>()
            {
                
            };

            return HarmonyHelpers.PatchBySequence(instructions, targetSequence, injectedSequence);
        }
    }*/
    [HarmonyPatch(typeof(PLGlobal), "Start")] //Create menu
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
        static float Height = .40f;
        static float Width = .40f;
        Rect Window = new Rect((Screen.width * .5f - ((Screen.width * Width)/2)), Screen.height * .5f - ((Screen.height * Height)/2), Screen.width * Width, Screen.height * Height);
        int TabSelectedID = 0;
        protected override void Update()
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
            if (GUI.Button(new Rect(0, 20, 80, 20), "Tab1", new GUIStyle("Toolbar")))//new GUIStyle("Toolbar")
            {
                TabSelectedID = 0;
            }
            if (GUI.Button(new Rect(100, 20, 80, 20), "Tab2", new GUIStyle("Toolbar")))//new GUIStyle("Toolbar")
            {
                TabSelectedID = 1;
                Messaging.Notification("Button2");
            }
            if (TabSelectedID == 0)
            {
                GUI.Label(new Rect (55, 0, 50, 20), "Unfinished, needs GUI expert.");
            }
            if (TabSelectedID == 1)
            {
                GUI.Label(new Rect (55, 0, 50, 20), "Filler2");
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }
    }
}
