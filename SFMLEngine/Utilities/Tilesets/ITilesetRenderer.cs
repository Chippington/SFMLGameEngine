﻿using SFMLEngine.Utilities.Grids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Utilities.Tilesets {
	public interface ITilesetRenderer<T> : IGameObject, IRenderable {
		void setGrid(IGridBase<T> grid);
		IGridBase<T> getGrid();
	}
}
