using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine {
	public static class Statistics {
		private static Queue<long> logicFrameQueue;
		private static Queue<long> graphicsFrameQueue;
		private static long last;

		private static long lastTickTime;
		private static Text text;

		static Statistics() {
			logicFrameQueue = new Queue<long>();
			graphicsFrameQueue = new Queue<long>();

			lastTickTime = Environment.TickCount;
			text = new Text("", new Font("Resources/Fonts/MavenPro-Regular.ttf"));
			text.FillColor = new Color(255,255,255);
			text.Position = new SFML.System.Vector2f(5f, 5f);
		}

		public static void debugDraw(RenderWindow window) {
			text.DisplayedString = $"CPU: {logicFrameQueue.Count}{Environment.NewLine}GPU: {graphicsFrameQueue.Count}";
			window.Draw(text);
		}

		public static void onLogicUpdate() {
			logicFrameQueue.Enqueue(Environment.TickCount);

			while (logicFrameQueue.Count > 0 && logicFrameQueue.Peek() < Environment.TickCount - 1000)
				logicFrameQueue.Dequeue();

			while (graphicsFrameQueue.Count > 0 && graphicsFrameQueue.Peek() < Environment.TickCount - 1000)
				graphicsFrameQueue.Dequeue();
		}

		public static void onGraphicsUpdate() {
			graphicsFrameQueue.Enqueue(Environment.TickCount);
		}

		public static int getLogicFramesPerSecond() {
			return logicFrameQueue.Count;
		}

		public static int getGraphicsFramesPerSecond() {
			return graphicsFrameQueue.Count;
		}
	}
}
