using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFMLEngine.Entities.Components;
using System.Threading.Tasks;
using SFML.System;
using SFMLEngine.Scenes;

namespace SFMLEngine.Entities {
	public delegate void EntityEvent(EntityEventArgs args);
	public class EntityEventArgs {
		public IEntity entity;
		public IComponent component;
	}

	public class Entity : ObjectBase, IEntity {
		public EntityEvent _onDestroyEvent;
		public EntityEvent _onUpdateEvent;
		public EntityEvent _onDrawEvent;

		public EntityEvent OnDestroyEvent { get => _onDestroyEvent; set => _onDestroyEvent = value; }
		public EntityEvent OnUpdateEvent { get => _onUpdateEvent; set => _onUpdateEvent = value; }
		public EntityEvent OnDrawEvent { get => _onDrawEvent; set => _onDrawEvent = value; }

		protected Scene owner;
		private bool destroyed;

		private ComponentSet _components;
		public ComponentSet components { get => _components; set { } }

		public Entity() {
			_components = new ComponentSet(this);
			destroyed = false;
		}

		public virtual void onInitialize(GameContext context) {
			_components.onInitialize(context);
		}

		public virtual void onUpdate(GameContext context) {
			OnUpdateEvent?.Invoke(new EntityEventArgs() {
				entity = this,
			});
		}

		public virtual void onDraw(GameContext context) {
			OnDrawEvent?.Invoke(new EntityEventArgs() {
				entity = this,
			});
		}

		public virtual void onDestroy() {
			OnDestroyEvent?.Invoke(new EntityEventArgs() {
				entity = this,
			});
		}

		public virtual void setOwner(Scene owner) {
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

		public virtual void onDispose(GameContext context) {
			_components.onDispose(context);
		}

		public Scene getOwner() {
			return owner;
		}
	}
}
