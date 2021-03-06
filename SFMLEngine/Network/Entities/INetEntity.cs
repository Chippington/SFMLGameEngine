﻿using NetUtils;
using NetUtils.Net.Data;
using NetUtils.Net.Default;
using SFMLEngine.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Network.Entities {
	public interface INetEntity : IEntity, INetUpdatable, ISerializable {
		Queue<PacketInfo> getOutgoingClientPackets();
		Queue<PacketInfo> getOutgoingServerPackets();
		PacketHandler getPacketRouter();
		void setEntityID(int id);
		int getEntityID();
	}
}
