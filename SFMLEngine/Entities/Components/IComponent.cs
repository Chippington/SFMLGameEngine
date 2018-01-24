using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities.Components {
	public interface IComponent {
		void onInitialize();
		void onUpdate(GameContext context);
		void onDraw(GameContext context);
		void onDestroy();
		void setEntity(IEntity owner);
		IEntity getEntity();
	}
}
