using SFML.Graphics;
using SFMLEngine.Entities.Collision;
using SFMLEngine.Entities.Components;
using SFMLEngine.Entities.Components.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities {
	public delegate void SceneEvent(SceneEventArgs args);
	public class SceneEventArgs {
		public IEntity entity;
		public IComponent component;
	}
	public class Scene : ObjectBase, IScene, IGameObject, IUpdatable, IRenderable {
		private static int id = 0;

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
		private List<IEntity> entityList;
		private int sceneID;

		protected CameraComponent camera;
		protected GameContext context;

		public Scene() {
			entityMap = new Dictionary<int, IEntity>();
			entityList = new List<IEntity>();
			sceneID = id++;

			log("Creating collision map");
			_collisionMap = new SweepAndPrune();

			OnEntityCreated += _collisionMap.onEntityCreated;
			OnEntityDestroyed += _collisionMap.onEntityDestroyed;
			OnEntityComponentAdded += _collisionMap.onEntityComponentAdded;
			OnEntityComponentRemoved += _collisionMap.onEntityComponentDestroyed;

			log(string.Format("Scene created [ID:{0}]", sceneID));
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
			for (int i = 0; i < entityList.Count; i++) {
				entityList[i].onUpdate(context);
				entityList[i].components.onUpdate(context);
			}

			_collisionMap.onUpdate(context);
		}

		public virtual void onDraw(GameContext context) {
			for (int i = 0; i < entityList.Count; i++) {
				entityList[i].components.onDraw(context);
				entityList[i].onDraw(context);
			}

			if (camera == null)
				return;

			if (camera.getView() == null)
				return;

			_collisionMap.onDraw(context);
			context.window.SetView(camera.getView());
		}

		public virtual void onSceneAdded(SceneManager manager) { }

		public virtual void onSceneRemoved(SceneManager manager) { }

		public virtual T instantiate<T>(params object[] args) where T : Entity {
			T ent = (T)Activator.CreateInstance(typeof(T), args);
			entityList.Add(ent);

			ent.OnDestroyEvent += onEntityDestroyed;
			ent.setOwner(this);
			ent.onInitialize(context);

			OnEntityCreated?.Invoke(new SceneEventArgs() {
				entity = ent,
			});
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

		private void onEntityDestroyed(EntityEventArgs args) {
			log(string.Format("Entity destroyed [Type:{0}]", args.entity.GetType().Name));
			OnEntityDestroyed?.Invoke(new SceneEventArgs() {
				entity = args.entity,
			});
		}
	}
}
