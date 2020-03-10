using HarmonyLib;
using UnityEngine;

namespace PulsarPluginLoader.Chat.Commands.Devhax.ToggleCam
{
    [HarmonyPatch(typeof(PLCameraMode_Scripted), "Tick")]
    class DropCam
    {
        static void Postfix(Vector3 ___CurrentPos, Quaternion ___CurrentRot)
        {
            /* Triple underscore parameters map to private fields on the original object instance */
            if (PLInput.Instance.GetButtonDown(PLInput.EInputActionName.flashlight))
            {
                PLPawn playerPawn = GetAvailableViewPawn();

                if (playerPawn != null)
                {
                    playerPawn.transform.localPosition = ___CurrentPos;

                    Vector3 playerRotation = playerPawn.transform.localEulerAngles;
                    playerPawn.transform.localRotation = Quaternion.Euler(playerRotation.x, ___CurrentRot.eulerAngles.y, playerRotation.z);

                    PLCameraSystem.Instance.ChangeCameraMode(new PLCameraMode_LocalPawn());
                }
            }
        }

        private static PLPawn GetAvailableViewPawn()
        {
            PLPawn deadPawn = null;

            if (PLGameStatic.Instance != null && PLNetworkManager.Instance != null)
            {
                foreach (PLPawn pawn in PLGameStatic.Instance.AllPawns)
                {
                    if (pawn != null && pawn.GetPlayer() == PLNetworkManager.Instance.LocalPlayer)
                    {
                        if (!pawn.IsDead)
                        {
                            return pawn;
                        }
                        else
                        {
                            deadPawn = pawn;
                        }
                    }
                }
            }

            return deadPawn;
        }
    }
}
