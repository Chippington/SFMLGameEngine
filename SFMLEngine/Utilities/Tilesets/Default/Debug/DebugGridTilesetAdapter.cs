using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFMLEngine.Utilities.Grids;

namespace SFMLEngine.Utilities.Tilesets.Default.Debug {
	public class DebugGridTilesetAdapter<T> : ObjectBase, ITilesetAdapter<T> {
		private IGridBase<T> grid;

		public void onInitialize(GameContext context) {
			log("Initialized");
		}

		public IGridBase<T> getGrid() {
			return grid;
		}

		public void setGrid(IGridBase<T> grid) {
			this.grid = grid;
		}

		public void onTileCreated(IGridNode<T> tile) {
			log("Tile created at dim: " + tile.getDim().ToString());
		}

		public void onTileDeleted(IGridNode<T> tile) {
			log("Tile removed at dim: " + tile.getDim().ToString());
		}

		public void onDispose(GameContext context) {
			log("Disposed");
		}
	}
}
