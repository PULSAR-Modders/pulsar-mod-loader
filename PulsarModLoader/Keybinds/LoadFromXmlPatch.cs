using HarmonyLib;
using System.Collections.Generic;
namespace PulsarModLoader.Keybinds
{
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
