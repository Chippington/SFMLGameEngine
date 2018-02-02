using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities.Collision {
	public interface ICollisionMap {
		void updateCollisionMap(GameContext context);

		void drawCollisionMap(GameContext context);

		void onEntityCreated(SceneEventArgs args);

		void onEntityDestroyed(SceneEventArgs args);

		void onEntityComponentCreated(SceneEventArgs args);

		void onEntityComponentDestroyed(SceneEventArgs args);
	}
}
