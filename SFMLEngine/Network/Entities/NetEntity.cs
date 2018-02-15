using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetUtils.Utilities;
using NetUtils.Net.Default;
using SFMLEngine.Entities;
using SFMLEngine.Entities.Components;
using SFMLEngine.Network.Entities.Components;

namespace SFMLEngine.Network.Entities {
	public class NetEntity : Entity, INetEntity {
		private List<INetComponent> netComponents;
		private bool netInitialized;
		private INetScene netScene;

		public NetEntity() {
			netComponents = new List<INetComponent>();
			netInitialized = false;
		}

		private void onComponentAdded(ComponentEventArgs args) {
			var netComponent = args.component as INetComponent;
			if (netComponent != null) {
				netComponents.Add(netComponent);
				if (netInitialized)
					netComponent.onNetInitialize(this);
			}
		}

		private void onComponentRemoved(ComponentEventArgs args) {
			var netComponent = args.component as INetComponent;
			if (netComponent != null)
				netComponents.Remove(netComponent);
		}

		public override void onInitialize(GameContext context) {
			base.onInitialize(context);
			components.OnComponentAdded += onComponentAdded;
			components.OnComponentRemoved += onComponentRemoved;

			foreach (var c in components) {
				var nc = c.Value as INetComponent;
				if (nc != null)
					netComponents.Add(nc);
			}
		}

		public override void setOwner(Scene owner) {
			base.setOwner(owner);
			netScene = owner as INetScene;
		}

		public void onClientUpdate() {
			throw new NotImplementedException();
		}

		public void onServerUpdate() {
			throw new NotImplementedException();
		}

		public bool isServer() {
			if (netScene != null) return netScene.isServer();
			return false;
		}

		public bool isClient() {
			if (netScene != null) return netScene.isClient();
			return false;
		}
	}
}
