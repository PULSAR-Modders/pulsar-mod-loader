using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulsarModLoader.Content.Dialogs
{
    public class HostSideDialog
    {
        public int id;
        public virtual void OnCreate(out string name, out string text, out string[] choices) {
            text = null; choices = null; name = null;
        }
        public virtual void OnClick(PhotonPlayer Who, string SelectedChoice) { }
        public virtual void OnDestroy() { }

        public void AddText(string text) => ModMessageHelper.Instance.photonView.RPC("DialogSyncText", PhotonTargets.All, id, false, text);
        public void SendNewChoices(string[] choices) => ModMessageHelper.Instance.photonView.RPC("DialogSyncChoices", PhotonTargets.All, id, choices);
        public void Destroy() => ModMessageHelper.Instance.photonView.RPC("DialogDestroy", PhotonTargets.All, id);
    }

    /*public class ExampleHostSideDialog : HostSideDialog {
        public override void OnCreate(out string name, out string text, out string[] choices) {
            name = "Example Dialog"; text = "Hello Captain!"; 
            choices = new string[] { "Hello!", "Goodbye" };
        }
        public override void OnClick(PhotonPlayer Who, string SelectedChoice) {
            switch (SelectedChoice) {
                case "Hello!":
                    AddText("force be with you!");
                    SendNewChoices(new[] { "Goodbye" });
                    break;
                case "Goodbye":
                    Destroy();
                    break;
            }
        }
    }
    public class ExampleDialogCreate : PulsarModLoader.Chat.Commands.CommandRouter.ChatCommand {
        public override void Execute(string arguments) {
            if (PhotonNetwork.isMasterClient)
                DialogsManager.Instance.CreateDialog<ExampleHostSideDialog>();
        }
        public override string Description() => "Example Description";
        public override string[] CommandAliases() => new[] { "ex" };
    } */
}
