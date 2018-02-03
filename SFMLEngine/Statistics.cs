using SFML.Graphics;
using SFML.System;
using SFMLEngine.Graphics.UI;
using SFMLEngine.Graphics.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine {
	public class StatisticsDebugWindow : UIWindow {
		private Label cpuLabel, gpuLabel;
		public StatisticsGraphControl graph;

		public StatisticsDebugWindow() : base("Debug Statistics", 300, 100, Style.DEFAULT) {
			cpuLabel = new Label();
			gpuLabel = new Label();
			graph = new StatisticsGraphControl(200, 
				new Vector2f(
					-10f + (float)getWidth(), 
					-10f + (float)getHeight()));

			cpuLabel.Position = new Vector2f(5f, 2f);
			gpuLabel.Position = new Vector2f(5f, 20f);
			cpuLabel.setFontSize(16);
			gpuLabel.setFontSize(16);
			cpuLabel.setDepth(1);
			gpuLabel.setDepth(1);

			graph.Position = new Vector2f(5f, 5f);
			graph.setDepth(0);
		}

		public override void onInitialize() {
			base.onInitialize();
			addControl(cpuLabel);
			addControl(gpuLabel);
			addControl(graph);
		}

		public void setCPURate(string rate) {
			cpuLabel.setText(rate);
		}

		public void setGPURate(string rate) {
			gpuLabel.setText(rate);
		}

		public override void drawControl(GameContext context, RenderTarget target) {
			base.drawControl(context, target);
		}
	}

	public class StatisticsGraphControl : UIControl {
		private List<KeyValuePair<long, float>> dbgLogicGraphQueue;
		private List<KeyValuePair<long, float>> dbgGraphicGraphQueue;

		private Vertex[] logicLines;
		private Vertex[] graphicLines;

		private int historyLength;
		private Vector2f size;

		public StatisticsGraphControl(int historyLength, Vector2f size) {
			dbgLogicGraphQueue = new List<KeyValuePair<long, float>>();
			dbgGraphicGraphQueue = new List<KeyValuePair<long, float>>();
			this.historyLength = historyLength;
			this.size = size;

			for(int i = 0; i < historyLength; i++) {
				addCPUPoint(0, 0f);
				addGPUPoint(0, 0f);
			}
		}

		public void addCPUPoint(long tick, float rate) {
			dbgLogicGraphQueue.Add(new KeyValuePair<long, float>(tick, rate));
			logicLines = new Vertex[dbgLogicGraphQueue.Count];

			for (int i = 0; i < logicLines.Length; i++) {
				float x = (size.X / historyLength) * ((float)i);
				float y = Math.Min(dbgLogicGraphQueue[i].Value, 1000f) / 1000f;
				y *= size.Y;

				logicLines[i].Position = Position + new Vector2f(x, size.Y - y);
				logicLines[i].Color = new Color(0, 255, 0, 255);
			}

			if (dbgLogicGraphQueue.Count > historyLength) dbgLogicGraphQueue.RemoveAt(0);
		}

		public void addGPUPoint(long tick, float rate) {
			dbgGraphicGraphQueue.Add(new KeyValuePair<long, float>(tick, rate));
			graphicLines = new Vertex[dbgGraphicGraphQueue.Count];

			for (int i = 0; i < graphicLines.Length; i++) {
				float x = (size.X / historyLength) * ((float)i);
				float y = Math.Min(dbgGraphicGraphQueue[i].Value, 1000f) / 1000f;
				y *= size.Y;

				graphicLines[i].Position = Position + new Vector2f(x, size.Y - y);
				graphicLines[i].Color = new Color(255, 0, 0, 255);
			}

			if (dbgGraphicGraphQueue.Count > historyLength) dbgGraphicGraphQueue.RemoveAt(0);
		}

		public override void drawControl(GameContext context, RenderTarget target) {
			base.drawControl(context, target);

			if (logicLines == null || graphicLines == null)
				return;

			if (logicLines.Length > 0)
				target.Draw(logicLines, PrimitiveType.LineStrip);
			if (graphicLines.Length > 0)
				target.Draw(graphicLines, PrimitiveType.LineStrip);
		}
	}

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
		private static StatisticsDebugWindow dbgWindow;

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

			dbgPosition = new Vector2f(5f, 5f);
			dbgSize = new Vector2f(500f, 300f);
			for(int i = 0; i < dbgGraphHistoryLength; i++) {
				dbgLogicGraphQueue.Add(new KeyValuePair<long, float>(0,0));
				dbgGraphicGraphQueue.Add(new KeyValuePair<long, float>(0,0));
			}

			dbgWindow = new StatisticsDebugWindow();
			dbgWindow.Position = new Vector2f(10f, 10f);
		}

		public static void initializeDebugDraw(GameContext context) {
			context.ui.addControl(dbgWindow);
		}

		public static void debugDraw(RenderTarget window) {
			if(Environment.TickCount - dbgGraphLastSample > dbgGraphSampleDelay) {
				dbgWindow.graph.addCPUPoint(Environment.TickCount, getLogicFramesPerSecond());
				dbgWindow.graph.addGPUPoint(Environment.TickCount, getGraphicsFramesPerSecond());
				dbgWindow.setCPURate($"CPU: {(int)getLogicFramesPerSecond()}");
				dbgWindow.setGPURate($"GPU: {(int)getGraphicsFramesPerSecond()}");

				dbgGraphLastSample = Environment.TickCount;
			}
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

			while (logicFrameQueue.Count > 0 && logicFrameQueue.Peek() < Environment.TickCount - 1000)
				logicFrameQueue.Dequeue();

			while (graphicsFrameQueue.Count > 0 && graphicsFrameQueue.Peek() < Environment.TickCount - 1000)
				graphicsFrameQueue.Dequeue();
		}

		public static float getLogicFramesPerSecond() {
			return ((float)logicFrameQueue.Count) / Math.Min(((float)Environment.TickCount - startTickTime) / 1000f, 1f);
		}

		public static float getGraphicsFramesPerSecond() {
			return ((float)graphicsFrameQueue.Count) / Math.Min(((float)Environment.TickCount - startTickTime) / 1000f, 1f);
		}
	}
}
