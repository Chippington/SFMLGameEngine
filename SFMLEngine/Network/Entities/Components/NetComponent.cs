using SFMLEngine.Entities.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFMLEngine.Entities;
using SFMLEngine.Network.Entities.Components;
using SFMLEngine.Network.Entities;
using NetUtils.Net.Interfaces;

namespace SFMLEngine.Net.Entities.Components {
	public class NetComponent : Component, INetComponent {
		private INetEntity netEntity;

		public void onNetInitialize(NetworkHandler netHandler) {
		}

		public void onNetEntityInitialize(INetEntity owner) {
		}

		public void onClientUpdate() {
			throw new NotImplementedException();
		}

		public void onServerUpdate() {
			throw new NotImplementedException();
		}

		public bool isServer() {
			if (netEntity != null) return netEntity.isServer();
			return false;
		}

		public bool isClient() {
			if (netEntity != null) return netEntity.isClient();
			return false;
		}

		public override void setEntity(IEntity owner) {
			base.setEntity(owner);
			netEntity = owner as INetEntity;
		}
	}
}
