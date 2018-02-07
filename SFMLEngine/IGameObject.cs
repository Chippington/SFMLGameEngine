using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine {
	public interface IGameObject {
		void onInitialize(GameContext context);
		void onDispose(GameContext context);
	}
}
