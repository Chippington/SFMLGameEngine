using NetUtils.Net.Interfaces;
using SFMLEngine.Scenes;
using SFMLEngine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Network.Services {
	public interface INetServiceBase : IGameService {
		NetworkHandler getNetHandler();
	}
}
