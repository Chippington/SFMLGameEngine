using NetUtils.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Network {
	public interface INetBase {
		void onNetInitialize(NetworkHandler netHandler);
		bool isServer();
		bool isClient();
	}
}
