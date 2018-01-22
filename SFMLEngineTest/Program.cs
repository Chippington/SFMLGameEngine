using SFMLEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngineTest {
	class Program {
		static void Main(string[] args) {
			Game g = new Game("Test game");
			g.start();

			while(g.isRunning()) {
				System.Threading.Thread.Sleep(100);
			}
		}
	}
}
