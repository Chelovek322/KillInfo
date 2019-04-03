using Smod2;
using Smod2.Events;
using Smod2.EventHandlers;
using Smod2.API;
using System;
using System.Collections.Generic;
using KillInfo.Managers;

namespace KillInfo
{
	class KillInfoEventLogic : IEventHandlerPlayerDie,IEventHandlerRoundEnd,IEventHandlerShoot, IEventHandlerWaitingForPlayers, IEventHandlerCallCommand
	{
		public Dictionary<string, PlayerInfo> CheckSteamIDForKillInfo = new Dictionary<string, PlayerInfo>();

		public KillFileReadAndWrite KillReadAndWrite = new KillFileReadAndWrite();

		public ConfigOptions configOptions = new ConfigOptions();

		readonly Plugin plugin;
		public KillInfoEventLogic(Plugin plugin)
		{
			this.plugin = plugin;
		}
		
		public void OnPlayerDie(PlayerDeathEvent ev)
		{
			if (!CheckSteamIDForKillInfo.ContainsKey(ev.Player.SteamId))
			{
				CheckSteamIDForKillInfo[ev.Player.SteamId] = KillReadAndWrite.ReadPlayerBySteamID(ev.Player.SteamId);
			}

			CheckSteamIDForKillInfo[ev.Player.SteamId].AddDeath(ev.DamageTypeVar);

			if (ev.Killer != null && ev.Player.SteamId != ev.Killer.SteamId)
			{
				if (!CheckSteamIDForKillInfo.ContainsKey(ev.Killer.SteamId))
				{
					CheckSteamIDForKillInfo[ev.Killer.SteamId] = KillReadAndWrite.ReadPlayerBySteamID(ev.Killer.SteamId);
				}
				CheckSteamIDForKillInfo[ev.Killer.SteamId].AddKill(ev.DamageTypeVar);
				ev.Killer.PersonalBroadcast(2, $"{CheckSteamIDForKillInfo[ev.Killer.SteamId].GetKillByDamageType(ev.DamageTypeVar)} (+{ CheckSteamIDForKillInfo[ev.Killer.SteamId].GetCurrentKillsByDamageType(ev.DamageTypeVar)}):Убийств с помощью {ev.DamageTypeVar.ToString()}", true);
			}
		}

		public void OnRoundEnd(RoundEndEvent ev)
		{
			if(!configOptions.CheckForFalseRoundEnd)
			{
				return;
			}

			foreach (Player playa in Smod2.PluginManager.Manager.Server.GetPlayers())
			{

				if (!CheckSteamIDForKillInfo.ContainsKey(playa.SteamId))
				{
					CheckSteamIDForKillInfo[playa.SteamId] = KillReadAndWrite.ReadPlayerBySteamID(playa.SteamId);
				}
		
				playa.SendConsoleMessage($"Твоя точность: {CheckSteamIDForKillInfo[playa.SteamId].GetShotInfo(2)}%. ({CheckSteamIDForKillInfo[playa.SteamId].GetShotInfo(1)}\\{CheckSteamIDForKillInfo[playa.SteamId].GetShotInfo(0)})");

				foreach (DamageType dmgtype in (DamageType[])Enum.GetValues(typeof(DamageType)))
				{
					if (CheckSteamIDForKillInfo[playa.SteamId].GetKillByDamageType(dmgtype) != 0)
					{
						playa.SendConsoleMessage("Ты убил " + CheckSteamIDForKillInfo[playa.SteamId].GetKillByDamageType(dmgtype) + "(+" + CheckSteamIDForKillInfo[playa.SteamId].GetCurrentKillsByDamageType(dmgtype) + " в этом раунде) человек с помощью " + dmgtype.ToString().Replace("_", "-") + ".");
					}
				}
				
				if (CheckSteamIDForKillInfo[playa.SteamId].GetAmountOfDeaths() == 0)
				{
					playa.SendConsoleMessage("Твой КДА: " + CheckSteamIDForKillInfo[playa.SteamId].GetAmountOfKills() + ". ");
				}
				else
				{
					playa.SendConsoleMessage($"Твой КДА: {(float)CheckSteamIDForKillInfo[playa.SteamId].GetAmountOfKills() / (float)CheckSteamIDForKillInfo[playa.SteamId].GetAmountOfDeaths()}. ({(float)CheckSteamIDForKillInfo[playa.SteamId].GetAmountOfKills()} / {(float)CheckSteamIDForKillInfo[playa.SteamId].GetAmountOfDeaths()}) ");
				}
				CheckSteamIDForKillInfo[playa.SteamId].KillsThisRoundCounter.Clear();
			}
			KillReadAndWrite.SaveAllPlayers(CheckSteamIDForKillInfo);
			Smod2.PluginManager.Manager.Server.Map.Broadcast(5, "Проверь консоль, чтобы узнать свои убийства, точность и КДА. Консоль открывается на клавишу 'Ё'!", true);
			configOptions.CheckForFalseRoundEnd = false;
		}

