using HarmonyLib;
using PulsarModLoader.Chat.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulsarModLoader.Patches
{
    [HarmonyPatch(typeof(PLServer), "Start")]
    class PLServerStart
    {
        static void Postfix()
        {
            //Chat Extensions
            ChatHelper.publicCached = false;
            HandlePublicCommands.RequestPublicCommands();
        }
    }
}
