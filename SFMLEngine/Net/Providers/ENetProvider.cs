using ENet;
using NetUtils.Net.Data;
using NetUtils.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Net.Providers {
	public class ENetProvider : INetworkProvider {
		public Peer clientPeer;
		public Host host;
		private bool isServ;
		List<ClientInfo> clientList;
		Dictionary<Peer, ClientInfo> clientMap;
		Queue<PData> clientSendQueue;

		class PData {
			public PacketInfo info;
			public IDataBuffer buff;
		}

		public static int cfgPeer = -1;

		public override void startServer(NetProviderConfig config) {
			clientMap = new Dictionary<Peer, ClientInfo>();
			clientList = new List<ClientInfo>();

			base.startServer(config);
			host = new Host();
			host.InitializeServer(config.port, config.maxclients);
			isServ = true;
		}

		public override void startClient(NetProviderConfig config) {
			clientMap = new Dictionary<Peer, ClientInfo>();
			clientSendQueue = new Queue<PData>();
			clientList = new List<ClientInfo>();

			base.startClient(config);
			host = new Host();
			host.InitializeClient(config.maxclients);

			clientPeer = host.Connect(config.ipaddress, config.port, 0);
			isServ = false;
		}

		public override void stopClient() {
			base.stopClient();
		}

		public override void stopServer() {
			base.stopServer();
		}

		public override List<ClientInfo> getClients() {
			return new List<ClientInfo>(clientList);
		}

		public override void sendMessage(IDataBuffer buffer, PacketInfo info) {
			base.sendMessage(buffer, info);
			if (isServ) {
				if (info.sendToAll)
					info.recipients = clientList;

				Queue<ClientInfo> dc = new Queue<ClientInfo>();
				foreach (ClientInfo cl in info.recipients) {
					try {
						var bytes = buffer.toBytes();

						sendMessage(bytes, cl);
					} catch (Exception) {
						dc.Enqueue(cl);
					}
				}

				while (dc.Count > 0) {
					var cl = dc.Dequeue();
					onClientDisconnected(new NetEventArgs() {
						client = cl,
					});

					clientList.Remove(cl);
				}
			} else {
				if (clientPeer.State != PeerState.Connected && clientPeer.State != PeerState.Disconnected)
					clientSendQueue.Enqueue(new PData() {
						info = info,
						buff = buffer
					});
				else
					clientPeer.Send(0, buffer.toBytes(), PacketFlags.Reliable);
			}
		}

		private void sendMessage(byte[] bytes, ClientInfo cl) {
			onDataSent?.Invoke(new NetEventArgs() {
				client = cl,
				bytesSent = bytes.Length,
			});

			Peer p = cl.getLocal<Peer>(cfgPeer);
			p.Send(0, bytes, PacketFlags.Reliable);
		}

		public override void update() {
			base.update();
			Event ev;

			if (host.Service(0, out ev)) {
				if (isServ) handleServerEvent(ev); else handleClientEvent(ev);
			}

			//while (host.CheckEvents(out ev)) {
			//	if (isServ) handleServerEvent(ev); else handleClientEvent(ev);
			//}
		}

		private void handleClientEvent(Event ev) {
			switch (ev.Type) {
				case EventType.Connect:
					onConnectedToServer?.Invoke(new NetEventArgs() {

					});

					while (clientSendQueue.Count > 0) {
						var info = clientSendQueue.Dequeue();
						sendMessage(info.buff, info.info);
					};
					break;

				case EventType.Disconnect:
					onDisconnectedFromServer?.Invoke(new NetEventArgs() {

					});
					break;

				case EventType.Receive:
					byte[] data = ev.Packet.GetBytes();
					var buff = new DataBufferStream();
					buff.write((byte[])data, 0, data.Length);
					buff.seek(0);

					onMessageReceived?.Invoke(new NetEventArgs() {
						dataBuffer = buff,
						bytesRead = data.Length,
					});

					onDataReceived?.Invoke(new NetEventArgs() {
						bytesRead = data.Length,
					});
					break;
			}
		}

		private void handleServerEvent(Event ev) {
			Peer peer = ev.Peer;
			ClientInfo client;

			switch (ev.Type) {
				case EventType.Connect:
					client = new ClientInfo();
					client.ipendpoint = peer.GetRemoteAddress().ToString();
					client.setLocal(cfgPeer, peer);
					clientList.Add(client);
					clientMap.Add(peer, client);
					onClientConnected?.Invoke(new NetEventArgs() {
						client = client,
					});
					break;

				case EventType.Disconnect:
					if (clientMap.ContainsKey(peer)) {
						onClientDisconnected?.Invoke(new NetEventArgs() {
							client = clientMap[peer],
						});

						clientList.Remove(clientMap[peer]);
						clientMap.Remove(peer);
					}
					break;

				case EventType.None:

					break;

				case EventType.Receive:
					byte[] data = ev.Packet.GetBytes();
					var buff = new DataBufferStream();
					buff.write((byte[])data, 0, data.Length);
					buff.seek(0);

					onMessageReceived?.Invoke(new NetEventArgs() {
						dataBuffer = buff,
						client = clientMap[peer],
						bytesRead = data.Length,
					});

					onDataReceived?.Invoke(new NetEventArgs() {
						client = clientMap[peer],
						bytesRead = data.Length,
					});
					break;
			}
		}

		/// <summary>
		/// Creates a buffer for use with the NetworkProvider.
		/// </summary>
		/// <param name="capacity"></param>
		/// <returns></returns>
		public override IDataBuffer createBuffer(int capacity) {
			return new DataBufferStream();
		}

		/// <summary>
		/// Creates a buffer for use with the NetworkProvider.
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		public override IDataBuffer createBuffer(IDataBuffer buffer) {
			return new DataBufferStream(buffer.toBytes());
		}
	}
}
