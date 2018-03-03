using NetUtils.Net;
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

	public class NetClientService : ObjectBase, IGameService {
		private INetworkProvider provider;
		private NetClient _netClient;
		private NetConfig config;

		public NetClientEvent OnConnectedToServer;
		public NetClientEvent OnDisconnectedFromServer;
		public NetClientEvent OnNetClientStart;

		public NetClient netClient { get => _netClient; set { } }

		public virtual void onInitialize(GameContext context) {
			DebugLog.setLogger(new DebugLogger());
			context.services.registerService<NetSceneManager>();
			context.sceneManager.OnSceneReset += onSceneReset;
			context.sceneManager.OnSceneActivated += onSceneActivated;
			context.sceneManager.OnSceneDeactivated += onSceneDeactivated;
			context.sceneManager.OnSceneRegistered += onSceneRegistered;

			var scenes = context.sceneManager.getScenes();
			foreach (var scene in scenes)
				onSceneRegistered(new SceneManagerEventArgs() {
					scene = scene,
				});

			var activeScene = context.sceneManager.getActiveScene();
			onSceneActivated(new SceneManagerEventArgs() {
				scene = activeScene,
			});
		}

		private void onSceneRegistered(SceneManagerEventArgs args) {
			throw new NotImplementedException();
		}

		private void onSceneDeactivated(SceneManagerEventArgs args) {
			throw new NotImplementedException();
		}

		private void onSceneActivated(SceneManagerEventArgs args) {
			throw new NotImplementedException();
		}

		private void onSceneReset(SceneManagerEventArgs args) {
			throw new NotImplementedException();
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

			_netClient = new NetClient(provider, config);
			netClient.start();

			netClient.onConnectedToServer += onConnectedToServer;
			log("Net client initialized");

			OnNetClientStart?.Invoke(new NetClientEventArgs());
		}

		protected virtual void onConnectedToServer(NetEventArgs args) {
			log("Connected to server");
			OnConnectedToServer?.Invoke(new NetClientEventArgs());
		}

		public virtual void onDraw(GameContext context) {

		}

		public virtual void onUpdate(GameContext context) {
			if(netClient != null)
				netClient.updateClient();
		}

		public virtual void onDispose(GameContext context) {
			if(netClient != null)
				netClient.stop();

			netClient = null;
		}
	}
}
