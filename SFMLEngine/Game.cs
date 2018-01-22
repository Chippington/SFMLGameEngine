using SFML.Graphics;
using SFMLEngine.Collision;
using SFMLEngine.Debug;
using SFMLEngine.Entities;
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

		public class TestEntity : Entities.Entity {
			public override void initialize() {
				base.initialize();
				addComponent<Entities.Components.RigidBody>();
			}
		}

		public Game(string name) {
			this.name = name;
		}

		public void start() {
			InputController input = new InputController();
			EntitySet set = new EntitySet();
			Collision.CollisionMap map = new CollisionMap(set);
			
			logicThread = new Thread(() => {
				Stopwatch sw = new Stopwatch();
				GameContext context = new GameContext();
				context.input = input;

				sw.Start();
				long last = 0;
				while (!exitFlag) {
					Thread.Sleep(1);
					context.deltaTime = ((float)(sw.ElapsedTicks - last)) / 10000f;
					last = sw.ElapsedTicks;

					input.updateInput();
					set.updateEntities();
					map.updateMap();

					logicUpdate(context);
				}
			});

			graphicsThread = new Thread(() => {
				GameContext context = new GameContext();
				context.input = input;

				RenderWindow window = new RenderWindow(new SFML.Window.VideoMode(800, 600), name);
				window.SetVerticalSyncEnabled(true);
				window.Closed += (sender, args) => {
					var win = sender as RenderWindow;
					exitFlag = true;
					if (win != null)
						win.Close();
				};

				input.setHooks(window);
				while (!exitFlag) {
					window.DispatchEvents();
					Thread.Sleep(1);

					window.Clear();
					graphicsUpdate(window, context);
					window.Display();
				}
			});

			logicThread.Start();
			graphicsThread.Start();
		}

		public bool isRunning() {
			return (logicThread.IsAlive || graphicsThread.IsAlive);
		}

		private void logicUpdate(GameContext context) {
			Statistics.onLogicUpdate();
		}

		private void graphicsUpdate(RenderWindow window, GameContext context) {
			Statistics.onGraphicsUpdate();
			Statistics.debugDraw(window);
		}
	}

	public class GameContext {
		public InputController input;
		public float deltaTime;
	}
}
