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
using NetUtils.Net.Default;

namespace SFMLEngine.Network.Scenes {
	public class NetScene : Scene, INetScene {
		private Dictionary<int, INetEntity> pendingIDMap;
		private Dictionary<int, INetEntity> entityIDMap;
		private Dictionary<int, int> localRemoteMap;
		private Dictionary<int, int> remoteLocalMap;
		private List<INetEntity> netEntityList;
		private Queue<PacketInfo> clientQueue;
		private Queue<PacketInfo> serverQueue;
		private NetworkHandler netHandler;
		private NetServiceBase netService;
		private PacketHandler router;
		private int nextEntityID;

		public NetScene() {
			pendingIDMap = new Dictionary<int, INetEntity>();
			entityIDMap = new Dictionary<int, INetEntity>();
			remoteLocalMap = new Dictionary<int, int>();
			localRemoteMap = new Dictionary<int, int>();
			netEntityList = new List<INetEntity>();
			clientQueue = new Queue<PacketInfo>();
			serverQueue = new Queue<PacketInfo>();
			nextEntityID = 0;

			OnEntityDestroyed += onEntityDestroyed;
		}

		public virtual void onNetInitialize(NetServiceBase netService, NetworkHandler netHandler) {
			this.router = new PacketHandler();
			this.netHandler = netHandler;
			this.netService = netService;

			router.addClientPacketCallback<P_SetRemoteID>(cbSetRemoteID);
			router.addClientPacketCallback<P_CreateEntity>(cbClientCreateEntity);
			router.addServerPacketCallback<P_CreateEntityRequest>(cbServerCreateEntityRequest);
			router.addClientPacketCallback<P_CreateEntityResponseDeny>(cbClientCreateEntityDeny);
			router.addClientPacketCallback<P_CreateEntityResponseAccept>(cbClientCreateEntityAccept);

			localRemoteMap.Clear();
			remoteLocalMap.Clear();
			pendingIDMap.Clear();
			entityIDMap.Clear();
			clientQueue.Clear();
			serverQueue.Clear();
		}

		public void handlePacket(Packet packet) {
		}

		private void cbServerCreateEntityRequest(P_CreateEntityRequest obj) {
			var inst = obj.entity;
			inst.setEntityID(nextEntityID++);
			instantiate(inst);

			entityIDMap.Add(inst.getEntityID(), inst);
			netEntityList.Add(inst);

			inst.onNetInitialize(netService, netHandler);

			P_CreateEntityResponseAccept response = new P_CreateEntityResponseAccept();
			response.localID = obj.localID;
			response.remoteID = inst.getEntityID();
			queuePacket(new PacketInfo() {
				packet = response,
				recipients = new List<ClientInfo>() { obj.sender },
			});

			P_CreateEntity packet = new P_CreateEntity();
			packet.entity = inst;
			queuePacket(new PacketInfo() {
				packet = packet,
				exclude = new List<ClientInfo>() { obj.sender },
			});
		}

		private void cbClientCreateEntityAccept(P_CreateEntityResponseAccept obj) {
			var ent = pendingIDMap[obj.localID];
			ent.setEntityID(obj.remoteID);
			netEntityList.Add(ent);
			entityIDMap.Add(ent.getEntityID(), ent);
			ent.onNetInitialize(netService, netHandler);
		}

		private void cbClientCreateEntityDeny(P_CreateEntityResponseDeny obj) {
			throw new NotImplementedException();
		}

		private void cbClientCreateEntity(P_CreateEntity obj) {
			var inst = obj.entity;
			instantiate(inst);

			entityIDMap.Add(inst.getEntityID(), inst);
			netEntityList.Add(inst);
			inst.onNetInitialize(netService, netHandler);
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
				handleNewEntity(netEntity);
			}

			return inst;
		}

		private void handleNewEntity(INetEntity netEntity) {
			netEntity.setEntityID(nextEntityID++);
			if (netHandler.isServer()) {

				entityIDMap.Add(netEntity.getEntityID(), netEntity);
				netEntityList.Add(netEntity);

				P_CreateEntity packet = new P_CreateEntity();
				packet.entity = netEntity;

				queuePacket(new PacketInfo() {
					packet = packet,
					sendToAll = true,
				});
			}

			if(netHandler.isClient()) {
				pendingIDMap.Add(netEntity.getEntityID(), netEntity);
				P_CreateEntityRequest packet = new P_CreateEntityRequest();
				packet.entity = netEntity;

				queuePacket(new PacketInfo() {
					packet = packet,
				});
			}

			netEntity.onNetInitialize(netService, netHandler);
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
					packet.entityID = entity.getEntityID();
					//TODO
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

		public IPacketHandler getPacketRouter() {
			return router;
		}
	}
}
