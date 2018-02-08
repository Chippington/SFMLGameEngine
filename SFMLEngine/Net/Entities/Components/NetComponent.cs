using SFMLEngine.Entities.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetUtils.Net.Services.Entities;
using SFMLEngine.Entities;

namespace SFMLEngine.Net.Entities.Components {
	public class NetComponent : Component, INetComponent {
		private INetEntity netEntity;

		public void onNetInitialize(INetEntity owner) {
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
