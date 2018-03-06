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
using SFMLEngine.Scenes;
using NetUtils;
using NetUtils.Net.Interfaces;
using NetUtils.Net.Data;
using SFMLEngine.Network.Scenes;
using SFMLEngine.Network.Services;

namespace SFMLEngine.Network.Entities {
	public class NetEntity : Entity, INetEntity, ISerializable {
		private List<INetComponent> netComponents;
		private NetworkHandler netHandler;
		private NetServiceBase netService;
		private bool netInitialized;
		private INetScene netScene;
		private int entityID;

		private Queue<PacketInfo> outgoingClientPackets;
		private Queue<PacketInfo> outgoingServerPackets;

		public NetEntity() {
			netComponents = new List<INetComponent>();
			netInitialized = false;
		}

		public virtual void onNetInitialize(NetServiceBase netService, NetworkHandler netHandler) {
			this.netHandler = netHandler;
			this.netService = netService;

			outgoingServerPackets = new Queue<PacketInfo>();
			outgoingClientPackets = new Queue<PacketInfo>();

			netInitialized = true;
		}

		private void onComponentAdded(ComponentEventArgs args) {
			var netComponent = args.component as INetComponent;
			if (netComponent != null) {
				netComponents.Add(netComponent);
				if (netInitialized) {
					netComponent.onNetInitialize(netService, netHandler);
					netComponent.onNetEntityInitialize(this);
				}
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

		public virtual void onClientUpdate() { }

		public virtual void onServerUpdate() { }

		public bool isServer() {
			if (netScene != null) return netScene.isServer();
			return false;
		}

		public bool isClient() {
			if (netScene != null) return netScene.isClient();
			return false;
		}

		public virtual void writeTo(IDataBuffer buffer) {
			buffer.write((int)entityID);
		}

		public virtual void readFrom(IDataBuffer buffer) {
			entityID = buffer.readInt32();
		}

		public Queue<PacketInfo> getOutgoingClientPackets() {
			return outgoingClientPackets;
		}

		public Queue<PacketInfo> getOutgoingServerPackets() {
			return outgoingServerPackets;
		}

		public void setEntityID(int id) {
			entityID = id;
		}

		public int getEntityID() {
			return entityID;
		}
	}
}