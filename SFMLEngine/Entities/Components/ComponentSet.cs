using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities.Components {
	public delegate void ComponentEvent(ComponentEventArgs args);
	public class ComponentEventArgs {
		public IComponent component;
	}

	public class ComponentSet {
		public ComponentEvent OnComponentAdded;
		public ComponentEvent OnComponentRemoved;
		private Dictionary<Type, IComponent> source;
		private IEntity owner;

		public ICollection<Type> Keys => source.Keys;

		public ICollection<IComponent> Values => source.Values;

		public int Count => source.Count;

		//public IComponent this[Type key] { get => source[key]; set => source[key] = value; }

		public ComponentSet(IEntity owner) {
			this.source = new Dictionary<Type, IComponent>();
			this.owner = owner;
		}

		public void updateComponents(GameContext context) {
			foreach (var c in Values)
				c.onUpdate(context);
		}

		public void drawComponents(GameContext context) {
			foreach (var c in Values)
				c.onDraw(context);
		}


		public T Add<T>() where T : IComponent {
			return (T)Add(typeof(T));
		}

		public bool Remove<T>() where T : IComponent {
			return Remove(typeof(T));
		}

		public IComponent Add(Type key) {
			if (source.ContainsKey(key))
				return source[key];

			var inst = (IComponent)Activator.CreateInstance(key);
			source.Add(key, inst);
			inst.setEntity(owner);
			inst.onInitialize();

			OnComponentAdded?.Invoke(new ComponentEventArgs() {
				component = inst,
			});

			return inst;
		}

		public T Get<T>() where T : IComponent {
			return (T)Get(typeof(T));
		}

		public IComponent Get(Type key) {
			if (source.ContainsKey(key) == false)
				return null;

			return source[key];
		}

		public bool Remove(Type key) {
			if (source.ContainsKey(key)) {
				var inst = source[key];
				inst.onDestroy();
				OnComponentRemoved?.Invoke(new ComponentEventArgs() {
					component = inst,
				});
			}

			return source.Remove(key);
		}

		public bool TryGetValue(Type key, out IComponent value) => source.TryGetValue(key, out value);

		public IEnumerator<KeyValuePair<Type, IComponent>> GetEnumerator() => source.GetEnumerator();
	}
}
