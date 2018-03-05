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
using NetUtils.Net.Data;

namespace SFMLEngine.Network.Services {
	public delegate void NetServerEvent(NetServerEventArgs args);
	public class NetServerEventArgs {

	}

	public class NetServerService : NetServiceBase, IGameService {
		private INetworkProvider provider;
		private NetServer _netServer;
		private NetConfig config;
		private GameContext context;

		public NetServerEvent OnServerStarted;
		public NetServerEvent OnClientConnected;
		public NetServerEvent OnClientDisconnected;

		public NetServer netServer { get => _netServer; set { } }

		public override void onInitialize(GameContext context) {
			DebugLog.setLogger(new NetDebugLogger());

			if (context.services.hasService<NetSceneManager>(true) == false)
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

			this.context = context;
		}

		protected override void onSceneRegistered(SceneManagerEventArgs args) {
			base.onSceneRegistered(args);
		}

		protected override void onSceneDeactivated(SceneManagerEventArgs args) {
			base.onSceneDeactivated(args);
		}

		protected override void onSceneActivated(SceneManagerEventArgs args) {
			base.onSceneActivated(args);

			if (_netServer == null)
				return;

			var netScene = args.scene as INetScene;
			if (netScene == null)
				return;

			var id = idFromScene(netScene);
			if (id == null)
				return;
			
			DataBufferStream buff = new DataBufferStream();
			netScene.writeTo(buff);

			P_SceneChange packet = new P_SceneChange(id.Value, buff);
			_netServer.sendToClients(new PacketInfo() {
				packet = packet,
				sendToAll = true,
			});
		}

		protected override void onSceneReset(SceneManagerEventArgs args) {
			base.onSceneReset(args);
		}

		public virtual void startServer(NetConfig config) {
			startServer(config, new ENetProvider());
		}

		public virtual void startServer(NetConfig config, INetworkProvider provider) {
			log("Starting net server");
			log(string.Format("Config settings: \r\nPort: {0}\r\nMax clients: {1}",
				config.port, config.maxclients));

			this.provider = provider;
			this.config = config;

			config.registerPacket<P_SceneChange>();

			_netServer = new NetServer(provider, config);
			netServer.start();

			netServer.onClientConnected += onClientConnected;
			log("Net server initialized");

			onSceneActivated(new SceneManagerEventArgs() {
				scene = context.sceneManager.getActiveScene(),
				active = true,
			});

			OnServerStarted?.Invoke(new NetServerEventArgs());
			foreach (var s in sceneList)
				s.onNetInitialize(_netServer);
		}

		protected virtual void onClientConnected(NetEventArgs args) {
			log(string.Format("Client connected: {0}", args.client.ipendpoint));
			OnClientConnected?.Invoke(new NetServerEventArgs());
			var netScene = context.sceneManager.getActiveScene() as INetScene;
			if (netScene == null)
				return;

			var id = idFromScene(netScene);
			if (id == null)
				return;

			DataBufferStream buff = new DataBufferStream();
			netScene.writeTo(buff);
			P_SceneChange packet = new P_SceneChange(id.Value, buff);
			_netServer.sendToClients(new PacketInfo() {
				packet = packet,
				sendToAll = false,
				recipients = new List<ClientInfo>() {
					args.client,
				}
			});
		}

		public override void onDraw(GameContext context) {

		}

		public override void onUpdate(GameContext context) {
			if(netServer != null)
				netServer.updateServer();
		}

		public override void onDispose(GameContext context) {
			if(netServer != null)
				netServer.stop();

			netServer = null;
		}
	}
}
