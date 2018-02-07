using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities.Collision {
	public interface ICollisionMap : IRenderable, IUpdatable {
		void onEntityCreated(SceneEventArgs args);

		void onEntityDestroyed(SceneEventArgs args);

		void onEntityComponentAdded(SceneEventArgs args);

		void onEntityComponentDestroyed(SceneEventArgs args);
	}
}
