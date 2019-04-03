using System;
using System.IO;
using Smod2.API;
using KillInfo.Managers;
using System.Collections.Generic;

/*
 * The idea here is to have the damage type and then the amount of kills with it for example 15:14,14:5 would be logicer with 15 kills and mp4 with 5 kills.
 * The next line is Amount of bullets hit and amount of bullets shot then total amount of deaths. 255:1024:2048 would be 255 hits and 1024 shots overall, the next number it just global deaths this example would be 2048 deaths. 
 * This is saved into a file with its name the steamid of the player in the default directory of %appdata%\SCP Secret Laboratory\KillInfo\PlayerInfo. (Sorry linux users)
*/

namespace KillInfo
{
	class KillFileReadAndWrite
	{
		public ConfigOptions configOptions = new ConfigOptions();

		public void SetUp(ConfigOptions config)
		{
			configOptions = config;
		}

		/// <summary>
		/// Goes through every player in the server and reads their stats from their file.
		/// </summary>
		public Dictionary<string,PlayerInfo> ReadAllPlayers(Dictionary<string,PlayerInfo> CheckSteamIDForKillInfo)
		{
			string dir = MakeSureDirExistAndGetDir();

			if (dir.Length == 0)
			{
				return null;
			}

			foreach (Player playa in Smod2.PluginManager.Manager.Server.GetPlayers())
			{
				if (File.Exists(dir + playa.SteamId + ".txt"))
				{
					if (!CheckSteamIDForKillInfo.ContainsKey(playa.SteamId))
					{
						CheckSteamIDForKillInfo[playa.SteamId] = new PlayerInfo();
					}
					string[] splitbyNewLine = File.ReadAllText(dir + playa.SteamId+".txt").Split('\n');
					string[] splitByDamageTypesKills = splitbyNewLine[0].Split(',');
					foreach (string str in splitByDamageTypesKills)
					{
						string[] damageTypeAndKills = str.Split(':');
						if (Int32.TryParse(damageTypeAndKills[0], out int value) && Int32.TryParse(damageTypeAndKills[1], out int value2))
						{
							CheckSteamIDForKillInfo[playa.SteamId].SetKill((DamageType)value, value2);
						}
					}
					string[] shotsHitsandDeaths = splitbyNewLine[1].Split(':');
					if (Int32.TryParse(shotsHitsandDeaths[0], out int value3) && Int32.TryParse(shotsHitsandDeaths[1], out int value4) && Int32.TryParse(shotsHitsandDeaths[2], out int value5))
					{
						CheckSteamIDForKillInfo[playa.SteamId].ShotsFired = value4;
						CheckSteamIDForKillInfo[playa.SteamId].ShotsHit = value3;
						CheckSteamIDForKillInfo[playa.SteamId].SetDeath(DamageType.NONE, value5);
					}
				}
			}
			return CheckSteamIDForKillInfo;
		}

		/// <summary>
		/// Gets one person's stats by steamid on disk.
		/// </summary>
		/// <param name="steamid">User's steamid</param>
		/// <param name="playerinfo">User's information about kills, deaths and shooting.</param>
		public PlayerInfo ReadPlayerBySteamID(string steamid)
		{
			string dir = MakeSureDirExistAndGetDir();

			if (dir.Length == 0)
			{
				return new PlayerInfo();
			}

			PlayerInfo playerinfo = new PlayerInfo();

			if (File.Exists(dir + steamid + ".txt"))
			{
				string fileText = File.ReadAllText(dir + steamid + ".txt");
				string[] splitbyNewLine = fileText.Split('\n');
				string[] splitByDamageTypesKills = splitbyNewLine[0].Split(',');

				foreach (string str in splitByDamageTypesKills)
				{
					string[] damageTypeAndKills = str.Split(':');
					if (Int32.TryParse(damageTypeAndKills[0], out int value) && Int32.TryParse(damageTypeAndKills[1], out int value2))
					{
						playerinfo.SetKill((DamageType)Int32.Parse(damageTypeAndKills[0]), Int32.Parse(damageTypeAndKills[1]));
					}
				}
				string[] shotsHitsandDeaths = splitbyNewLine[1].Split(':');
				if (Int32.TryParse(shotsHitsandDeaths[0], out int value3) && Int32.TryParse(shotsHitsandDeaths[1], out int value4) && Int32.TryParse(shotsHitsandDeaths[2], out int value5))
				{
					playerinfo.ShotsFired = value4;
					playerinfo.ShotsHit = value3;
					playerinfo.SetDeath(DamageType.FALLDOWN, value5);
				}
			}
			
			return playerinfo;
		}
		
