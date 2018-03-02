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
		private static int nextId = 0;

		private int _id = -1;
		private int id {
			get { if (_id == -1) _id = nextId++; return _id; }
		}
		public void log(string str) {
			var name = getName();
			string tag = string.Format(" [{0}]", id);
			ConsoleColor nameColor = ConsoleColor.White;

			var sc = this as Scene;
			if (sc != null) {
				//name = name + " [" + sc.getSceneID() + "]";
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
				while(str.Length > Console.WindowWidth - div - 5) {
					int ind = Math.Min(str.Length, Console.WindowWidth - div - 5);
					while (ind > 1 && breakChars.Contains(str[ind - 1]) == false)
						ind--;

					var substr = str.Substring(0, ind);
					if(string.IsNullOrWhiteSpace(substr) == false)
						outputList.Enqueue(str.Substring(0, ind));

					str = str.Substring(ind);
				}

				for (int i = 0; i < name.Length; i++)
					tmp += " ";

				if (str.Length > 0 && string.IsNullOrEmpty(str) == false)
					outputList.Enqueue(str);

				bool first = true;
				while(outputList.Count > 0) {
					string line = outputList.Dequeue();

					if(!first) {
						Console.Write(tmp + " │ ");
					}

					Console.WriteLine(line.Trim());
					first = false;
				}
			}
		}

		public void log2(string str) {
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
					for (int i = 0; i < name.Length + 2; i++)
						str2 += " ";

					Console.Write(str2);
					output += str2;
				} else {
					if (lastLogger != null)
						Console.WriteLine();

					Console.Write(name + ": ");
					output += name + ": ";
				}

				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine(str);
				output += str;
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

		protected virtual string getName() {
			return GetType().Name;
		}
	}
}
