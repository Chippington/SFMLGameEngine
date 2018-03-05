using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Debug {
	public class TimedLogBlock : LogBlock {
		Func<TimeSpan, string> func;
		Stopwatch sw;

		public TimedLogBlock(Func<TimeSpan, string> func) : base("") {
			this.func = func;
			sw = new Stopwatch();
			sw.Start();
		}

		public override void Dispose() {
			sw.Stop();
			string str = "";
			if (func != null)
				str = func.Invoke(sw.Elapsed);

			Console.WriteLine(str);
		}
	}

	public class LogBlock : IDisposable {
		private string str;
		Func<string> strFunc;

		public LogBlock(string str) {
			this.str = str;
		}

		public LogBlock(Func<string> func) {
			strFunc = func;
		}

		public virtual void Dispose() {
			if (strFunc != null)
				str = strFunc.Invoke();

			Console.WriteLine(str);
		}
	}
}
