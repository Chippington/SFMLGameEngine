using NetUtils.Net.Interfaces;
using NetUtils.Utilities;
using SFMLEngine.Network.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Network {
	public class NetConfig : NetUtils.Net.NetConfig {
		private IDMap<Type> entityTypeMap;

		public NetConfig() : base() {
			entityTypeMap = new IDMap<Type>();
		}

		public ID registerNetEntity<T>() where T : INetEntity {
			return entityTypeMap.addObject(typeof(T));
		}

		/// <summary>
		/// Writes the ID for the given entity type to the buffer.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="svc"></param>
		public void writeEntityHeader(IDataBuffer buffer, INetEntity entity) {
			ID id = entityTypeMap.getObjectId(entity.GetType());
			entityTypeMap.writeIDHeader(id, buffer);
		}

		/// <summary>
		/// Reads the buffer for an ID and returns the corresponding entity type.
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		public INetEntity readEntityHeader(IDataBuffer buffer) {
			ID id = entityTypeMap.readIDHeader(buffer);
			var p = (INetEntity)Activator.CreateInstance(entityTypeMap.getObject(id));
			return p;
		}


	}
}
