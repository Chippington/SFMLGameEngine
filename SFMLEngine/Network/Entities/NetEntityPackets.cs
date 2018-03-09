using NetUtils.Net.Data;
using NetUtils.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Network.Entities {
	public static class NetEntityPackets {
		public static void registerPackets(NetConfig config) {
			config.registerPacket<P_DeleteEntity>();
			config.registerPacket<P_DeleteEntityRequest>();
			config.registerPacket<P_DeleteEntityRequestDeny>();
			config.registerPacket<P_DeleteEntityRequestAccept>();
			config.registerPacket<P_EntityPacketContainer>();
		}
	}

	public class P_EntityPacketContainer : PacketContainer {
		public byte entityTypeID;
		public int entityID;

		public override void writeTo(IDataBuffer buffer) {
			base.writeTo(buffer);
			buffer.write((int)entityID);
			buffer.write((byte)entityTypeID);
		}

		public override void readFrom(IDataBuffer buffer) {
			base.readFrom(buffer);
			entityID = buffer.readInt32();
			entityTypeID = buffer.readByte();
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
