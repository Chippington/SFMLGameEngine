using SFMLEngine.Entities.Components;
using SFMLEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities {
	public interface IEntity : IGameObject, IUpdatable, IRenderable {
		EntityEvent OnDestroyEvent { get; set; }
		EntityEvent OnUpdateEvent { get; set; }
		EntityEvent OnDrawEvent { get; set; }

		ComponentSet components { get; set; }
		ComponentSet getComponents();
		void setOwner(Scene owner);
		Scene getOwner();
		bool isDestroyed();
	}
}
