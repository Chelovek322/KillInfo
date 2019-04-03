using Smod2;
using Smod2.Commands;

namespace KillInfo.Commands
{
	class KillInfo_Disable : ICommandHandler
	{
		private Plugin plugin;
		public KillInfo_Disable(Plugin plugin)
		{
			this.plugin = plugin;
		}

		public string GetCommandDescription()
		{
			return "Disables this entire plugin.";
		}

		public string GetUsage()
		{
			return "ki_disable";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			plugin.pluginManager.DisablePlugin(plugin);
			return new string[] { "Disabled " + plugin.Details.name };
		}
	}
}
