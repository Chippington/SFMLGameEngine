using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Debug {
	public interface ILogger {
		void log(string message);
		void log(string message, object sender);
		void log(string message, string sender);
	}
}
