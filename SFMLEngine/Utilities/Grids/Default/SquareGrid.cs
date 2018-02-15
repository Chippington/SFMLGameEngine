using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFMLEngine.Utilities.Data;
using SFML.System;

namespace SFMLEngine.Utilities.Grids.Default {
	public class SquareGrid<T> : IGridBase<T> {
		public class SquareGridNode : IGridNode<T> {
			public SquareGridNode up, down, left, right;
			public Dimension dim;
			public Vector2i pos;

			private T value;

			public SquareGridNode(int x, int y) {
				this.dim = new Dimension(2);
				dim[0] = x;
				dim[1] = y;

				this.pos = new Vector2i(x, y);
			}

			public Dimension getDim() {
				return dim;
			}

			public T getValue() {
				return value;
			}

			public void setValue(T val) {
				value = val;
			}
		}

		private List<IGridEventListener<T>> listeners;
		private SquareGridNode[,] arr;
		private Dimension size;

		public void onInitialize(GameContext context) {
			listeners = new List<IGridEventListener<T>>();
			arr = new SquareGridNode[0, 0];
		}

		public IGridNode<T> getNode(Dimension dim) {
			return arr[dim[0], dim[1]];
		}

		public IGridNode<T> getNode(Vector2i pos) {
			return arr[pos.X, pos.Y];
		}

		public Dimension getSize() {
			return size;
		}

		public void setSize(Dimension dim) {
			setSize(new Vector2i(dim[0], dim[1]));
		}

		public void setSize(Vector2i size) {
			arr = new SquareGridNode[size.X, size.Y];
			for(int i = 0; i < size.X; i++) {
				for(int j = 0; j < size.Y; j++) {
					arr[i, j] = new SquareGridNode(i, j);
					arr[i, j].dim = new Dimension(2);
					arr[i, j].dim[0] = i;
					arr[i, j].dim[1] = j;
				}
			}

			this.size = new Dimension(2);
			this.size[0] = size.X;
			this.size[1] = size.Y;
		}

		public T getValue(Dimension dim) {
			return arr[dim[0], dim[1]].getValue();
		}

		public void setValue(Dimension dim, T value) {
			arr[dim[0], dim[1]].setValue(value);
		}

		public void onDispose(GameContext context) {
			this.size = null;
			this.arr = null;

			listeners.Clear();
			listeners = null;
		}

		public void addListener(IGridEventListener<T> listener) {
			listeners.Add(listener);
		}

		public void removeListener(IGridEventListener<T> listener) {
			listeners.Remove(listener);
		}
	}
}
