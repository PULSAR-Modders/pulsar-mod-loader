using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulsarModLoader.Content.Dialogs
{
    public class HostSideDialog
    {
        public int DialogId;
        public virtual void OnCreate(out string Name, out string Text, out string[] Choices) {
            Name = null; Text = null; Choices = null;
        }
        public virtual void OnClick(PhotonPlayer Who, string SelectedChoice) { }
        public virtual void OnDestroy() { }

        public void AddText(string Text) => ModMessageHelper.Instance.photonView.RPC("DialogSyncText", PhotonTargets.All, DialogId, false, Text);
        public void SendNewChoices(string[] Choices) => ModMessageHelper.Instance.photonView.RPC("DialogSyncChoices", PhotonTargets.All, DialogId, Choices);
        public void Destroy() => ModMessageHelper.Instance.photonView.RPC("DialogDestroy", PhotonTargets.All, DialogId);
    }

    /*public class ExampleHostSideDialog : HostSideDialog {
        public override void OnCreate(out string name, out string text, out string[] choices) {
            name = "Example Dialog"; text = "Hello Captain!"; 
            choices = new string[] { "Hello!", "Goodbye" };
        }
        public override void OnClick(PhotonPlayer Who, string SelectedChoice) {
            switch (SelectedChoice) {
                case "Hello!":
                    AddText("Good luck!");
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
