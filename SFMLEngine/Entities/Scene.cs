using SFML.Graphics;
using SFMLEngine.Entities.Components;
using SFMLEngine.Entities.Components.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities {
	public delegate void EntitySetEvent(SceneEventArgs args);
	public class SceneEventArgs {
		public IEntity entity;
		public IComponent component;
	}
	public class Scene : ObjectBase {
		private static int id = 0;
		public EntitySetEvent OnEntityCreated;
		public EntitySetEvent OnEntityDestroyed;
		public EntitySetEvent OnEntityComponentAdded;
		public EntitySetEvent OnEntityComponentRemoved;
		public Dictionary<int, IEntity> entityMap;
		public List<IEntity> entityList;
		private CameraComponent camera;
		private int sceneID;
		public Scene() {
			entityMap = new Dictionary<int, IEntity>();
			entityList = new List<IEntity>();
			sceneID = id++;

			log(string.Format("Scene created [ID:{0}]", sceneID));
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

			if (camera == null)
				return;

			if (camera.getView() == null)
				return;

			context.window.SetView(camera.getView());
		}

		public T instantiate<T>(params object[] args) where T : Entity {
			T ent = (T)Activator.CreateInstance(typeof(T), args);
			entityList.Add(ent);

			ent.OnDestroyEvent += onEntityDestroyed;
			ent.setOwner(this);
			ent.onInitialize();

			OnEntityCreated?.Invoke(new SceneEventArgs() {
				entity = ent,
			});
			return ent;
		}

		public void setCamera(CameraComponent camera) {
			this.camera = camera;
		}

		public CameraComponent getCamera() {
			return camera;
		}

		public int getSceneID() {
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
