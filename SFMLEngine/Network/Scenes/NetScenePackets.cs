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
			config.registerPacket<ScenePacketContainer>();
		}
	}

	public class ScenePacketContainer : PacketContainer {
		public int sceneID;

		public override void writeTo(IDataBuffer buffer) {
			base.writeTo(buffer);
		}

		public override void readFrom(IDataBuffer buffer) {
			base.readFrom(buffer);
		}
	}

	public class P_CreateEntity : Packet {
		public INetEntity entity;

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

	public class P_SetRemoteID : Packet {
		public int localEntityID;
		public int remoteEntityID;

		public override void writeTo(IDataBuffer buffer) {
			base.writeTo(buffer);
			buffer.write((int)localEntityID);
			buffer.write((int)remoteEntityID);
		}

		public override void readFrom(IDataBuffer buffer) {
			base.readFrom(buffer);
			localEntityID = buffer.readInt32();
			remoteEntityID = buffer.readInt32();
		}
	}
}
