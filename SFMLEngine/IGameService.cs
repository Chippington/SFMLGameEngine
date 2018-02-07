using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine {
	public interface IGameService {
		void initializeService(GameContext context);
		void updateService(GameContext context);
		void drawService(GameContext context);
	}
}
