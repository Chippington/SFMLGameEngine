using SFMLEngine.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFMLEngine.Scenes;
using SFMLEngine.Network.Entities;
using NetUtils;
using NetUtils.Net.Data;
using NetUtils.Net.Interfaces;
using NetUtils.Utilities;
using SFMLEngine.Network.Services;

namespace SFMLEngine.Network.Scenes {
	public class NetScene : Scene, INetScene {
		private IDMap<INetEntity> entityIDMap;
		private Queue<PacketInfo> clientQueue;
		private Queue<PacketInfo> serverQueue;
		private List<NetEntity> netEntityList;
		private NetworkHandler netHandler;
		private NetServiceBase netService;

		public NetScene() {
			clientQueue = new Queue<PacketInfo>();
			serverQueue = new Queue<PacketInfo>();

			netEntityList = new List<NetEntity>();
			entityIDMap = new IDMap<INetEntity>();

			OnEntityDestroyed += onEntityDestroyed;
		}

		public void onNetInitialize(NetServiceBase netService, NetworkHandler netHandler) {
			this.netHandler = netHandler;
			this.netService = netService;

			netHandler.addClientPacketCallback<P_SetRemoteID>(cbSetRemoteID);
			netHandler.addClientPacketCallback<P_CreateEntity>(cbClientCreateEntity);
			netHandler.addServerPacketCallback<P_CreateEntity>(cbServerCreateEntity);

			clientQueue.Clear();
			serverQueue.Clear();
		}

		private void cbServerCreateEntity(P_CreateEntity obj) {
			throw new NotImplementedException();
		}

		private void cbClientCreateEntity(P_CreateEntity obj) {

		}

		private void cbSetRemoteID(P_SetRemoteID obj) {
			throw new NotImplementedException();
		}

		private void onEntityDestroyed(SceneEventArgs args) {
			var inst = args.entity as NetEntity;
			if(inst != null)
				netEntityList.Remove(inst);
		}

		public override T instantiate<T>(params object[] args) {
			var inst = base.instantiate<T>(args);
			var netEntity = inst as NetEntity;
			if(netEntity != null) {
				netEntityList.Add(netEntity);
				entityIDMap.addObject(netEntity);

				P_CreateEntity packet = new P_CreateEntity();
				packet.entity = netEntity;

				queuePacket(new PacketInfo() {
					packet = packet,
					sendToAll = true,
				});
			}

			return inst;
		}

		public void onClientUpdate() {
			for (int i = 0; i < netEntityList.Count; i++) {
				netEntityList[i].onClientUpdate();
				var outgoing = netEntityList[i].getOutgoingClientPackets();
			}
		}

		public void onServerUpdate() {
			for (int i = 0; i < netEntityList.Count; i++) {
				var entity = netEntityList[i];
				entity.onServerUpdate();
				var outgoing = entity.getOutgoingServerPackets();
				while(outgoing.Count > 0) {
					EntityPacketContainer packet = new EntityPacketContainer();
					packet.entityID = entity.id;
				}
			}
		}

		public bool isServer() {
			return netHandler.isServer();
		}

		public bool isClient() {
			return netHandler.isClient();
		}

		public void queuePacket(PacketInfo info) {
			if (netHandler.isServer()) serverQueue.Enqueue(info);
			if (netHandler.isClient()) clientQueue.Enqueue(info);
		}

		public virtual void writeTo(IDataBuffer buffer) {

		}

		public virtual void readFrom(IDataBuffer buffer) {

		}

		public Queue<PacketInfo> getOutgoingClientPackets() {
			return clientQueue;
		}

		public Queue<PacketInfo> getOutgoingServerPackets() {
			return serverQueue;
		}
	}
}
