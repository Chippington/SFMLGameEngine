﻿using SFMLEngine.Scenes;
using SFMLEngine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Network.Services {
	public class NetServiceBase : ObjectBase, IGameService {
		private Dictionary<byte, INetScene> idSceneMap = new Dictionary<byte, INetScene>();
		private Dictionary<INetScene, byte> sceneIDMap = new Dictionary<INetScene, byte>();
		private Dictionary<Type, byte> typeIDMap = new Dictionary<Type, byte>();
		private Dictionary<byte, Type> idTypeMap = new Dictionary<byte, Type>();
		private List<INetScene> sceneList = new List<INetScene>();

		protected INetScene sceneFromID(byte id) {
			if (idSceneMap.ContainsKey(id) == false)
				return default(INetScene);

			return idSceneMap[id];
		}

		protected byte? idFromScene(INetScene scene) {
			if (sceneIDMap.ContainsKey(scene) == false)
				return null;

			return sceneIDMap[scene];
		}

		protected byte? idFromType<T>() where T : INetScene {
			if (typeIDMap.ContainsKey(typeof(T)) == false)
				return null;

			return typeIDMap[typeof(T)];
		}

		protected Type typeFromID<T>(byte id) where T : INetScene {
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

			for(byte i = 0; i < orderedSceneList.Count; i++) {
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

		protected virtual void onSceneDeactivated(SceneManagerEventArgs args) {

		}

		protected virtual void onSceneActivated(SceneManagerEventArgs args) {

		}

		protected virtual void onSceneReset(SceneManagerEventArgs args) {

		}

		public virtual void onUpdate(GameContext context) {

		}

		public virtual void onDraw(GameContext context) {

		}

		public virtual void onInitialize(GameContext context) {

		}

		public virtual void onDispose(GameContext context) {

		}
	}
}