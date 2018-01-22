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
		public Entity entity;
		public Component component;
	}

	public class Entity {
		private Dictionary<Type, Component> componentMap;
		public EntityEvent OnComponentAdded;
		public EntityEvent OnComponentRemoved;
		public EntityEvent OnDestroyEvent;
		public EntityEvent OnUpdateEvent;
		public EntityEvent OnDrawEvent;
		public Vector2f position;
		public float rotation;

		protected EntitySet owner;
		private bool destroyed;

		public Entity() {
			componentMap = new Dictionary<Type, Component>();
			destroyed = false;

			OnComponentAdded = new EntityEvent((a) => { });
		}

		internal void _initialize(EntitySet initializer) {
			owner = initializer;
			initialize();
		}

		internal void _update() {
			OnUpdateEvent?.Invoke(new EntityEventArgs());
			update();
		}

		internal void _draw() {
			OnDrawEvent?.Invoke(new EntityEventArgs());
			draw();
		}

		public virtual void initialize() { }

		public virtual void update() { }

		public virtual void draw() { }

		public void destroy() {
			destroyed = true;
		}

		public bool isDestroyed() {
			return destroyed;
		}

		public T addComponent<T>() where T : Component {
			if (componentMap.ContainsKey(typeof(T)))
				throw new ArgumentException("Component already exists");

			var component = (Component)Activator.CreateInstance(typeof(T));
			componentMap.Add(typeof(T), component);
			component.entity = this;
			component.onInitialize();

			OnComponentAdded?.Invoke(new EntityEventArgs() {
				component = component,
				entity = this,
			});

			return (T)component;
		}

		public void removeComponent<T>() where T : Component {
			if (componentMap.ContainsKey(typeof(T)) == false)
				return;

			var comp = componentMap[typeof(T)];
			comp.onDestroy();

			componentMap.Remove((typeof(T)));
			OnComponentRemoved?.Invoke(new EntityEventArgs() {
				component = comp,
				entity = this,
			});
		}

		public T getComponent<T>() where T : Component {
			if (componentMap.ContainsKey(typeof(T)))
				return (T)componentMap[typeof(T)];

			return null;
		}

		public List<Component> getComponents() {
			return componentMap.Values.ToList();
		}
	}
}
