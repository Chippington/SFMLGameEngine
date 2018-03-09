using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace SFMLEngine.Debug.Loggers {
	public class ConsoleLogger : ILogger {
		private object last = null;
		private static string divider = " │ ";
		private bool altLine = false;
		private int nameDiv = 40;
		private int tagDiv = 8;
		private static List<int> lockList = new List<int>();

		public void log(string message) {
			log(message, this);
		}

		public void log(string message, string sender) {
			lock (lockList) {
				string name = sender;
				log("[ext]", name, message, sender);
			}
		}

		public void log(string message, object sender) {
				if (sender == null)
					sender = this;

				string id = "";
				var gameObject = sender as ObjectBase;
				if (gameObject != null)
					id = string.Format("[{0}]", gameObject.id.ToString());

				string name = sender.GetType().FullName;
				log(id, name, message, sender);
		}

		private void log(string tag, string name, string msg, object sender) {
			lock (lockList) {
				if (sender.Equals(last)) {
					name = "";
				} else {
					//for (int i = 0; i < 5; i++) {
					//	writeTag("");
					//	writeName("");
					//	Console.Write(divider, Color.White);
					//	writeMessage("");
					//}
					altLine = !altLine;
					last = sender;
				}

				writeTag(tag);
				writeName(name);
				Console.Write(divider, Color.White);
				writeMessage(msg);
			}
		}

		private void writeTag(string tag) {
			if (altLine) Console.BackgroundColor = Color.FromArgb(255, 0, 0, 0);
			if (!altLine) Console.BackgroundColor = Color.FromArgb(255, 0, 0, 0);

			string pad = "";
			for (int i = 0; i < tagDiv - tag.Length; i++)
				pad += " ";

			Color col = Color.WhiteSmoke;
			if (tag.Contains("ext"))
				col = Color.FromArgb(255, 50, 50, 50);

			Console.Write(tag + pad, col);
		}

		private void writeName(string sender) {
			string name = sender.Trim();

			var length = name.Length;
			int diff = nameDiv - length;

			if (diff < 0) {
				name = "..." + name.Substring(length - nameDiv + 3);
			}

			string pad = "";
			if(diff > 0) {
				for (int i = 0; i < diff; i++)
					pad += " ";
			}

			Console.Write(pad);
			if (name == "")
				return;

			var nameColor = Color.GhostWhite;
			var col = Color.FromArgb(255, 50, 50, 50);
			var nameSplit = name.Split('.');
			for (int i = 0; i < nameSplit.Length; i++) {
				if (nameSplit[i] == "") {
					Console.Write('.', col);
					continue;
				}

				var innerSplit = nameSplit[i].Split('+');
				for (int j = 0; j < innerSplit.Length; j++) {
					if (i == nameSplit.Length - 1 && j == innerSplit.Length - 1)
						col = nameColor;

					if (innerSplit[j].Trim() == "")
						continue;

					string o = innerSplit[j].Trim();
					if (i != nameSplit.Length - 1 || j != innerSplit.Length - 1)
						o += ".";

					Console.Write(o, col);
				}
			}
		}
		private void writeMessage(string message) {
			string breakChars = " .,:;-";
			var lineSplit = message.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
			var outputList = new List<string>();

			var maxLen = Console.WindowWidth - (tagDiv + nameDiv) - 3;
			for (int i = 0; i < lineSplit.Length; i++) {
				var line = lineSplit[i];
				while (line.Length > maxLen) {
					int ind = Math.Min(line.Length, Console.WindowWidth - (tagDiv + nameDiv) - 5);
					while (ind > 1 && breakChars.Contains(line[ind - 1]) == false)
						ind--;

					var substr = line.Substring(0, ind);
					if (string.IsNullOrWhiteSpace(substr) == false)
						outputList.Add(line.Substring(0, ind));

					line = line.Substring(ind);
				}

				outputList.Add(line);
			}

			string tmp = "";
			for (int i = 0; i < nameDiv + tagDiv; i++)
				tmp += " ";

			bool first = true;
			for(int i = 0; i < outputList.Count; i++) { 
				string line = outputList[i].Trim();
				for (int j = line.Length; j < maxLen; j++)
					line += " ";

				if (!first) {
					Console.Write(tmp + " │ ", Color.White);
				}

				Console.Write(line, Color.LightGray);
				first = false;
			}
		}
	}
}
