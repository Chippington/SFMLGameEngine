using SFMLEngine.Debug.Loggers;
using SFMLEngine.Entities;
using SFMLEngine.Entities.Collision;
using SFMLEngine.Entities.Components;
using SFMLEngine.Graphics.UI;
using SFMLEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace SFMLEngine {
	public class ObjectBase {
		internal static List<string> logList = new List<string>();
		private static int nextId = 0;

		private int _id = -1;

		internal int id {
			get { if (_id == -1) _id = nextId++; return _id; }
		}

		internal static int maxid {
			get { return nextId - 1; }
		}

		private static ConsoleLogger logger = new ConsoleLogger();
		public void log(string str) {
			logger.log(str, this);
		}

		protected virtual string getName() {
			return GetType().FullName;
		}
	}
}
