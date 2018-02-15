using SFMLEngine.Utilities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Utilities.Grids {
	public interface IGridBase<T> : IGameObject {
		void setSize(Dimension dim);
		Dimension getSize();
		IGridNode<T> getNode(Dimension dim);
		T getValue(Dimension dim);
		void setValue(Dimension dim, T value);
		void addListener(IGridEventListener<T> listener);
		void removeListener(IGridEventListener<T> listener);
	}
}
