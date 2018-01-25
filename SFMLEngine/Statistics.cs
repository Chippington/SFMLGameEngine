using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine {
	public static class Statistics {
		private static Queue<long> logicFrameQueue;
		private static Queue<long> graphicsFrameQueue;
		private static List<KeyValuePair<long, float>> dbgLogicGraphQueue;
		private static List<KeyValuePair<long, float>> dbgGraphicGraphQueue;
		private static long dbgGraphSampleDelay;
		private static long dbgGraphLastSample;
		private static int dbgGraphHistoryLength;
		private static Vector2f dbgPosition;
		private static Vector2f dbgSize;

		private static long startTickTime;
		private static long lastTickTime;
		private static Text text;

		static Statistics() {
			logicFrameQueue = new Queue<long>();
			graphicsFrameQueue = new Queue<long>();

			dbgLogicGraphQueue = new List<KeyValuePair<long, float>>();
			dbgGraphicGraphQueue = new List<KeyValuePair<long, float>>();
			dbgGraphLastSample = Environment.TickCount;
			dbgGraphHistoryLength = 500;
			dbgGraphSampleDelay = 25;

			startTickTime = Environment.TickCount;
			lastTickTime = Environment.TickCount;
			text = new Text("", new Font("Resources/Fonts/MavenPro-Regular.ttf"));
			text.FillColor = new Color(255,255,255);
			text.Position = new SFML.System.Vector2f(5f, 5f);

			dbgPosition = new Vector2f(5f, 5f);
			dbgSize = new Vector2f(500f, 300f);
			for(int i = 0; i < dbgGraphHistoryLength; i++) {
				dbgLogicGraphQueue.Add(new KeyValuePair<long, float>(0,0));
				dbgGraphicGraphQueue.Add(new KeyValuePair<long, float>(0,0));
			}
		}

		private static Vertex[] logicLines;
		private static Vertex[] graphicLines;
		public static void debugDraw(RenderWindow window) {
			text.DisplayedString = $"CPU: {(int)getLogicFramesPerSecond()}{Environment.NewLine}GPU: {(int)getGraphicsFramesPerSecond()}";
			if(Environment.TickCount - dbgGraphLastSample > dbgGraphSampleDelay) {
				dbgLogicGraphQueue.Add(new KeyValuePair<long, float>(Environment.TickCount, getLogicFramesPerSecond()));
				dbgGraphicGraphQueue.Add(new KeyValuePair<long, float>(Environment.TickCount, getGraphicsFramesPerSecond()));

				if (dbgLogicGraphQueue.Count > dbgGraphHistoryLength) dbgLogicGraphQueue.RemoveAt(0);
				if (dbgGraphicGraphQueue.Count > dbgGraphHistoryLength) dbgGraphicGraphQueue.RemoveAt(0);
				dbgGraphLastSample = Environment.TickCount;

				logicLines = new Vertex[dbgLogicGraphQueue.Count];
				graphicLines = new Vertex[dbgGraphicGraphQueue.Count];

				for (int i = 0; i < logicLines.Length; i++) {
					float x = (dbgSize.X / dbgGraphHistoryLength) * ((float)i);
					float y = Math.Min(dbgLogicGraphQueue[i].Value, 1000f) / 1000f;
					y *= dbgSize.Y;

					logicLines[i].Position = dbgPosition + new Vector2f(x, dbgSize.Y - y);
					logicLines[i].Color = new Color(0, 255, 0, 255);
				}

				for (int i = 0; i < graphicLines.Length; i++) {
					float x = (dbgSize.X / dbgGraphHistoryLength) * ((float)i);
					float y = Math.Min(dbgGraphicGraphQueue[i].Value, 1000f) / 1000f;
					y *= dbgSize.Y;

					graphicLines[i].Position = dbgPosition + new Vector2f(x, dbgSize.Y - y);
					graphicLines[i].Color = new Color(255, 0, 0, 255);
				}
			}

			window.Draw(text);
			if (logicLines.Length > 0)
				window.Draw(logicLines, PrimitiveType.LineStrip);
			if (graphicLines.Length > 0)
				window.Draw(graphicLines, PrimitiveType.LineStrip);
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

		public static float getLogicFramesPerSecond() {
			return ((float)logicFrameQueue.Count) / Math.Min(((float)Environment.TickCount - startTickTime) / 1000f, 1f);
		}

		public static float getGraphicsFramesPerSecond() {
			return ((float)graphicsFrameQueue.Count) / Math.Min(((float)Environment.TickCount - startTickTime) / 1000f, 1f);
		}
	}
}
