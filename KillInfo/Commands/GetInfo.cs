using Smod2;
using Smod2.API;
using Smod2.Commands;

using KillInfo.Managers;

using System;
using System.Collections.Generic;
using System.IO;

namespace KillInfo.Commands
{
	class KillInfo_GetInfo : ICommandHandler
	{
		private Plugin plugin;
		KillInfoEventLogic eventLogic;
		public KillInfo_GetInfo(Plugin plugin,KillInfoEventLogic killInfoEvent)
		{
			this.plugin = plugin;
			eventLogic = killInfoEvent;
		}

		public string GetCommandDescription()
		{
			return "Gets information about player.";
		}

		public string GetUsage()
		{
			return "ki_getinfo SteamID64";
		}
		
		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if(!File.Exists(eventLogic.KillReadAndWrite.MakeSureDirExistAndGetDir() + args[0] + ".txt"))
			{
				return new string[] {"Player was not found."};
			}

			PlayerInfo playerInfo = eventLogic.KillReadAndWrite.ReadPlayerBySteamID(args[0]);
			List<string> StringToReturn = new List<string>()
			{
				""
			};
			
			StringToReturn.Add($"Their accuracy is {playerInfo.GetShotInfo(2)}% ({playerInfo.GetShotInfo(1)}/{playerInfo.GetShotInfo(0)}).");
			
			foreach (DamageType dmgtype in (DamageType[])Enum.GetValues(typeof(DamageType)))
			{
				if (playerInfo.GetKillByDamageType(dmgtype) != 0)
				{
					StringToReturn.Add("They have gotten " + playerInfo.GetKillByDamageType(dmgtype) + $"({playerInfo.GetCurrentKillsByDamageType(dmgtype)} this round) kill(s) with " + dmgtype.ToString().Replace("_", "-") + ".");
				}
			}

			if (playerInfo.GetAmountOfDeaths() == 0)
			{
				StringToReturn.Add("Their KDR is " + playerInfo.GetAmountOfKills() + ". ");
			}
			else
			{
				StringToReturn.Add($"Their KDR is {(float)playerInfo.GetAmountOfKills() / (float)playerInfo.GetAmountOfDeaths()} ({playerInfo.GetAmountOfKills()} / {playerInfo.GetAmountOfDeaths()}).");
			}
			
			return StringToReturn.ToArray();
		}
	}
}
