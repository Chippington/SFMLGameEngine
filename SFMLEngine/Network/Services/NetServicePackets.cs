using NetUtils.Net.Data;
using NetUtils.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Network.Services {
	public class P_SceneChange : Packet {
		public IDataBuffer sceneData;
		public byte id;

		public P_SceneChange() {
			this.sceneData = null;
			this.id = 0;
		}

		public P_SceneChange(byte id, IDataBuffer sceneData) {
			this.sceneData = sceneData;
			this.id = id;
		}

		public override void writeTo(IDataBuffer buffer) {
			base.writeTo(buffer);
			buffer.write((byte)id);
			buffer.write(sceneData);
		}

		public override void readFrom(IDataBuffer buffer) {
			base.readFrom(buffer);
			id = buffer.readByte();
			sceneData = buffer.readBuffer();
		}
	}
}
