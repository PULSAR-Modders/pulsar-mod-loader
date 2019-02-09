using PulsarPluginLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PulsarPluginLoader.Chat.Commands.Devhax
{
    class MaxOutCommand : IChatCommand
    {
        private static readonly int MAX_LEVEL = 15;

        public string[] CommandAliases()
        {
            return new string[] { "maxout" };
        }

        public string Description()
        {
            return "Maxes out all talents, crew items, and ship components.";
        }

        public string UsageExample()
        {
            return $"/{CommandAliases()[0]}";
        }

        public bool Execute(string arguments)
        {
            if (PhotonNetwork.isMasterClient && DevhaxCommand.IsEnabled)
            {
                UnlockAllTalents();
                MaxAllTalents();
                MaxAllItems();
                MaxAllComponents();

                Messaging.Notification(PhotonTargets.All, $"Maxed out all levels.");
            }
            else
            {
                string reason = !DevhaxCommand.IsEnabled ? "Cheats Disabled" : "Not Host";
                Messaging.Notification(PLNetworkManager.Instance.LocalPlayer.GetPhotonPlayer(), $"Command Failed: {reason}");
            }

            return false;
        }

        private void UnlockAllTalents()
        {
            PLServer.Instance.TalentLockedStatus = 0L;
        }

        private void MaxAllTalents()
        {
            foreach (ETalents talentID in Enum.GetValues(typeof(ETalents)))
            {
                TalentInfo talent = PLGlobal.GetTalentInfoForTalentType(talentID);

                if (talent != null)
                {
                    foreach (PLPlayer player in PLServer.Instance.AllPlayers)
                    {
                        if (player.TeamID == 0 && (talent.ClassID == -1 || talent.ClassID == player.GetClassID()))
                        {
                            for (int i = player.Talents[talent.TalentID]; i < talent.MaxRank; i++)
                            {
                                if (player.TalentPointsAvailable < 1)
                                {
                                    player.TalentPointsAvailable++;
                                }

                                if (PLServer.Instance.CurrentCrewLevel < talent.MinLevel)
                                {
                                    PLServer.Instance.CurrentCrewLevel = talent.MinLevel;
                                }

                                player.photonView.RPC("ServerRankTalent", PhotonTargets.MasterClient, new object[]
                                {
                                    talent.TalentID
                                });
                            }
                        }
                    }

                }
            }
        }

        private void MaxAllItems()
        {
            foreach (PLPlayer player in PLServer.Instance.AllPlayers)
            {
                if (player != null)
                {
                    bool includeLocker = true;

                    foreach (List<PLPawnItem> inventory in player.MyInventory.GetAllItems(includeLocker))
                    {
                        foreach (PLPawnItem item in inventory)
                        {
                            if (item != null)
                            {
                                item.Level = MAX_LEVEL;
                                item.AmmoCurrent = item.AmmoMax;
                            }
                        }
                    }
                }
            }
        }

        private void MaxAllComponents()
        {
            PLShipInfo ship = PLEncounterManager.Instance.PlayerShip;

            if (ship != null)
            {
                // Upgrade ship components
                foreach (PLShipComponent component in ship.MyStats.AllComponents)
                {
                    if (component != null)
                    {
                        component.Level = MAX_LEVEL;
                    }
                }

                // Fix ship's current stats since upgrades only change max values.
                // Use a background thread that waits for ship.MyStats to update;
                // hackish but we don't have any way to respond to these events yet.
                // TODO:  Update this in response to events some day.
                new Thread(() =>
                {
                    Thread.Sleep(500);

                    PLServer.Instance.photonView.RPC("ServerRepairHull", PhotonTargets.MasterClient, new object[]
                    {
                        ship.ShipID,
                        (int)(ship.MyStats.HullMax - ship.MyStats.HullCurrent),
                        0
                    });

                    ship.NumberOfFuelCapsules = int.MaxValue;

                    ship.MyStats.ReactorTempCurrent = 0.0f;

                    ship.ReactorCoolantLevelPercent = 1.0f;
                    ship.ReactorCoolingPumpState = 0;
                    ship.ReactorCoolingEnabled = false;
                }).Start();
            }
        }
    }
}
