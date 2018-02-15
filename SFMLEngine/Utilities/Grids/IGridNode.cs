using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Utilities.Grids {
	public interface IGridNode<T> {
		T getValue();
		void setValue(T val);
	}
}
