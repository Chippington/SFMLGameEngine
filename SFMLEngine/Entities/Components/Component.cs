using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities.Components {
	public class Component : IComponent {
		public IEntity entity;

		public virtual void onInitialize(GameContext context) { }
		public virtual void onDispose(GameContext context) { }
		public virtual void onUpdate(GameContext context) { }
		public virtual void onDraw(GameContext context) { }
		public void setEntity(IEntity owner) {
			this.entity = owner;
		}
		public IEntity getEntity() {
			return this.entity;
		}
	}
}
