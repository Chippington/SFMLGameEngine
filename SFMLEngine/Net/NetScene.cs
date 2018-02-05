using NetUtils.Net.Services.Entities;
using SFMLEngine.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Net {
	public class NetScene : Scene {
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

		public override void updateEntities(GameContext context) {
			base.updateEntities(context);
			for(int i = 0; i < netEntityList.Count; i++) {
				if (netEntityList[i].isClient()) netEntityList[i].onClientUpdate();
				if (netEntityList[i].isServer()) netEntityList[i].onServerUpdate();
			}
		}
	}
}
