using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities.Components {
	public interface IComponent : IGameObject, IUpdatable, IRenderable{
		void setEntity(IEntity owner);
		IEntity getEntity();
	}
}
