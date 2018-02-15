using NetUtils.Net.Services.Entities;
using SFMLEngine.Entities.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Network.Entities.Components {
	public interface INetComponent : IComponent, INetUpdatable {
		void onNetInitialize(INetEntity owner);
	}
}
