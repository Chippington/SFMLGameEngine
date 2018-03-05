using NetUtils.Utilities.Logging;
using SFMLEngine.Debug.Loggers;
using SFMLEngine.Entities;
using SFMLEngine.Entities.Collision;
using SFMLEngine.Graphics.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace SFMLEngine.Network {
	public class NetDebugLogger : IDebugLogger {
		private ConsoleLogger logger;

		public NetDebugLogger() {
			logger = new ConsoleLogger();
		}

		public void Clear() {
		}

		public void Log(string str) {
			_log(str);
		}

		public void Log(string str, params object[] args) {
			_log(string.Format(str, args));
		}

		public void SaveToFile(string path) {
		}

		private void _log(string str) {
			string name = "(external)";
			Color nameColor = Color.White;
			var split = str.Split(new string[] { "->" }, StringSplitOptions.None);
			if (split.Length > 0) {
				name = "";
				for (int i = 0; i < split.Length - 1; i++)
					name += split[i].Trim() + ".";

				name = name.Substring(0, name.Length - 1);
				str = split[split.Length - 1];
			}

			logger.log(str, name);
		}
	}
}
