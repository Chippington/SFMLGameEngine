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

		public int this[int index] {
			get {
				return values[index];
			}

			set {
				values[index] = value;
			}
		}

		public override string ToString() {
			if (values.Length == 0)
				return "[ ](0)";

			string ret = "";
			for(int i = 0; i < values.Length; i++) {
				ret += string.Format("{0}, ");
			}

			return string.Format("[{0}]({1})", 
				ret.Substring(0, ret.Length - 2), 
				values.Length);
		}
	}
}