		/// <summary>
		/// Goes through and saves each player's stats to their file
		/// </summary>
		public void SaveAllPlayers(Dictionary<string,PlayerInfo> CheckSteamIDForKillInfo)
		{
			string dir = MakeSureDirExistAndGetDir();
			if (dir.Length == 0)
			{
				return;
			}
			foreach (var SteamIDandPlayerInfo in CheckSteamIDForKillInfo)
			{
				using (StreamWriter writeData = new StreamWriter(dir + SteamIDandPlayerInfo.Key +".txt", false))
				{
					string formatedString = "";

					foreach (Smod2.API.DamageType dmgtyp in (Smod2.API.DamageType[])Enum.GetValues(typeof(Smod2.API.DamageType)))
					{
						if(SteamIDandPlayerInfo.Value.GetKillByDamageType(dmgtyp) >= 1)
						{
							formatedString = formatedString + (int)dmgtyp + ":" + SteamIDandPlayerInfo.Value.GetKillByDamageType(dmgtyp) + ",";
						}
					}
					formatedString = formatedString + "\n" + SteamIDandPlayerInfo.Value.ShotsHit + ":" + SteamIDandPlayerInfo.Value.ShotsFired + ":" + SteamIDandPlayerInfo.Value.GetAmountOfDeaths();
					writeData.Write(formatedString);
				}

			}
		}

		/// <summary>
		/// Saves a certain player's stats to their file.
		/// </summary>
		/// <param name="steamid">User's steamid</param>
		/// <param name="playerinfo">User's information about kills,deaths and shooting.</param>
		public void SavePlayerBySteamID(string steamid, PlayerInfo playerinfo)
		{
			string dir = MakeSureDirExistAndGetDir();

			if(dir.Length == 0)
			{
				return;
			}

			if (dir.Length >= 1)
			{
				using (StreamWriter writeData = new StreamWriter(dir + steamid + ".txt", false))
				{
					string formatedString = "";
					if (playerinfo == null)
					{
						playerinfo = new PlayerInfo();
					}

					foreach (Smod2.API.DamageType dmgtyp in (Smod2.API.DamageType[])Enum.GetValues(typeof(Smod2.API.DamageType)))
					{
						if (playerinfo.GetKillByDamageType(dmgtyp) >= 1)
						{
							formatedString = formatedString + (int)dmgtyp + ":" + playerinfo.GetKillByDamageType(dmgtyp) + ",";
						}
					}
					formatedString = formatedString + "\n" + playerinfo.ShotsHit + ":" + playerinfo.ShotsFired + ":" + playerinfo.GetAmountOfDeaths();
					writeData.Write(formatedString);
				}

			}
		}

		/// <summary>
		/// Gets directory where to save all information about players
		/// </summary>
		/// <returns></returns>
		public string MakeSureDirExistAndGetDir()
		{
			if (configOptions.killinfo_dir == "config")
			{
				if (!Directory.Exists(FileManager.GetAppFolder() + "\\KillInfo\\"))
				{
					Directory.CreateDirectory(FileManager.GetAppFolder() + "\\KillInfo\\");
				}
				if (!Directory.Exists(FileManager.GetAppFolder() + "\\KillInfo\\PlayerInfo\\"))
				{
					Directory.CreateDirectory(FileManager.GetAppFolder() + "\\KillInfo\\PlayerInfo\\");
				}
				return FileManager.GetAppFolder() + "\\KillInfo\\PlayerInfo\\";
			}
			else
			{
				if (!Directory.Exists(configOptions.killinfo_dir))
				{
					return "";
				}

				if(configOptions.killinfo_dir[configOptions.killinfo_dir.Length-1] == '\\')
				{
					return configOptions.killinfo_dir;
				}
				return configOptions.killinfo_dir + "\\";
			}
		}
	}
}
