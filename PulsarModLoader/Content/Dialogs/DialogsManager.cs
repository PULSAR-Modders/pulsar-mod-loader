using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulsarModLoader.Content.Dialogs
{
    public class DialogsManager
    {
        public static DialogsManager Instance
        {
            get; internal set;
        }

        internal Dictionary<int, GenericDialog> ActiveDialogs = new Dictionary<int, GenericDialog>();
        public Dictionary<int, HostSideDialog> ActiveHostSideDialogs = new Dictionary<int, HostSideDialog>();

        private int Id = 90000;

        public void CreateDialog<T>() where T : HostSideDialog, new()
        {
            var instance = new UnityEngine.GameObject().AddComponent<GenericDialog>();
            var dialog = new T();
            ActiveDialogs.Add(Id,instance);
            instance.id = Id;
            instance.HailTargetID = Id;
            ActiveHostSideDialogs.Add(Id, dialog);
            dialog.id = Id;
            dialog.OnCreate(out string name, out string text, out string[] choices);
            ModMessageHelper.Instance.photonView.RPC("SendDialogCreate", PhotonTargets.Others, Id, name, text, choices);
            instance.AddText(false, text);
            instance.SetChoices(choices);
            instance.dialogName = name;
            Id++;
        }

        internal GenericDialog CreateGenericDialog(int id)
        {
            Id = id;
            var dialog = new UnityEngine.GameObject().AddComponent<GenericDialog>();
            dialog.id = id;
            dialog.HailTargetID = Id;
            ActiveDialogs.Add(id, dialog);
            return dialog;
        }

        internal void DestroyDialog(int id)
        {
            if (ActiveHostSideDialogs.TryGetValue(id, out var dialog))
                dialog.OnDestroy();
            
            UnityEngine.GameObject.Destroy(ActiveDialogs[id].gameObject);
            ActiveDialogs.Remove(id);
        }
    }
}
