using NetUtils.Utilities.Logging;
using SFMLEngine.Entities;
using SFMLEngine.Entities.Collision;
using SFMLEngine.Graphics.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Net {
	public class DebugLogger : IDebugLogger {
		private List<string> logList;

		public DebugLogger() {
			logList = new List<string>();
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
			ConsoleColor nameColor = ConsoleColor.White;
			//string tag = string.Format(" [{0}]", id);
			var name = "(external)";
			var split = str.Split(new string[] { "->" }, StringSplitOptions.None);
			if(split.Length > 0) {
				name = "";
				for (int i = 0; i < split.Length - 1; i++)
					name += split[i].Trim() + ".";

				name = name.Substring(0, name.Length - 1);
				str = split[split.Length - 1];
				nameColor = ConsoleColor.DarkGray;
			}

			string tag = "";

			string newlineGap = "";
			for (int i = 0; i < Console.WindowWidth; i++)
				newlineGap += " ";

			str = str.Replace("\r\n", newlineGap);

			lock (logList) {
				int div = 25;
				if (name.Length > div - tag.Length - 5)
					name = name.Substring(0, div - tag.Length - 5) + "(...)";

				name = name + tag;

				int diff = div - name.Length;

				string tmp = " ";
				for (int i = 0; i < diff; i++)
					tmp += " ";

				Console.Write(tmp);
				Console.ForegroundColor = nameColor;
				Console.Write(name);
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write(" │ ");

				string breakChars = " .,:;-";
				Queue<string> outputList = new Queue<string>();
				while (str.Length > Console.WindowWidth - div - 5) {
					int ind = Math.Min(str.Length, Console.WindowWidth - div - 5);
					while (ind > 1 && breakChars.Contains(str[ind - 1]) == false)
						ind--;

					outputList.Enqueue(str.Substring(0, ind));
					str = str.Substring(ind);
				}

				for (int i = 0; i < name.Length; i++)
					tmp += " ";

				if (str.Length > 0)
					outputList.Enqueue(str);

				bool first = true;
				while (outputList.Count > 0) {
					string line = outputList.Dequeue();

					if (!first) {
						Console.Write(tmp + " │ ");
					}

					Console.WriteLine(line.Trim());
					first = false;
				}
			}
		}
	}
}
