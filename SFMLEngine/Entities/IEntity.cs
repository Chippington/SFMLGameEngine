using SFMLEngine.Entities.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities {
	public interface IEntity : IGameObject, IUpdatable, IRenderable {
		ComponentSet components { get; set; }
		ComponentSet getComponents();
		void setOwner(Scene owner);
	}
}
