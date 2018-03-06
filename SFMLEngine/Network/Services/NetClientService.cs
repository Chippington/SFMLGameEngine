using SFMLEngine.Services;
using NetUtils.Net.Default;
using NetUtils.Net.Interfaces;
using SFMLEngine.Network.Providers;

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using NetUtils.Utilities.Logging;
using SFMLEngine.Scenes;

namespace SFMLEngine.Network.Services {
	public delegate void NetClientEvent(NetClientEventArgs args);
	public class NetClientEventArgs {

	}

	public class NetClientService : NetServiceBase, IGameService {
		private NetSceneManager sceneManager;
		private INetworkProvider provider;
		private NetClient _netClient;
		private NetConfig config;

		public NetClientEvent OnConnectedToServer;
		public NetClientEvent OnDisconnectedFromServer;
		public NetClientEvent OnNetClientStart;

		public NetClient netClient { get => _netClient; set { } }

		public override void onInitialize(GameContext context) {
			DebugLog.setLogger(new NetDebugLogger());

			if (context.services.hasService<NetSceneManager>(true) == false)
				context.services.registerService<NetSceneManager>();

			sceneManager = context.services.getService<NetSceneManager>();
			sceneManager.setNetService(this);
		}

		public virtual void startClient(NetConfig config) {
			startClient(config, new ENetProvider());
		}

		public virtual void startClient(NetConfig config, INetworkProvider provider) {
			log("Starting net client");
			log(string.Format("Config settings: \r\nPort: {0}\r\nIP Address: {1}",
				config.port, config.ipaddress));

			this.provider = provider;
			this.config = config;

			NetServicePackets.registerPackets(config);

			_netClient = new NetClient(provider, config);
			_netClient.addClientPacketCallback<P_SceneChange>(cbOnSceneChange);
			_netClient.addClientPacketCallback<P_SceneReset>(cbOnSceneReset);
			netClient.start();

			netClient.onConnectedToServer += onConnectedToServer;
			log("Net client initialized");

			OnNetClientStart?.Invoke(new NetClientEventArgs());
		}

		private void cbOnSceneReset(P_SceneReset obj) {
			var scene = sceneManager.sceneFromID(obj.id);
			if (scene == null) {
				log(string.Format("Received scene RESET data for non-existant scene [{0}]", obj.id));
				return;
			}

			log(string.Format("Received scene RESET data for {0} [{1}]", scene.GetType().Name, obj.id));
			scene.readFrom(obj.sceneData);
		}

		private void cbOnSceneChange(P_SceneChange obj) {
			var scene = sceneManager.sceneFromID(obj.id);
			if (scene == null) {
				log(string.Format("Received scene CHANGE data for non-existant scene [{0}]", obj.id));
				return;
			}

			log(string.Format("Received scene CHANGE data for {0} [{1}]", scene.GetType().Name, obj.id));
			scene.readFrom(obj.sceneData);
		}

		protected virtual void onConnectedToServer(NetEventArgs args) {
			log("Connected to server");
			OnConnectedToServer?.Invoke(new NetClientEventArgs());
		}

		public override void onDraw(GameContext context) {

		}

		public override void onUpdate(GameContext context) {
			if(netClient != null)
				netClient.updateClient();
		}

		public override void onDispose(GameContext context) {
			if(netClient != null)
				netClient.stop();

			netClient = null;
		}

		public override NetworkHandler getNetHandler() {
			return _netClient;
		}
	}
}
