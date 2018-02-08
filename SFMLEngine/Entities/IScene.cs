using SFMLEngine.Entities.Components.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities {
	public interface IScene : IGameObject, IRenderable, IUpdatable {
		SceneEvent OnEntityCreated { get; set; }
		SceneEvent OnEntityDestroyed { get; set; }
		SceneEvent OnEntityComponentAdded { get; set; }
		SceneEvent OnEntityComponentRemoved { get; set; }

		void onSceneAdded(SceneManager manager);

		void onSceneRemoved(SceneManager manager);

		T instantiate<T>(params object[] args) where T : Entity;

		void setCamera(CameraComponent camera);

		CameraComponent getCamera();
	}
}
