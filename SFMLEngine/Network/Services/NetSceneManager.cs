using NetUtils.Net.Interfaces;
using SFMLEngine.Network.Services;
using SFMLEngine.Scenes;
using SFMLEngine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Network {
	public class NetSceneManager : SceneManager, IGameService {
		private INetServiceBase netService;
		private Dictionary<byte, INetScene> idSceneMap = new Dictionary<byte, INetScene>();
		private Dictionary<INetScene, byte> sceneIDMap = new Dictionary<INetScene, byte>();
		private Dictionary<Type, byte> typeIDMap = new Dictionary<Type, byte>();
		private Dictionary<byte, Type> idTypeMap = new Dictionary<byte, Type>();
		protected List<INetScene> sceneList = new List<INetScene>();

		public NetSceneManager() : base() {
			OnSceneRegistered += onSceneRegistered;
		}

		public INetScene sceneFromID(byte id) {
			if (idSceneMap.ContainsKey(id) == false)
				return default(INetScene);

			return idSceneMap[id];
		}

		public byte? idFromScene(INetScene scene) {
			if (sceneIDMap.ContainsKey(scene) == false)
				return null;

			return sceneIDMap[scene];
		}

		public byte? idFromType<T>() where T : INetScene {
			if (typeIDMap.ContainsKey(typeof(T)) == false)
				return null;

			return typeIDMap[typeof(T)];
		}

		public Type typeFromID<T>(byte id) where T : INetScene {
			if (idTypeMap.ContainsKey(id) == false)
				return null;

			return idTypeMap[id];
		}

		private void buildMaps() {
			idSceneMap.Clear();
			sceneIDMap.Clear();
			idTypeMap.Clear();
			typeIDMap.Clear();

			var orderedSceneList = sceneList
				.OrderBy(i => i.GetType().Name)
				.ToList();

			for (byte i = 0; i < orderedSceneList.Count; i++) {
				var scene = orderedSceneList[i];
				idSceneMap.Add((byte)(i + 1), scene);
				sceneIDMap.Add(scene, (byte)(i + 1));
				typeIDMap.Add(scene.GetType(), (byte)(i + 1));
				idTypeMap.Add((byte)(i + 1), scene.GetType());
			}
		}

		protected virtual void onSceneRegistered(SceneManagerEventArgs args) {
			var netScene = args.scene as INetScene;
			if (netScene == null)
				return;

			sceneList.Add(netScene);
			buildMaps();
		}

		public List<INetScene> getNetScenes() {
			return sceneList;
		}

		public void setNetService(INetServiceBase svc) {
			this.netService = svc;
		}
	}
}
