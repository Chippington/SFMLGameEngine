using NetUtils.Net.Data;
using NetUtils.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Network {
	public class PacketContainer : Packet {
		public Packet packet;

		public override void writeTo(IDataBuffer buffer) {
			base.writeTo(buffer);
			config.writePacketHeader(buffer, packet);
			packet.config = config;
			packet.sender = sender;
			packet.writeTo(buffer);
		}

		public override void readFrom(IDataBuffer buffer) {
			base.readFrom(buffer);
			packet = config.readPacketHeader(buffer);
			packet.config = config;
			packet.sender = sender;
			packet.readFrom(buffer);
		}
	}
}
