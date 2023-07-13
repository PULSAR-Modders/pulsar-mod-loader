using System.Collections.Generic;
using System.Linq;
namespace PulsarModLoader.Keybinds
{
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
        public bool GetButton(string inID)
        {
            return PLInput.Instance.GetButton(inID);
        }

        public void NewBind(string inName, string inID, string inCategory, string inKey)
        {
            KeybindManager.Instance.keybindings.Add(new PMLKeybind(inName, inID, inCategory, inKey));
        }
    }
}
