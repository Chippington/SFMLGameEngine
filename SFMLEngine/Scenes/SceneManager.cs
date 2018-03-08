using SFMLEngine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Scenes {
	public delegate void SceneManagerEvent(SceneManagerEventArgs args);
	public class SceneManagerEventArgs {
		public IScene scene;
		public bool active;
	}

	public class SceneManager : ObjectBase, ISceneManager {
		public SceneManagerEvent _onSceneReset;
		public SceneManagerEvent _onSceneActivated;
		public SceneManagerEvent _onSceneDeactivated;
		public SceneManagerEvent _onSceneRegistered;

		public SceneManagerEvent OnSceneReset { get => _onSceneReset; set => _onSceneReset = value; }
		public SceneManagerEvent OnSceneActivated { get => _onSceneActivated; set => _onSceneActivated = value; }
		public SceneManagerEvent OnSceneDeactivated { get => _onSceneDeactivated; set => _onSceneDeactivated = value; }
		public SceneManagerEvent OnSceneRegistered { get => _onSceneRegistered; set => _onSceneRegistered = value; }

		public SceneEvent OnEntityCreated;
		public SceneEvent OnEntityDestroyed;
		public SceneEvent OnEntityComponentAdded;
		public SceneEvent OnEntityComponentRemoved;

		private Dictionary<Type, IScene> sceneMap;
		private List<IScene> sceneList;
		private IScene activeScene;

		protected GameContext context;

		public virtual void onInitialize(GameContext context) {
			this.sceneMap = new Dictionary<Type, IScene>();
			this.sceneList = new List<IScene>();
			this.context = context;
		}

		public virtual void setActiveScene(IScene scene) {
			if (activeScene != null) {
				OnSceneDeactivated?.Invoke(new SceneManagerEventArgs() {
					scene = activeScene,
				});

				activeScene.onDispose(context);
			}

			activeScene = scene;
			activeScene.onInitialize(context);
			OnSceneActivated?.Invoke(new SceneManagerEventArgs() {
				scene = activeScene,
			});
		}

		public virtual void onDispose(GameContext context) {
			if (activeScene != null)
				activeScene.onDispose(context);

			activeScene = null;
		}

		public virtual void onUpdate(GameContext context) {
			if (activeScene == null)
				return;

			activeScene.onUpdate(context);
		}

		public virtual void onDraw(GameContext context) {
			if (activeScene == null)
				return;

			activeScene.onDraw(context);
		}


		public void registerScene<T>() where T : IScene {
			if (sceneMap.ContainsKey(typeof(T)))
				throw new Exception("Scene already exists in the registry.");

			var inst = (T)Activator.CreateInstance<T>();
			registerScene<T>(inst);
		}

		public void registerScene<T>(T inst) where T : IScene {
			if (sceneMap.ContainsKey(typeof(T)))
				throw new Exception("Scene already exists in the registry.");

			sceneMap.Add(typeof(T), inst);
			sceneList.Add(inst);

			inst.onSceneRegistered(this);
			OnSceneRegistered?.Invoke(new SceneManagerEventArgs() {
				scene = inst,
				active = false
			});
		}

		public void resetScene<T>() where T : IScene {
			var scene = (IScene)getScene<T>();
			scene.onSceneReset();
			OnSceneReset?.Invoke(new SceneManagerEventArgs() {
				scene = scene,
				active = (scene == activeScene)
			});
		}

		public void setActiveScene<T>() where T : IScene {
			if (sceneMap.ContainsKey(typeof(T)) == false)
				throw new Exception("Scene does not exist in the registry.");

			var nextScene = (IScene)getScene<T>();
			if (activeScene == nextScene)
				return;

			if (activeScene != null) {
				activeScene.onSceneDeactivated();
				OnSceneDeactivated?.Invoke(new SceneManagerEventArgs() {
					scene = activeScene,
					active = false
				});
			}

			activeScene = nextScene;
			activeScene.onSceneActivated();
			OnSceneActivated?.Invoke(new SceneManagerEventArgs() {
				scene = activeScene,
				active = true
			});
		}

		public T getScene<T>() where T : IScene {
			if (sceneMap.ContainsKey(typeof(T)) == false)
				throw new Exception("Scene does not exist in the registry.");

			return (T)sceneMap[typeof(T)];
		}

		public IEnumerable<IScene> getScenes() {
			return sceneList;
		}

		public IScene getActiveScene() {
			return activeScene;
		}
	}
}
