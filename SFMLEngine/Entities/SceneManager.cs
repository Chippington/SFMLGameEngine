using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities {
	public delegate void SceneManagerEvent(SceneManagerEventArgs args);
	public class SceneManagerEventArgs {
		public IScene scene;
		public bool active;
	}

	public class SceneManager : IGameObject, IUpdatable, IRenderable {
		public SceneManagerEvent onSceneAdded;
		public SceneManagerEvent onSceneRemoved;
		public SceneManagerEvent onSceneActiveChanged;

		public SceneEvent OnEntityCreated;
		public SceneEvent OnEntityDestroyed;
		public SceneEvent OnEntityComponentAdded;
		public SceneEvent OnEntityComponentRemoved;

		private List<IScene> sceneList;
		private List<IScene> activeList;
		protected GameContext context;

		public SceneManager() {
			activeList = new List<IScene>();
			sceneList = new List<IScene>();
		}

		public void onInitialize(GameContext context) {
			this.context = context;
		}

		public bool addScene(IScene scene) {
			if (sceneList.Contains(scene))
				return false;

			sceneList.Add(scene);
			scene.onInitialize(context);
			scene.onSceneAdded(this);
			onSceneAdded?.Invoke(new SceneManagerEventArgs() {
				scene = scene,
				active = false,
			});

			return true;
		}

		public void addScene(IScene scene, bool active) {
			if (addScene(scene)) setSceneActive(scene, active);
		}

		public void setSceneActive(IScene scene, bool active) {
			if (sceneList.Contains(scene) == false)
				return;

			if(active) {
				if (activeList.Contains(scene) == false)
					activeList.Add(scene);

				OnEntityCreated += scene.OnEntityCreated;
				OnEntityDestroyed += scene.OnEntityDestroyed;
				OnEntityComponentAdded += scene.OnEntityComponentAdded;
				OnEntityComponentRemoved += scene.OnEntityComponentRemoved;
			} else {
				if (activeList.Contains(scene))
					activeList.Remove(scene);

				OnEntityCreated -= scene.OnEntityCreated;
				OnEntityDestroyed -= scene.OnEntityDestroyed;
				OnEntityComponentAdded -= scene.OnEntityComponentAdded;
				OnEntityComponentRemoved -= scene.OnEntityComponentRemoved;
			}

			onSceneActiveChanged?.Invoke(new SceneManagerEventArgs() {
				scene = scene,
				active = active
			});
		}

		public void clearActiveScenes() {
			foreach (var sc in activeList)
				onSceneActiveChanged?.Invoke(new SceneManagerEventArgs() {
					scene = sc,
					active = false,
				});

			activeList.Clear();
		}

		public void removeScene(IScene scene) {
			if (sceneList.Contains(scene) == false)
				return;

			sceneList.Remove(scene);
			scene.onSceneRemoved(this);
			onSceneRemoved?.Invoke(new SceneManagerEventArgs() {
				scene = scene,
				active = false,
			});
		}

		public void onDispose(GameContext context) {
			foreach (var sc in sceneList)
				sc.onDispose(context);

			sceneList.Clear();
			sceneList = null;

			activeList.Clear();
			activeList = null;
		}

		public void onUpdate(GameContext context) {
			for (int i = 0; i < activeList.Count; i++)
				activeList[i].onUpdate(context);
		}

		public void onDraw(GameContext context) {
			for (int i = 0; i < activeList.Count; i++)
				activeList[i].onDraw(context);
		}
	}
}
