using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulsarModLoader.Content.Dialogs
{
    internal class GenericDialog : PLHailTarget
    {
        public int id;
        public string leftText = String.Empty;
        public string rightText = String.Empty;
        public string dialogName = String.Empty;
        bool canClick = true;
        float timeFromLastClick = 0f;

        public void SetChoices(string[] choices)
        {
            this.m_AllChoices.Clear();
            if (choices.Length != 0)
            {
                foreach (string choice in choices)
                    this.m_AllChoices.Add(new CustomChoice(this, choice, id));
            }
            UpdateAvailableChoicesList();
        }

        public void AddText(bool right, string content)
        {
            rightText += '\n';
            leftText += '\n';
            _ = right ? rightText += $"\n{content}" : leftText += $"\n{content}";
        }

        public override void Update() // anti double click ><
        {
            if (!canClick)
            {
                timeFromLastClick += UnityEngine.Time.deltaTime;
                if (timeFromLastClick > 2f)
                {
                    canClick = true;
                }
            }
            base.Update();
        }

        public override string GetCurrentDialogueLeft() => leftText;
        public override string GetCurrentDialogueRight() => rightText;
        public override string GetName() => dialogName;

        private class CustomChoice : PLHailChoice
        {
            int id;
            string text;
            GenericDialog instance;
            public CustomChoice(GenericDialog gd, string text, int id)
            {
                this.OnSelect = Selected;
                this.text = text;
                this.id = id;
                instance = gd;
            }

            public override string GetText() => text;

            private void Selected(bool authority, bool local)
            {
                if (!instance.canClick)
                    return;

                instance.canClick = false;
                instance.timeFromLastClick = 0f;
                ModMessageHelper.Instance.photonView.RPC("DialogClick", PhotonTargets.MasterClient, id, text);
            }
        }
    }
}
