using SFML.Graphics;
using SFMLEngine.Entities;
using SFMLEngine.Entities.Collision;
using SFMLEngine.Entities.Components;
using SFMLEngine.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFMLEngine {
	public class Game {
		private string name;
		private bool exitFlag;
		private Thread logicThread;
		private Thread graphicsThread;

		public Game() : this("") {
		}

		public Game(string name) {
			this.name = name;
		}

		public void start() {
			InputController input = new InputController();
			Scene set = new Scene();
			CollisionMap map = new CollisionMap(set);
			
			logicThread = new Thread(() => {
				Stopwatch sw = new Stopwatch();
				GameContext context = new GameContext();
				context.input = input;
				context.entities = set;
				context.collision = map;
				logicInitialized(context);

				sw.Start();
				long last = 0;
				while (!exitFlag) {
					Thread.Sleep(1);
					context.deltaTime = ((float)(sw.ElapsedTicks - last)) / 10000f;
					last = sw.ElapsedTicks;

					set.updateEntities(context);
					map.updateMap();

					_logicUpdate(context);
				}
			});

			graphicsThread = new Thread(() => {
				RenderWindow window = new RenderWindow(new SFML.Window.VideoMode(800, 600), name);
				//window.SetVerticalSyncEnabled(true);
				window.Closed += (sender, args) => {
					var win = sender as RenderWindow;
					exitFlag = true;
					if (win != null)
						win.Close();
				};
				GameContext context = new GameContext();
				context.input = input;
				context.window = window;
				context.entities = set;
				context.collision = map;

				input.setHooks(window);
				graphicsInitialized(context);
				while (!exitFlag) {
					window.DispatchEvents();
					input.updateInput();
					Thread.Sleep(1);

					window.Clear();
					set.drawEntities(context);
					_graphicsUpdate(context);
					window.Display();
				}
			});

			logicThread.Start();
			graphicsThread.Start();
		}

		public bool isRunning() {
			return (logicThread.IsAlive || graphicsThread.IsAlive);
		}

		private void _logicUpdate(GameContext context) {
			Statistics.onLogicUpdate();
			logicUpdate(context);
		}

		private void _graphicsUpdate(GameContext context) {
			Statistics.onGraphicsUpdate();
			Statistics.debugDraw(context.window);
			graphicsUpdate(context);
		}

		protected virtual void logicInitialized(GameContext context) {

		}

		protected virtual void graphicsInitialized(GameContext context) {

		}

		protected virtual void logicUpdate(GameContext context) {

		}

		protected virtual void graphicsUpdate(GameContext context) {

		}
	}

	public class GameContext {
		public RenderWindow window;
		public CollisionMap collision;
		public InputController input;
		public Scene entities;
		public float deltaTime;
	}
}
