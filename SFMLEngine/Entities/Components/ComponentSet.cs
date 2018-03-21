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

	public class ComponentSet : IGameObject, IRenderable, IUpdatable {
		public ComponentEvent OnComponentAdded;
		public ComponentEvent OnComponentRemoved;
		private Dictionary<Type, IComponent> source;
		private List<IComponent> componentList;
		protected GameContext context;
		private bool initialized;
		private IEntity owner;

		public ICollection<Type> Keys => source.Keys;

		public ICollection<IComponent> Values => source.Values;

		public int Count => source.Count;

		public ComponentSet(IEntity owner) {
			componentList = new List<IComponent>();

			this.source = new Dictionary<Type, IComponent>();
			this.owner = owner;
		}

		public void onInitialize(GameContext context) {
			this.context = context;
		}

		public void onUpdate(GameContext context) {
			for (int i = 0; i < componentList.Count; i++)
				componentList[i].onUpdate(context);
		}

		public void onDraw(GameContext context) {
			for (int i = 0; i < componentList.Count; i++)
				componentList[i].onDraw(context);
		}


		public T Add<T>() where T : IComponent {
			return (T)Add(typeof(T));
		}

		public bool Remove<T>() where T : IComponent {
			return Remove(typeof(T));
		}

		private HashSet<Type> typeCheck = new HashSet<Type>();
		public IComponent Add(Type key) {
			if (source.ContainsKey(key))
				return source[key];

			var constructors = key.GetConstructors();

			var parameters = constructors.FirstOrDefault().GetParameters()
				.Where(i => i.ParameterType.GetInterfaces().Contains(typeof(IComponent)))
				.ToList();

			object[] args = new object[parameters.Count];
			if (args.Length > 0) {
				for (int i = 0; i < parameters.Count; i++) {
					var p = parameters[i];
					var req = p.ParameterType;
					var reqComp = Get(req);

					if (reqComp == null) {
						if (typeCheck.Contains(req))
							throw new Exception("Cannot have cyclical dependencies in components. ("+req.FullName+")");

						typeCheck.Add(req);
						reqComp = Add(req);
						typeCheck.Remove(req);
					}

					args[i] = reqComp;
				}
			}

			var inst = (IComponent)Activator.CreateInstance(key, args);
			source.Add(key, inst);
			componentList.Add(inst);
			inst.setEntity(owner);
			inst.onInitialize(context);

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
				inst.onDispose(context);
				OnComponentRemoved?.Invoke(new ComponentEventArgs() {
					component = inst,
				});

				componentList.Remove(inst);
			}

			return source.Remove(key);
		}

		public bool TryGetValue(Type key, out IComponent value) => source.TryGetValue(key, out value);

		public IEnumerator<KeyValuePair<Type, IComponent>> GetEnumerator() => source.GetEnumerator();

		public void onDispose(GameContext context) {
			throw new NotImplementedException();
		}
	}
}
