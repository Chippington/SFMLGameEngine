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
		private HashSet<INetEntity> entityHashSet;
		private Queue<INetEntity> instantiationQueue;
		private Dictionary<int, INetEntity> pendingIDMap;
		private Dictionary<int, INetEntity> netEntityIDMap;
		private List<INetEntity> netEntityList;
		private Queue<PacketInfo> clientQueue;
		private Queue<PacketInfo> serverQueue;
		private NetworkHandler netHandler;
		private NetServiceBase netService;
		private PacketHandler router;
		private int nextEntityID;

		public NetScene() {
			entityHashSet = new HashSet<INetEntity>();
			instantiationQueue = new Queue<INetEntity>();
			pendingIDMap = new Dictionary<int, INetEntity>();
			netEntityIDMap = new Dictionary<int, INetEntity>();
			netEntityList = new List<INetEntity>();
			clientQueue = new Queue<PacketInfo>();
			serverQueue = new Queue<PacketInfo>();
			nextEntityID = 0;
		}

		public virtual void onNetInitialize(NetServiceBase netService, NetworkHandler netHandler) {
			log("Net-Initializing NetScene");
			this.router = new PacketHandler();
			this.netHandler = netHandler;
			this.netService = netService;

			pendingIDMap.Clear();
			netEntityIDMap.Clear();
			clientQueue.Clear();
			serverQueue.Clear();

			log("Creating callback hooks");
			router.addClientPacketCallback<P_CreateEntity>(cbClientCreateEntity);
			router.addClientPacketCallback<P_CreateEntityResponseDeny>(cbClientCreateEntityDeny);
			router.addClientPacketCallback<P_EntityPacketContainer>(cbClientEntityPacketContainer);
			router.addClientPacketCallback<P_CreateEntityResponseAccept>(cbClientCreateEntityAccept);

			router.addServerPacketCallback<P_EntityPacketContainer>(cbServerEntityPacketContainer);
			router.addServerPacketCallback<P_CreateEntityRequest>(cbServerCreateEntityRequest);

			if(instantiationQueue.Count > 0)
				log("Instantiating queued entities");

			while (instantiationQueue.Count > 0)
				netInstantiate(instantiationQueue.Dequeue());
		}

		private void cbServerEntityPacketContainer(P_EntityPacketContainer obj) {
			var ent = netEntityIDMap[obj.entityID];
			ent.getPacketRouter().onServerReceivePacket(obj.packet);
		}

		private void cbClientEntityPacketContainer(P_EntityPacketContainer obj) {
			var ent = netEntityIDMap[obj.entityID];
			ent.getPacketRouter().onClientReceivePacket(obj.packet);
		}

		protected virtual void cbServerCreateEntityRequest(P_CreateEntityRequest obj) {
			var ent = obj.entity;
			ent.setEntityID(nextEntityID++);

			base.instantiate(ent);
			addNetEntityHooks(ent);

			P_CreateEntityResponseAccept packet = new P_CreateEntityResponseAccept();
			packet.localID = obj.localID;
			packet.remoteID = ent.getEntityID();
			queuePacket(new PacketInfo() {
				recipients = new List<ClientInfo>() { obj.sender },
				packet = packet
			});

			P_CreateEntity packet2 = new P_CreateEntity();
			packet2.entity = ent;
			queuePacket(new PacketInfo() {
				sendToAll = true,
				exclude = new List<ClientInfo>() { obj.sender },
				packet = packet2,
			});
		}

		protected virtual void cbClientCreateEntityAccept(P_CreateEntityResponseAccept obj) {
			var netEntity = pendingIDMap[obj.localID];
			pendingIDMap.Remove(obj.localID);

			netEntity.setEntityID(obj.remoteID);
			addNetEntityHooks(netEntity);
		}

		protected virtual void cbClientCreateEntityDeny(P_CreateEntityResponseDeny obj) {
			throw new NotImplementedException();
		}

		protected virtual void cbClientCreateEntity(P_CreateEntity obj) {
			base.instantiate(obj.entity);
			addNetEntityHooks(obj.entity);
		}

		public override IEntity instantiate(IEntity ent) {
			ent = base.instantiate(ent);
			var netEnt = ent as NetEntity;
			if(netEnt != null) {
				netInstantiate(netEnt);
			}

			return ent;
		}

		private void netInstantiate(INetEntity netEntity) {
			netEntity.setEntityID(nextEntityID++);
			if (netHandler == null) {
				instantiationQueue.Enqueue(netEntity);
				return;
			}

			if (this.containsEntity(netEntity) == false)
				throw new Exception("Cannot net-instantiate a non-mapped entity.");
			
			var info = new PacketInfo();

			if(isServer()) {
				addNetEntityHooks(netEntity);

				P_CreateEntity packet = new P_CreateEntity();
				packet.entity = netEntity;

				info.sendToAll = true;
				info.packet = packet;
				queuePacket(info);
			}

			if(isClient()) {
				P_CreateEntityRequest packet = new P_CreateEntityRequest();
				packet.localID = netEntity.getEntityID();
				packet.entity = netEntity;

				info.packet = packet;
				queuePacket(info);

				pendingIDMap.Add(netEntity.getEntityID(), netEntity);
			}
		}
		
		private void addNetEntityHooks(INetEntity netEntity) {
			entityHashSet.Add(netEntity);
			netEntityList.Add(netEntity);
			netEntityIDMap.Add(netEntity.getEntityID(), netEntity);
			netEntity.onNetInitialize(netService, netHandler);
		}

		public void onClientUpdate() {
			for (int i = 0; i < netEntityList.Count; i++) {
				var entity = netEntityList[i];
				entity.onClientUpdate();
				var outgoing = netEntityList[i].getOutgoingClientPackets();
				while (outgoing.Count > 0) {
					var info = outgoing.Dequeue();
					P_EntityPacketContainer packet = new P_EntityPacketContainer();
					packet.packet = info.packet;
					info.packet = packet;

					packet.entityID = entity.getEntityID();
					queuePacket(info);
				}
			}
		}

		public void onServerUpdate() {
			for (int i = 0; i < netEntityList.Count; i++) {
				var entity = netEntityList[i];
				entity.onServerUpdate();
				var outgoing = entity.getOutgoingServerPackets();
				while(outgoing.Count > 0) {
					var info = outgoing.Dequeue();
					P_EntityPacketContainer packet = new P_EntityPacketContainer();
					packet.packet = info.packet;
					info.packet = packet;

					packet.entityID = entity.getEntityID();
					queuePacket(info);
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
