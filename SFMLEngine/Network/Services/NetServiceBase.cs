using NetUtils.Net.Interfaces;
using SFMLEngine.Scenes;
using SFMLEngine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Network.Services {
	public class NetServiceBase : ObjectBase, IGameService {
		public virtual NetworkHandler getNetHandler() { return null; }

		public virtual void onInitialize(GameContext context) { }
		public virtual void onDispose(GameContext context) { }
		public virtual void onUpdate(GameContext context) { }
		public virtual void onDraw(GameContext context) { }
	}
}
