using SFMLEngine.Utilities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Utilities.Grids {
	public interface IGridBase<T> : IGameObject {
		void setSize(Dimension dim);
		IGridNode<T> getNode(Dimension dim);
		void setNode(IGridNode<T> node, Dimension dim);
	}
}
