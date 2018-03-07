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
}
