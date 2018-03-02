using NetUtils.Net.Services.Entities;
using SFMLEngine.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetUtils.Net.Interfaces;
using SFMLEngine.Scenes;

namespace SFMLEngine.Network {
	public class NetScene : Scene, INetScene {
		private List<NetEntity> netEntityList;
		private bool isServerFlag, isClientFlag;

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
			isServerFlag = true;
			if (isClientFlag)
				throw new Exception("NetScene cannot be utilized by both client and server services.");
		}

		private void instantiateClient(INetEntity entity) {
			isClientFlag = true;
			if(isServerFlag)
				throw new Exception("NetScene cannot be utilized by both client and server services.");
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
			return isServerFlag;
		}

		public bool isClient() {
			return isClientFlag;
		}

		public void writeTo(IDataBuffer buffer) {
			throw new NotImplementedException();
		}

		public void readFrom(IDataBuffer buffer) {
			throw new NotImplementedException();
		}
	}
}
