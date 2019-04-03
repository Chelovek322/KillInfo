using Smod2;
using Smod2.Commands;

namespace KillInfo.Commands
{
	class KillInfo_Version : ICommandHandler
	{
		private Plugin plugin;
		public KillInfo_Version(Plugin plugin)
		{
			this.plugin = plugin;
		}

		public string GetCommandDescription()
		{
			return "Version for this plugin.";
		}

		public string GetUsage()
		{
			return "ki_version";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			return new string[] { "This is version " + plugin.Details.version };
		}
	}
}
