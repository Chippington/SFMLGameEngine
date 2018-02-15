using SFMLEngine.Utilities.Grids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Utilities.Tilesets {
	public interface ITilesetBase<T> : IGameObject, IRenderable, IUpdatable {
		void setRenderer(ITilesetRenderer<T> renderer);
		void setAdapter(ITilesetAdapter<T> adapter);
		void setGrid(IGridBase<T> grid);

		ITilesetRenderer<T> getRenderer();
		ITilesetAdapter<T> getAdapter();
		IGridBase<T> getGrid();
	}
}
