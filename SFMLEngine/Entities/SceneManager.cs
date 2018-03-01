using SFMLEngine.Services;
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

	public class SceneManager : IGameObject, IUpdatable, IRenderable, IGameService {
		public SceneManagerEvent OnSceneActivated;
		public SceneManagerEvent OnSceneDeactivated;

		public SceneEvent OnEntityCreated;
		public SceneEvent OnEntityDestroyed;
		public SceneEvent OnEntityComponentAdded;
		public SceneEvent OnEntityComponentRemoved;

		protected GameContext context;
		private IScene activeScene;

		public void onInitialize(GameContext context) {
			this.context = context;
		}

		public void setActiveScene(IScene scene) {
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

		public void onDispose(GameContext context) {
			if (activeScene != null)
				activeScene.onDispose(context);

			activeScene = null;
		}

		public void onUpdate(GameContext context) {
			if (activeScene == null)
				return;

			activeScene.onUpdate(context);
		}

		public void onDraw(GameContext context) {
			if (activeScene == null)
				return;

			activeScene.onDraw(context);
		}
	}
}
