using NetUtils.Net.Data;
using NetUtils.Net.Interfaces;
using SFMLEngine.Network.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Network.Scenes {
	public static class NetScenePackets {
		public static void registerPackets(NetConfig config) {
			config.registerPacket<P_DeleteEntity>();
			config.registerPacket<P_DeleteEntityRequest>();
			config.registerPacket<P_DeleteEntityRequestDeny>();
			config.registerPacket<P_DeleteEntityRequestAccept>();
			config.registerPacket<P_CreateEntityResponseAccept>();
			config.registerPacket<P_CreateEntityResponseDeny>();
			config.registerPacket<P_CreateEntityResponse>();
			config.registerPacket<P_ScenePacketContainer>();
			config.registerPacket<P_CreateEntityRequest>();
			config.registerPacket<P_CreateEntity>();
		}
	}

	public class P_ScenePacketContainer : PacketContainer {
		public byte sceneID;

		public override void writeTo(IDataBuffer buffer) {
			base.writeTo(buffer);
			buffer.write((byte)sceneID);
		}

		public override void readFrom(IDataBuffer buffer) {
			base.readFrom(buffer);
			sceneID = buffer.readByte();
		}
	}

	public class P_CreateEntity : Packet {
		public INetEntity entity;

		public P_CreateEntity() {

		}

		public override void writeTo(IDataBuffer buffer) {
			base.writeTo(buffer);
			(config as NetConfig).writeEntityHeader(buffer, entity);
			entity.writeTo(buffer);
		}

		public override void readFrom(IDataBuffer buffer) {
			base.readFrom(buffer);
			entity = (config as NetConfig).readEntityHeader(buffer);
			entity.readFrom(buffer);
		}
	}

	public class P_CreateEntityRequest : P_CreateEntity {
		public int localID;

		public override void writeTo(IDataBuffer buffer) {
			base.writeTo(buffer);
			buffer.write((int)localID);
		}

		public override void readFrom(IDataBuffer buffer) {
			base.readFrom(buffer);
			localID = buffer.readInt32();
		}
	}

	public class P_CreateEntityResponse : Packet {
		public int localID, remoteID;

		public override void writeTo(IDataBuffer buffer) {
			base.writeTo(buffer);
			buffer.write((int)localID);
			buffer.write((int)remoteID);
		}

		public override void readFrom(IDataBuffer buffer) {
			base.readFrom(buffer);
			localID = buffer.readInt32();
			remoteID = buffer.readInt32();
		}
	}

	public class P_CreateEntityResponseAccept : P_CreateEntityResponse { }

	public class P_CreateEntityResponseDeny : P_CreateEntityResponse {
		public int errorCode;

		public override void writeTo(IDataBuffer buffer) {
			base.writeTo(buffer);
			buffer.write((byte)errorCode);
		}

		public override void readFrom(IDataBuffer buffer) {
			base.readFrom(buffer);
			errorCode = buffer.readByte();
		}
	}

	public class P_DeleteEntity : Packet {
		public int entityID;

		public override void writeTo(IDataBuffer buffer) {
			base.writeTo(buffer);
			buffer.write((int)entityID);
		}

		public override void readFrom(IDataBuffer buffer) {
			base.readFrom(buffer);
			entityID = buffer.readInt32();
		}
	}

	public class P_DeleteEntityRequest : P_DeleteEntity { }
	public class P_DeleteEntityRequestAccept : P_DeleteEntity { }
	public class P_DeleteEntityRequestDeny : P_DeleteEntity {
		public int errorCode;

		public override void writeTo(IDataBuffer buffer) {
			base.writeTo(buffer);
			buffer.write((byte)errorCode);
		}

		public override void readFrom(IDataBuffer buffer) {
			base.readFrom(buffer);
			errorCode = buffer.readByte();
		}
	}
}
