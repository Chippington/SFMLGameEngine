using SFMLEngine.Entities.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities {
	public delegate void EntitySetEvent(EntitySetEventArgs args);
	public class EntitySetEventArgs {
		public IEntity entity;
		public IComponent component;
	}
	public class EntitySet {
		private int _id = 0;
		private int id {
			get { return ++_id; }
		}

		public EntitySetEvent OnEntityCreated;
		public EntitySetEvent OnEntityDestroyed;
		public EntitySetEvent OnEntityComponentAdded;
		public EntitySetEvent OnEntityComponentRemoved;
		public Dictionary<int, IEntity> entityMap;
		public List<IEntity> entityList;

		public EntitySet() {
			entityMap = new Dictionary<int, IEntity>();
			entityList = new List<IEntity>();
		}

		public void updateEntities(GameContext context) {
			for(int i = 0; i < entityList.Count; i++) {
				entityList[i].onUpdate(context);
				entityList[i].components.updateComponents(context);
			}
		}

		public void drawEntities(GameContext context) {
			for (int i = 0; i < entityList.Count; i++) {
				entityList[i].onDraw(context);
				entityList[i].components.drawComponents(context);
			}
		}

		public T instantiate<T>(params object[] args) where T : Entity {
			T ent = (T)Activator.CreateInstance(typeof(T), args);
			entityList.Add(ent);

			ent.OnDestroyEvent += onEntityDestroyed;
			ent.onInitialize();

			OnEntityCreated?.Invoke(new EntitySetEventArgs() {
				entity = ent,
			});
			return ent;
		}

		private void onEntityDestroyed(EntityEventArgs args) {
			OnEntityDestroyed?.Invoke(new EntitySetEventArgs() {
				entity = args.entity,
			});
		}
	}
}
