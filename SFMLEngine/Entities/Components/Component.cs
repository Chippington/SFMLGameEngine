using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities.Components {
	public class Component {
		private Entity _entity;
		public Entity entity {
			get { return _entity; }
			set {
				if (_entity == null)
					_entity = value;
			}
		}

		public virtual void onInitialize() { }
		public virtual void onDestroy() { }
	}
}
