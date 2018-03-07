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
			router.addClientPacketCallback<P_EntityPacketContainer>(cbClientEntityPacketContainer);
			router.addServerPacketCallback<P_EntityPacketContainer>(cbServerEntityPacketContainer);
			router.addClientPacketCallback<P_CreateEntityResponseAccept>(cbClientCreateEntityAccept);

			localRemoteMap.Clear();
			remoteLocalMap.Clear();
			pendingIDMap.Clear();
			entityIDMap.Clear();
			clientQueue.Clear();
			serverQueue.Clear();
		}

		private void cbServerEntityPacketContainer(P_EntityPacketContainer obj) {
			var ent = entityIDMap[obj.entityID];
			ent.getPacketRouter().onServerReceivePacket(obj.packet);
		}

		private void cbClientEntityPacketContainer(P_EntityPacketContainer obj) {
			var ent = entityIDMap[obj.entityID];
			ent.getPacketRouter().onClientReceivePacket(obj.packet);
		}

		private void cbServerCreateEntityRequest(P_CreateEntityRequest obj) {
			var inst = obj.entity;
			inst.setEntityID(nextEntityID++);
			instantiateRemoteEntity(inst);

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
			instantiateRemoteEntity(ent);
		}

		private void cbClientCreateEntityDeny(P_CreateEntityResponseDeny obj) {
			throw new NotImplementedException();
		}

		private void cbClientCreateEntity(P_CreateEntity obj) {
			instantiateRemoteEntity(obj.entity);
		}

		private void cbSetRemoteID(P_SetRemoteID obj) {
			throw new NotImplementedException();
		}

		private void onEntityDestroyed(SceneEventArgs args) {
			var inst = args.entity as NetEntity;
			if(inst != null)
				netEntityList.Remove(inst);
		}

		public override IEntity instantiate(IEntity ent) {
			base.instantiate(ent);
			var netEntity = ent as NetEntity;
			if (netEntity != null) {
				instantiateLocalEntity(netEntity);
			}

			return ent;
		}

		public void instantiateRemoteEntity(INetEntity entity) {
			base.instantiate(entity);
			netEntityList.Add(entity);
			entityIDMap.Add(entity.getEntityID(), entity);
			entity.onNetInitialize(netService, netHandler);
		}

		private void instantiateLocalEntity(INetEntity netEntity) {
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
				packet.localID = netEntity.getEntityID();
				packet.entity = netEntity;

				queuePacket(new PacketInfo() {
					packet = packet,
				});
			}

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
			var config = netHandler.getNetConfig() as NetConfig;

			buffer.write((int)netEntityList.Count);
			for (int i = 0; i < netEntityList.Count; i++) {
				var ent = netEntityList[i];

				config.writeEntityHeader(buffer, ent);
				ent.writeTo(buffer);
			}
		}

		public virtual void readFrom(IDataBuffer buffer) {
			var config = netHandler.getNetConfig() as NetConfig;

			localRemoteMap.Clear();
			remoteLocalMap.Clear();
			entityIDMap.Clear();

			var ct = buffer.readInt32();
			for(int i = 0; i < ct; i++) {
				var ent = config.readEntityHeader(buffer);
				ent.readFrom(buffer);

				instantiateRemoteEntity(ent);
			}
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
