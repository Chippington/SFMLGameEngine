using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Utilities.Grids {
	public interface IGridEventListener<T> {
		void onTileCreated(IGridNode<T> tile);
		void onTileDeleted(IGridNode<T> tile);
	}
}
