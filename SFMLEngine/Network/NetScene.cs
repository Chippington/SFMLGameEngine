using NetUtils.Net.Services.Entities;
using SFMLEngine.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Network {
	public class NetScene : Scene, INetScene {
		private List<NetEntity> netEntityList;

		public NetScene() {
			netEntityList = new List<NetEntity>();
			OnEntityDestroyed += onEntityDestroyed;
		}

		private void onEntityDestroyed(SceneEventArgs args) {
			var inst = args.entity as NetEntity;
			if(inst != null)
				netEntityList.Remove(inst);
		}

		public override T instantiate<T>(params object[] args) {
			var inst = base.instantiate<T>(args);
			var netEntity = inst as NetEntity;
			if(netEntity != null) {
				netEntityList.Add(netEntity);
			}

			return inst;
		}

		private void instantiateServer(INetEntity entity) {

		}

		private void instantiateClient(INetEntity entity) {

		}

		public void onClientUpdate() {
			for (int i = 0; i < netEntityList.Count; i++) {
				netEntityList[i].onClientUpdate();
			}
		}

		public void onServerUpdate() {
			for (int i = 0; i < netEntityList.Count; i++) {
				netEntityList[i].onServerUpdate();
			}
		}

		public bool isServer() {
			throw new NotImplementedException();
		}

		public bool isClient() {
			throw new NotImplementedException();
		}
	}
}
