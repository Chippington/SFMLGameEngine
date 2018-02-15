using SFMLEngine.Utilities.Grids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Utilities.Tilesets {
	public interface ITilesetAdapter<T> : IGameObject, IGridEventListener<T> {
		void setGrid(IGridBase<T> grid);
		IGridBase<T> getGrid();
	}
}
