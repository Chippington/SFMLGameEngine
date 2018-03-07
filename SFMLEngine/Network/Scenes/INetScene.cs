using NetUtils;
using NetUtils.Net.Data;
using NetUtils.Net.Interfaces;
using SFMLEngine.Entities;
using SFMLEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Network.Scenes {
	public interface INetScene : IScene, INetUpdatable, ISerializable {
		Queue<PacketInfo> getOutgoingClientPackets();
		Queue<PacketInfo> getOutgoingServerPackets();
		IPacketHandler getPacketRouter();
	}
}
