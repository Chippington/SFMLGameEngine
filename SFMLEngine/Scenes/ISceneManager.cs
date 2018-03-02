using SFMLEngine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Scenes {
	public interface ISceneManager : IGameObject, IUpdatable, IRenderable, IGameService {
		SceneManagerEvent OnSceneReset { get; set; }
		SceneManagerEvent OnSceneActivated { get; set; }
		SceneManagerEvent OnSceneDeactivated { get; set; }
		SceneManagerEvent OnSceneRegistered { get; set; }

		void registerScene<T>() where T : IScene;
		void registerScene<T>(T inst) where T : IScene;
		void resetScene<T>() where T : IScene;
		void setActiveScene<T>() where T : IScene;
		T getScene<T>() where T : IScene;
	}
}
