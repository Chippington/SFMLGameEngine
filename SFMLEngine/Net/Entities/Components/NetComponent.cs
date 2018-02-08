using SFMLEngine.Entities.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetUtils.Net.Services.Entities;

namespace SFMLEngine.Net.Entities.Components {
	public class NetComponent : Component, INetComponent {
		public void onNetInitialize(INetEntity owner) {
		}

		public void onClientUpdate() {
			throw new NotImplementedException();
		}

		public void onServerUpdate() {
			throw new NotImplementedException();
		}
	}
}
