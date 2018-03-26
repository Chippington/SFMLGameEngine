using SFML.Graphics;
using SFMLEngine.Entities;
using SFMLEngine.Entities.Collision;
using SFMLEngine.Entities.Components;
using SFMLEngine.Entities.Components.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Scenes {
	public delegate void SceneEvent(SceneEventArgs args);
	public class SceneEventArgs {
		public IEntity entity;
		public IComponent component;
	}
	public class Scene : ObjectBase, IScene, IGameObject, IUpdatable, IRenderable {
		public SceneEvent _OnEntityCreated;
		public SceneEvent _OnEntityDestroyed;
		public SceneEvent _OnEntityComponentAdded;
		public SceneEvent _OnEntityComponentRemoved;

		public SceneEvent OnEntityCreated { get => _OnEntityCreated; set => _OnEntityCreated = value; }
		public SceneEvent OnEntityDestroyed { get => _OnEntityDestroyed; set => _OnEntityDestroyed = value; }
		public SceneEvent OnEntityComponentAdded { get => _OnEntityComponentAdded; set => _OnEntityComponentAdded = value; }
		public SceneEvent OnEntityComponentRemoved { get => _OnEntityComponentRemoved; set => _OnEntityComponentRemoved = value; }

		private ICollisionMap _collisionMap;
		public ICollisionMap collisionMap;

		private Dictionary<int, IEntity> entityMap;

		private Queue<IComponent> componentDrawQueue;
		private Queue<IComponent> componentDrawAdditionQueue;
		private HashSet<IComponent> componentDrawRemovalMap;

		private Queue<IComponent> componentUpdateQueue;
		private Queue<IComponent> componentUpdateAdditionQueue;
		private HashSet<IComponent> componentUpdateRemovalMap;

		private HashSet<IEntity> entityHash;
		private List<IEntity> entityList;
		private int sceneID;

		protected ISceneManager manager;
		protected CameraComponent camera;
		protected GameContext context;

		public Scene() {
			componentDrawAdditionQueue = new Queue<IComponent>();
			componentDrawRemovalMap = new HashSet<IComponent>();
			componentDrawQueue = new Queue<IComponent>();

			componentUpdateAdditionQueue = new Queue<IComponent>();
			componentUpdateRemovalMap = new HashSet<IComponent>();
			componentUpdateQueue = new Queue<IComponent>();

			entityMap = new Dictionary<int, IEntity>();
			entityHash = new HashSet<IEntity>();
			entityList = new List<IEntity>();
			sceneID = this.id;

			log("Creating collision map");
			_collisionMap = new SweepAndPrune();

			OnEntityCreated += _collisionMap.onEntityCreated;
			OnEntityDestroyed += _collisionMap.onEntityDestroyed;
			OnEntityComponentAdded += _collisionMap.onEntityComponentAdded;
			OnEntityComponentRemoved += _collisionMap.onEntityComponentDestroyed;

			OnEntityComponentAdded += onEntityComponentAdded;
			OnEntityComponentRemoved += onEntityComponentRemoved;

			log(string.Format("Scene created [ID:{0}]", sceneID));
		}

		private void onEntityComponentAdded(SceneEventArgs args) {
			lock (componentDrawAdditionQueue)
				componentDrawAdditionQueue.Enqueue(args.component);

			lock (componentUpdateAdditionQueue)
				componentUpdateAdditionQueue.Enqueue(args.component);
		}

		private void onEntityComponentRemoved(SceneEventArgs args) {
			lock (componentDrawRemovalMap)
				componentDrawRemovalMap.Add(args.component);

			lock (componentUpdateRemovalMap)
				componentUpdateRemovalMap.Add(args.component);
		}

		public void onInitialize(GameContext context) {
			this.context = context;
			_collisionMap.onInitialize(context);
		}

		public void onDispose(GameContext context) {
			for (int i = 0; i < entityList.Count; i++)
				entityList[i].onDispose(context);

			_collisionMap.onDispose(context);
			_collisionMap = null;

			entityList.Clear();
			entityList = null;

			entityMap.Clear();
			entityMap = null;
		}

		public virtual void onUpdate(GameContext context) {
			lock (componentUpdateAdditionQueue) lock (componentUpdateQueue)
					while (componentUpdateAdditionQueue.Any())
						componentUpdateQueue.Enqueue(componentUpdateAdditionQueue.Dequeue());

			Queue<IComponent> tmp = new Queue<IComponent>();
			while (componentUpdateQueue.Any()) {
				var c = componentUpdateQueue.Dequeue();
				if (componentUpdateRemovalMap.Contains(c))
					continue;

				c.onUpdate(context);
				tmp.Enqueue(c);
			}

			componentUpdateRemovalMap.Clear();
			componentUpdateAdditionQueue.Clear();
			componentUpdateQueue = tmp;

			Queue<IEntity> destroyQueue = new Queue<IEntity>();
			for (int i = 0; i < entityList.Count; i++) {
				entityList[i].onUpdate(context);

				if(entityList[i].isDestroyed()) {
					destroyQueue.Enqueue(entityList[i]);
				}
			}

			while(destroyQueue.Count > 0) {
				var ent = destroyQueue.Dequeue();
				var args = new EntityEventArgs() {
					entity = ent,
				};

				ent.onDestroy();
				ent.OnDestroyEvent?.Invoke(args);
				entityList.Remove(ent);
				entityHash.Remove(ent);
				onEntityDestroyed(args);
			}

			_collisionMap.onUpdate(context);
		}

		public virtual void onDraw(GameContext context) {
			lock (componentDrawAdditionQueue) lock (componentDrawQueue)
					while (componentDrawAdditionQueue.Any())
						componentDrawQueue.Enqueue(componentDrawAdditionQueue.Dequeue());

			Queue<IComponent> tmp = new Queue<IComponent>();
			while(componentDrawQueue.Any()) {
				var c = componentDrawQueue.Dequeue();
				if (componentDrawRemovalMap.Contains(c))
					continue;

				c.onDraw(context);
				tmp.Enqueue(c);
			}

			componentDrawRemovalMap.Clear();
			componentDrawAdditionQueue.Clear();
			componentDrawQueue = tmp;

			for (int i = 0; i < entityList.Count; i++) {
				//entityList[i].components.onDraw(context);
				entityList[i].onDraw(context);
			}

			if (camera == null)
				return;

			if (camera.getView() == null)
				return;

			_collisionMap.onDraw(context);
			context.window.SetView(camera.getView());
		}

		public virtual T instantiate<T>(params object[] args) where T : IEntity {
			T ent = (T)Activator.CreateInstance(typeof(T), args);
			return (T)instantiate(ent);
		}

		public virtual IEntity instantiate(IEntity ent) {
			entityList.Add(ent);

			ent.setOwner(this);

			OnEntityCreated?.Invoke(new SceneEventArgs() {
				entity = ent,
			});

			ent.components.OnComponentAdded += (i) => OnEntityComponentAdded.Invoke(new SceneEventArgs() {
				entity = ent,
				component = i.component,
			});

			ent.components.OnComponentRemoved += (i) => OnEntityComponentRemoved.Invoke(new SceneEventArgs() {
				entity = ent,
				component = i.component,
			});

			entityHash.Add(ent);
			ent.onInitialize(context);
			return ent;
		}

		public virtual void setCamera(CameraComponent camera) {
			this.camera = camera;
		}

		public virtual CameraComponent getCamera() {
			return camera;
		}

		public virtual int getSceneID() {
			return sceneID;
		}

		protected virtual void onEntityDestroyed(EntityEventArgs args) {
			OnEntityDestroyed?.Invoke(new SceneEventArgs() {
				entity = args.entity,
			});

			entityHash.Remove(args.entity);
		}

		public virtual void onSceneRegistered(SceneManager manager) {
			this.manager = manager;
		}

		public bool containsEntity(IEntity ent) {
			return entityHash.Contains(ent);
		}

		public List<IEntity> getEntityList() {
			return entityList;
		}

		public virtual void onSceneDeactivated() { }

		public virtual void onSceneActivated() { }

		public virtual void onSceneReset() { }
	}
}
