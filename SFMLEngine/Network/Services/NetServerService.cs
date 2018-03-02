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

namespace SFMLEngine.Network.Services {
	public delegate void NetServerEvent(NetServerEventArgs args);
	public class NetServerEventArgs {

	}

	public class NetServerService : ObjectBase, IGameService {
		private INetworkProvider provider;
		private NetServer _netServer;
		private NetConfig config;

		public NetServerEvent OnServerStarted;
		public NetServerEvent OnClientConnected;
		public NetServerEvent OnClientDisconnected;

		public NetServer netServer { get => _netServer; set { } }

		public virtual void onInitialize(GameContext context) {
			DebugLog.setLogger(new DebugLogger());
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

			_netServer = new NetServer(provider, config);
			netServer.start();

			netServer.onClientConnected += onClientConnected;
			log("Net server initialized");

			OnServerStarted?.Invoke(new NetServerEventArgs());
		}

		protected virtual void onClientConnected(NetEventArgs args) {
			log(string.Format("Client connected: {0}", args.client.ipendpoint));
			OnClientConnected?.Invoke(new NetServerEventArgs());
		}

		public virtual void onDraw(GameContext context) {

		}

		public virtual void onUpdate(GameContext context) {
			if(netServer != null)
				netServer.updateServer();
		}

		public virtual void onDispose(GameContext context) {
			if(netServer != null)
				netServer.stop();

			netServer = null;
		}
	}
}
