using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Utilities.Data {
	public class Dimension {
		public int[] values;
		public int x => values[0];
		public int y => values[1];
		public int z => values[2];

		public Dimension(int size) {
			values = new int[size];
		}
	}
}
