
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
namespace PulsarModLoader
{

    public class PMLKeybind
    {
        public string Name;
        public string ID;
        public string Category;
        public string Key;


        public PMLKeybind(string inName, string inID, string inCategory, string inKey)
        {
            
            this.Name = inName;
            this.ID = inID;
            this.Category = inCategory;
            this.Key = inKey;
        }
    }

    public interface IKeybind
    {
        void RegisterBinds(KeybindManager manager);
    }
    public class KeybindManager
    {
            private static KeybindManager _instance;
            public List<PMLKeybind> keybindings = new List<PMLKeybind>();
            private KeybindManager()
            {
                ModManager.Instance.OnModSuccessfullyLoaded += OnModLoaded;
            }
            public static KeybindManager Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new KeybindManager();
                        
                    }

                    return _instance;
                }
            }


        void OnModLoaded(string modName, PulsarMod mod)
        {
            if (mod is IKeybind k)
            {
                k.RegisterBinds(KeybindManager.Instance);
            }
        }
        public PMLKeybind GetPMLKeybind(string inID)
        {
            return keybindings.FirstOrDefault(k => k.ID == inID);
        }

        public bool GetButtonDown(string inID)
        {
            return PLInput.Instance.GetButtonDown(inID);
        }

        public void NewBind(string inName, string inID, string inCategory, string inKey)
        {
            KeybindManager.Instance.keybindings.Add(new PMLKeybind(inName, inID, inCategory, inKey));
        }

       
    }
 
    [HarmonyPatch(typeof(PLInput), "LoadFromXmlDoc")]
    internal class LoadFromXmlPatch
    {
        private static void Postfix(PLInput __instance, string xmlFileName)
        {
                    foreach (PMLKeybind keybind in KeybindManager.Instance.keybindings)
                    {
                        List<PLInputAction> list = __instance.FindActionsByID(keybind.ID);
                        int num = 0;
                        PLInputCategory plinputCategory = null;
                        foreach (PLInputCategory plinputCategory2 in __instance.AllInputCategories)
                        {
                            if (plinputCategory2 != null && plinputCategory2.m_Name == keybind.Category)
                            {
                                plinputCategory = plinputCategory2;
                                break;
                            }
                        }
                        using (List<PLInputAction>.Enumerator enumerator4 = list.GetEnumerator())
                        {
                            while (enumerator4.MoveNext())
                            {
                                if (enumerator4.Current.m_Category == plinputCategory)
                                {
                                    num++;
                                }
                            }
                        }
              
                        if (num == 0 && plinputCategory != null)
                        {
                            var plinputAction = new PLInputAction(keybind.Name, keybind.ID, plinputCategory);
                            var plinputKey = new PLInputKey();
                            plinputKey.Type = "standard";
                            plinputKey.ID = keybind.Key.ToLower();
                            plinputKey.ID_Upper = keybind.Key.ToUpper();
                            plinputAction.AddKey(plinputKey);
                            __instance.AllInputActions.Add(plinputAction);
                            plinputAction.m_Category = plinputCategory;
                            __instance.SaveToXmlFile(xmlFileName, false);
                            

                        }
                    }
        }
    }

}
