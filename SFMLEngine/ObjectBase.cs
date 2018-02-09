using SFMLEngine.Entities;
using SFMLEngine.Entities.Collision;
using SFMLEngine.Entities.Components;
using SFMLEngine.Graphics.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine {
	public class ObjectBase {
		private static ObjectBase lastLogger = null;
		private static List<string> logList = new List<string>();

		public void log(string str) {
			var name = GetType().Name;
			ConsoleColor nameColor = ConsoleColor.White;

			var sc = this as Scene;
			if (sc != null) {
				name = name + " [" + sc.getSceneID() + "]";
				nameColor = ConsoleColor.Green;
			}

			var cm = this as ICollisionMap;
			if (cm != null)
				nameColor = ConsoleColor.Yellow;

			var gm = this as GameWindow;
			if (gm != null)
				nameColor = ConsoleColor.DarkYellow;

			var ct = this as UIControl;
			if (ct != null)
				nameColor = ConsoleColor.Magenta;

			var en = this as IEntity;
			if (en != null)
				nameColor = ConsoleColor.Red;

			var cp = this as IComponent;
			if (cp != null)
				nameColor = ConsoleColor.DarkRed;

			lock(logList) {
				string output = "";
				Console.ForegroundColor = nameColor;

				if (lastLogger == this) {
					string str2 = "";
					for (int i = 0; i < name.Length; i++)
						str2 += " ";

					Console.Write(str2);
					output += str2;
				} else {
					if (lastLogger != null)
						Console.WriteLine();

					Console.Write(name);
					output += name;
				}

				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine(" -> " + str);
				output += " -> " + str;
				lastLogger = this;
				logList.Add(output);
			}
		}

		public void write(string str) {
			Console.Write(str);
		}

		public void writeLine(string str) {
			Console.WriteLine(str);
		}

		public void writeLine() {
			Console.WriteLine();
		}
	}
}
