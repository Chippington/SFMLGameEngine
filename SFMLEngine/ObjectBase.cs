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
		public void log(string str) {
			var name = GetType().Name;
			ConsoleColor nameColor = ConsoleColor.White;

			var sc = this as Scene;
			if(sc != null) {
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

			Console.ForegroundColor = nameColor;
			Console.Write(name);
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(" -> " + str);

		}
	}
}
