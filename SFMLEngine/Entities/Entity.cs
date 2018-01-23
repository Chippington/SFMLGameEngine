using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFMLEngine.Entities.Components;
using System.Threading.Tasks;
using SFML.System;

namespace SFMLEngine.Entities {
	public delegate void EntityEvent(EntityEventArgs args);
	public class EntityEventArgs {
		public IEntity entity;
		public IComponent component;
	}

	public class Entity : IEntity {
		public EntityEvent OnDestroyEvent;
		public EntityEvent OnUpdateEvent;
		public EntityEvent OnDrawEvent;

		protected EntitySet owner;
		private bool destroyed;

		private ComponentSet _components;
		public ComponentSet components { get => _components; set { } }

		public Entity() {
			_components = new ComponentSet(this);
			destroyed = false;
		}

		public virtual void onInitialize() { }

		public virtual void onUpdate() {
			OnUpdateEvent?.Invoke(new EntityEventArgs() {
				entity = this,
			});
		}

		public virtual void onDraw() {
			OnDrawEvent?.Invoke(new EntityEventArgs() {
				entity = this,
			});
		}

		public virtual void onDestroy() {
			OnDestroyEvent?.Invoke(new EntityEventArgs() {
				entity = this,
			});
		}

		public virtual void setOwner(EntitySet owner) {
			this.owner = owner;
		}

		public void destroy() {
			destroyed = true;
		}

		public bool isDestroyed() {
			return destroyed;
		}

		public ComponentSet getComponents() {
			return _components;
		}
	}
}
