﻿using HarmonyLib;
using PulsarModLoader.Chat.Commands;
using System;
using UnityEngine;

namespace PulsarModLoader.Utilities
{
    [HarmonyPatch(typeof(PLNetworkManager), "Start")]
    class ExceptionWarningPatch
    {
        private static void Prefix()
        {
            Application.logMessageReceived += OnUnityLog;
        }
        private static void OnUnityLog(string line, string stackTrace, LogType type)
        {
            if (type.Equals(LogType.Exception))
            {
                string id = String.Format("{0:X}", DateTime.UtcNow.GetHashCode()).Substring(0, 7).ToUpper();
                string msg = $"<color='#{ColorUtility.ToHtmlStringRGB(Color.red)}'>Exception!</color> {id}";
                if (PMLConfig.DebugMode && PLNetworkManager.Instance != null && PLNetworkManager.Instance.LocalPlayer != null)
                {
                    Messaging.Notification(msg);
                }
                Logger.Info($"Exception ID: {id}");
            }
        }
    }
}