		public void OnShoot(PlayerShootEvent ev)
		{
			if (!CheckSteamIDForKillInfo.ContainsKey(ev.Player.SteamId))
			{
				CheckSteamIDForKillInfo[ev.Player.SteamId] = KillReadAndWrite.ReadPlayerBySteamID(ev.Player.SteamId);
			}

			if (ev.Target != null)
			{
				CheckSteamIDForKillInfo[ev.Player.SteamId].AddShot(true);
			}
			else
			{
				CheckSteamIDForKillInfo[ev.Player.SteamId].AddShot(false);
			}
		}

		public void OnCallCommand(PlayerCallCommandEvent ev)
		{
			plugin.Info(ev.Command);
			if(ev.Command.ToLower() == "killinfo")
			{
				if (!CheckSteamIDForKillInfo.ContainsKey(ev.Player.SteamId))
				{
					CheckSteamIDForKillInfo[ev.Player.SteamId] = KillReadAndWrite.ReadPlayerBySteamID(ev.Player.SteamId);
				}

				ev.Player.SendConsoleMessage($"Твоя точность: {CheckSteamIDForKillInfo[ev.Player.SteamId].GetShotInfo(2)}%.");

				foreach (DamageType dmgtype in (DamageType[])Enum.GetValues(typeof(DamageType)))
				{
					if (CheckSteamIDForKillInfo[ev.Player.SteamId].GetKillByDamageType(dmgtype) != 0)
					{
						ev.Player.SendConsoleMessage("Ты убил " + CheckSteamIDForKillInfo[ev.Player.SteamId].GetKillByDamageType(dmgtype) + " человек с помощью " + dmgtype.ToString().Replace("_", "-") + ".");
					}
				}

				if (CheckSteamIDForKillInfo[ev.Player.SteamId].GetAmountOfDeaths() == 0)
				{
					ev.Player.SendConsoleMessage("Твой КДА " + CheckSteamIDForKillInfo[ev.Player.SteamId].GetAmountOfKills() + ". ");
				}
				else
				{
					ev.Player.SendConsoleMessage($"Твой КДА {(float)CheckSteamIDForKillInfo[ev.Player.SteamId].GetAmountOfKills() / (float)CheckSteamIDForKillInfo[ev.Player.SteamId].GetAmountOfDeaths()}.");
				}
			}
		}

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			if (plugin.GetConfigBool("ki_disable"))
			{
				plugin.pluginManager.DisablePlugin(plugin);
			}
			configOptions.SetUp(plugin);
			KillReadAndWrite.SetUp(configOptions);
			CheckSteamIDForKillInfo = KillReadAndWrite.ReadAllPlayers(CheckSteamIDForKillInfo);

			if (KillReadAndWrite.MakeSureDirExistAndGetDir().Length == 0)
			{
				plugin.Error("ki_playerinfodir is not set correctly. Nothing is going to be saved.");
			}
		}
	}
}