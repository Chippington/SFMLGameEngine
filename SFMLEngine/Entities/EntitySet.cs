using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities {
	public delegate void EntitySetEvent(EntitySetEventArgs args);
	public class EntitySetEventArgs {
		public Entity entity;
	}
	public class EntitySet {
		private int _id = 0;
		private int id {
			get { return ++_id; }
		}

		public EntitySetEvent OnEntityCreated;
		public EntitySetEvent OnEntityDestroyed;
		public Dictionary<int, Entity> entityMap;
		public List<Entity> entityList;

		public EntitySet() {
			entityMap = new Dictionary<int, Entity>();
			entityList = new List<Entity>();
		}

		public void updateEntities() {
			for(int i = 0; i < entityList.Count; i++) {
				entityList[i]._update();

			}
		}

		public T instantiate<T>() where T : Entity {
			T ent = (T)Activator.CreateInstance(typeof(T));
			ent.OnDestroyEvent += onEntityDestroyed;
			ent._initialize(this);

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
