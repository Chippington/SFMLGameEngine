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
using SFMLEngine.Network.Scenes;

namespace SFMLEngine.Network.Services {
	public delegate void NetServerEvent(NetServerEventArgs args);
	public class NetServerEventArgs {
		public ClientInfo client;
	}

	public class NetServerService : NetServiceBase, IGameService {
		private NetSceneManager sceneManager;
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

			sceneManager = context.services.getService<NetSceneManager>();

			context.sceneManager.OnSceneReset += onSceneReset;
			context.sceneManager.OnSceneActivated += onSceneActivated;

			var scenes = sceneManager.getScenes();
			var activeScene = context.sceneManager.getActiveScene();
			onSceneActivated(new SceneManagerEventArgs() {
				scene = activeScene,
			});

			this.context = context;
		}

		protected void onSceneActivated(SceneManagerEventArgs args) {
			if (_netServer == null)
				return;

			var netScene = args.scene as INetScene;
			if (netScene == null)
				return;

			var id = sceneManager.idFromScene(netScene);
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

		protected void onSceneReset(SceneManagerEventArgs args) {
			if (_netServer == null)
				return;

			var netScene = args.scene as INetScene;
			if (netScene == null)
				return;

			var id = sceneManager.idFromScene(netScene);
			if (id == null)
				return;

			DataBufferStream buff = new DataBufferStream();
			netScene.writeTo(buff);

			P_SceneReset packet = new P_SceneReset(id.Value, buff);
			_netServer.sendToClients(new PacketInfo() {
				packet = packet,
				sendToAll = true,
			});
		}

		public virtual void startServer(SFMLEngine.Network.NetConfig config) {
			startServer(config, new ENetProvider());
		}

		public void startServer(SFMLEngine.Network.NetConfig config, INetworkProvider provider) {
			log("Starting net server");
			log(string.Format("Config settings: \r\nPort: {0}\r\nMax clients: {1}",
				config.port, config.maxclients));

			this.provider = provider;
			this.config = config;

			NetServicePackets.registerPackets(config);
			NetScenePackets.registerPackets(config);

			_netServer = new NetServer(provider, config);
			netServer.start();

			netServer.onClientConnected += onClientConnected;
			netServer.onClientDisconnected += onClientDisconnected;
			log("Net server initialized");

			sceneManager.onNetInitialize(this, _netServer);
			onSceneActivated(new SceneManagerEventArgs() {
				scene = context.sceneManager.getActiveScene(),
				active = true,
			});

			OnServerStarted?.Invoke(new NetServerEventArgs());
			foreach (var s in sceneManager.getNetScenes())
				s.onNetInitialize(this, _netServer);
		}

		protected virtual void onClientDisconnected(NetEventArgs args) {
			log(string.Format("Client disconnected: {0}", args.client.ipendpoint));
			OnClientDisconnected?.Invoke(new NetServerEventArgs() {
				client = args.client,
			});
		}

		protected virtual void onClientConnected(NetEventArgs args) {
			log(string.Format("Client connected: {0}", args.client.ipendpoint));
			OnClientConnected?.Invoke(new NetServerEventArgs() {
				client = args.client,
			});

			var netScene = context.sceneManager.getActiveScene() as INetScene;
			if (netScene == null)
				return;

			var id = sceneManager.idFromScene(netScene);
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

		public override NetworkHandler getNetHandler() {
			return _netServer;
		}
	}
}
