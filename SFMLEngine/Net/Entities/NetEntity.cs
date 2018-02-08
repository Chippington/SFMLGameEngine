using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetUtils.Utilities;
using NetUtils.Net.Default;
using SFMLEngine.Entities;
using SFMLEngine.Entities.Components;
using SFMLEngine.Net.Entities.Components;

namespace SFMLEngine.Net.Entities {
	public class NetEntity : Entity, INetEntity {
		private List<INetComponent> netComponents;
		private bool netInitialized;

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

		public void onClientUpdate() {
			throw new NotImplementedException();
		}

		public void onServerUpdate() {
			throw new NotImplementedException();
		}
	}
}
