using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFMLEngine.Utilities.Data;

namespace SFMLEngine.Utilities.Grids.Default {
	public class OctreeGrid<T> : IGridBase<T> {
		public void onInitialize(GameContext context) {
			throw new NotImplementedException();
		}

		public void addListener(IGridEventListener<T> listener) {
			throw new NotImplementedException();
		}
		public void removeListener(IGridEventListener<T> listener) {
			throw new NotImplementedException();
		}

		public IGridNode<T> getNode(Dimension dim) {
			throw new NotImplementedException();
		}

		public Dimension getSize() {
			throw new NotImplementedException();
		}

		public T getValue(Dimension dim) {
			throw new NotImplementedException();
		}

		public void setSize(Dimension dim) {
			throw new NotImplementedException();
		}

		public void setValue(Dimension dim, T value) {
			throw new NotImplementedException();
		}

		public void onDispose(GameContext context) {
			throw new NotImplementedException();
		}
	}
}
