﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Network {
	public interface INetUpdatable {
		void onClientUpdate();
		void onServerUpdate();
		bool isServer();
		bool isClient();
	}
}