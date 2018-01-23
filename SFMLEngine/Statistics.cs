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

		private static long startTickTime;
		private static long lastTickTime;
		private static Text text;

		static Statistics() {
			logicFrameQueue = new Queue<long>();
			graphicsFrameQueue = new Queue<long>();

			startTickTime = Environment.TickCount;
			lastTickTime = Environment.TickCount;
			text = new Text("", new Font("Resources/Fonts/MavenPro-Regular.ttf"));
			text.FillColor = new Color(255,255,255);
			text.Position = new SFML.System.Vector2f(5f, 5f);
		}

		public static void debugDraw(RenderWindow window) {
			text.DisplayedString = $"CPU: {(int)getLogicFramesPerSecond()}{Environment.NewLine}GPU: {(int)getGraphicsFramesPerSecond()}";
			window.Draw(text);
		}

		public static void onLogicUpdate() {
			logicFrameQueue.Enqueue(Environment.TickCount);

			while (logicFrameQueue.Count > 0 && logicFrameQueue.Peek() < Environment.TickCount - 5000)
				logicFrameQueue.Dequeue();

			while (graphicsFrameQueue.Count > 0 && graphicsFrameQueue.Peek() < Environment.TickCount - 5000)
				graphicsFrameQueue.Dequeue();
		}

		public static void onGraphicsUpdate() {
			graphicsFrameQueue.Enqueue(Environment.TickCount);
		}

		public static float getLogicFramesPerSecond() {
			return ((float)logicFrameQueue.Count) / Math.Min(((float)Environment.TickCount - startTickTime) / 1000f, 5f);
		}

		public static float getGraphicsFramesPerSecond() {
			return ((float)graphicsFrameQueue.Count) / Math.Min(((float)Environment.TickCount - startTickTime) / 1000f, 5f);
		}
	}
}
