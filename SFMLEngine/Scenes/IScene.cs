using SFMLEngine.Entities;
using SFMLEngine.Entities.Components.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Scenes {
	public interface IScene : IGameObject, IRenderable, IUpdatable {
		SceneEvent OnEntityCreated { get; set; }
		SceneEvent OnEntityDestroyed { get; set; }
		SceneEvent OnEntityComponentAdded { get; set; }
		SceneEvent OnEntityComponentRemoved { get; set; }

		void onSceneRegistered(SceneManager manager);
		void onSceneDeactivated();
		void onSceneActivated();
		void onSceneReset();

		T instantiate<T>(params object[] args) where T : IEntity;

		void setCamera(CameraComponent camera);

		CameraComponent getCamera();
		List<IEntity> getEntityList();
	}
}
