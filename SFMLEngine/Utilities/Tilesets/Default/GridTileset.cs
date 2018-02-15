using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFMLEngine.Utilities.Grids;

namespace SFMLEngine.Utilities.Tilesets.Default {
	public class GridTileset<T> : ITilesetBase<T> {
		protected ITilesetRenderer<T> renderer;
		protected ITilesetAdapter<T> adapter;
		protected IGridBase<T> grid;
		protected GameContext context;

		public virtual void onInitialize(GameContext context) {
			this.context = context;
		}

		public virtual void onUpdate(GameContext context) {

		}

		public virtual void onDraw(GameContext context) {
			renderer?.onDraw(context);
		}

		public virtual ITilesetAdapter<T> getAdapter() {
			return adapter;
		}

		public virtual IGridBase<T> getGrid() {
			return grid;
		}

		public virtual ITilesetRenderer<T> getRenderer() {
			return renderer;
		}

		public virtual void setAdapter(ITilesetAdapter<T> adapter) {
			if (this.adapter != null) {
				if (this.grid != null)
					this.grid.removeListener(this.adapter);

				this.adapter.onDispose(context);
			}

			this.adapter = adapter;
			this.adapter.onInitialize(context);
			this.adapter.setGrid(getGrid());

			if (this.grid != null)
				this.grid.addListener(this.adapter);
		}

		public virtual void setGrid(IGridBase<T> grid) {
			if (this.grid != null)
				this.grid.onDispose(context);

			this.grid = grid;
			this.grid.onInitialize(context);

			if (this.adapter != null)
				this.grid.addListener(this.adapter);
		}

		public virtual void setRenderer(ITilesetRenderer<T> renderer) {
			if (this.renderer != null)
				this.renderer.onDispose(context);

			this.renderer = renderer;
			this.renderer.onInitialize(context);
			this.renderer.setGrid(getGrid());
		}

		public virtual void onDispose(GameContext context) {
			renderer.onDispose(context);
			adapter.onDispose(context);
			grid.onDispose(context);

			renderer = null;
			adapter = null;
			grid = null;
		}
	}
}
