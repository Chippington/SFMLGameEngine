using SFMLEngine.Entities.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities {
	public interface IEntity {
		ComponentSet components { get; set; }
		void onInitialize();
		void onUpdate(GameContext context);
		void onDraw(GameContext context);
		void onDestroy();
		ComponentSet getComponents();
		void setOwner(EntitySet owner);
	}
}
