using NetUtils.Net.Data;
using NetUtils.Net.Interfaces;
using SFMLEngine.Network.Scenes;
using SFMLEngine.Network.Services;
using SFMLEngine.Scenes;
using SFMLEngine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Network {
	public class NetSceneManager : SceneManager, INetBase, IGameService {
		private Dictionary<byte, INetScene> idSceneMap = new Dictionary<byte, INetScene>();
		private Dictionary<INetScene, byte> sceneIDMap = new Dictionary<INetScene, byte>();
		private Dictionary<Type, byte> typeIDMap = new Dictionary<Type, byte>();
		private Dictionary<byte, Type> idTypeMap = new Dictionary<byte, Type>();
		protected List<INetScene> sceneList = new List<INetScene>();
		private NetworkHandler netHandler;
		private NetServiceBase netService;

		public NetSceneManager() : base() {
			OnSceneRegistered += onSceneRegistered;
		}

		private bool netInit = false;
		public void onNetInitialize(NetServiceBase netService, NetworkHandler netHandler) {
			log("Net-Initializing NetSceneManager");
			this.netService = netService;
			this.netHandler = netHandler;
			netHandler.addPacketCallback<P_ScenePacketContainer>(cbScenePacketContainer);

			netInit = true;
			log("Registering queued NetScenes");
			for (int i = 0; i < sceneList.Count; i++) {
				sceneList[i].onNetInitialize(netService, netHandler);
			}
		}

		private void cbScenePacketContainer(P_ScenePacketContainer obj) {
			var scene = sceneFromID(obj.sceneID);
			if (isServer()) scene.getPacketRouter().onServerReceivePacket(obj.packet);
			if (isClient()) scene.getPacketRouter().onClientReceivePacket(obj.packet);
		}

		public override void onUpdate(GameContext context) {
			base.onUpdate(context);
			var activeScene = getActiveScene() as INetScene;
			if (activeScene == null)
				return;

			Queue<PacketInfo> outgoing = null;
			if (netHandler.isServer()) {
				activeScene.onServerUpdate();
				outgoing = activeScene.getOutgoingServerPackets();
				while(outgoing.Count > 0) {
					var info = outgoing.Dequeue();
					var innerPacket = info.packet;
					info.packet = new P_ScenePacketContainer() {
						packet = innerPacket,
						sceneID = idFromScene(activeScene).Value,
					};

					netHandler.queueServerSendToClients(info);
				}
			}

			if (netHandler.isClient()) {
				activeScene.onClientUpdate();
				outgoing = activeScene.getOutgoingClientPackets();
				while (outgoing.Count > 0) {
					var info = outgoing.Dequeue();
					var innerPacket = info.packet;
					info.packet = new P_ScenePacketContainer() {
						packet = innerPacket,
						sceneID = idFromScene(activeScene).Value,
					};

					netHandler.queueClientSendToServer(info);
				}
			}
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

			log("Registering scene: " + args.scene.GetType().Name);
			sceneList.Add(netScene);
			buildMaps();

			if(netInit)
				netScene.onNetInitialize(netService, netHandler);
		}

		public List<INetScene> getNetScenes() {
			return sceneList;
		}

		public bool isServer() {
			return netHandler.isServer();
		}

		public bool isClient() {
			return netHandler.isClient();
		}
	}
}
