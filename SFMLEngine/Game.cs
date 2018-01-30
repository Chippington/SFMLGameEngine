using SFML.Graphics;
using SFMLEngine.Entities;
using SFMLEngine.Entities.Collision;
using SFMLEngine.Entities.Components;
using SFMLEngine.Graphics.UI;
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
			UIWindow uiwindow = new UIWindow("", 800, 600, UIWindow.Style.NONE);

			logicThread = new Thread(() => {
				GameContext context = new GameContext();
				SFML.System.Clock clock = new SFML.System.Clock();
				uiwindow.onInitialize();

				context.ui = uiwindow;
				context.input = input;
				context.entities = set;
				context.collision = map;
				logicInitialized(context);

				clock.Restart();
				while (!exitFlag) {
					Thread.Sleep(1);
					var t = clock.Restart();
					context.deltaTime = ((float)t.AsMicroseconds()) / 100000f;

					set.updateEntities(context);
					map.updateMap();
					_logicUpdate(context);
					uiwindow.onUpdate(context);
				}
			});

			graphicsThread = new Thread(() => {
				RenderWindow window = new RenderWindow(new SFML.Window.VideoMode(800, 600), name);
				RenderTexture uilayer = new RenderTexture(800, 600);
				uiwindow.onGraphicsInitialize();

				window.SetVerticalSyncEnabled(true);
				window.Closed += (sender, args) => {
					var win = sender as RenderWindow;
					exitFlag = true;
					if (win != null)
						win.Close();
				};

				Sprite uisprite = new Sprite(uilayer.Texture);

				SFML.System.Clock clock = new SFML.System.Clock();
				GameContext context = new GameContext();
				context.input = input;
				context.entities = set;
				context.window = window;
				context.collision = map;
				context.uiLayer = uilayer;
				context.ui = uiwindow;

				input.setHooks(window);
				graphicsInitialized(context);
				Statistics.initializeDebugDraw(context);
				while (!exitFlag) {
					Thread.Sleep(1);
					var t = clock.Restart();
					context.deltaTime = ((float)t.AsMicroseconds()) / 100000f;

					window.DispatchEvents();
					input.updateInput();

					window.Clear();
					uilayer.Clear(new Color(0,0,0,0));
					set.drawEntities(context);
					_graphicsUpdate(context);
					uiwindow.onDraw(context, uilayer);

					uisprite.Position = window.GetView().Center - (window.GetView().Size / 2);
					uilayer.Display();
					window.Draw(uisprite);
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
			Statistics.debugDraw(context.uiLayer);
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
		public RenderTarget window;
		public RenderTarget uiLayer;
		public UIWindow ui;
		public CollisionMap collision;
		public InputController input;
		public Scene entities;
		public float deltaTime;
	}
}
