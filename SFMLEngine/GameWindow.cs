using SFML.Graphics;
using SFML.System;
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
	public class GameWindow {
		private string name;
		private bool exitFlag;
		private bool vsyncEnabled;
		private Thread logicThread;
		private Thread graphicsThread;
		private RenderWindow window;
		public GameWindow() : this("") {
		}

		public GameWindow(string name) {
			this.name = name;
			vsyncEnabled = false;
		}

		public void start() {
			Clock time = new Clock();
			InputController input = new InputController();
			Scene set = new Scene();
			ICollisionMap map = new SweepAndPrune();
			UIWindow uiwindow = new UIWindow("", 800, 600, UIWindow.Style.NONE);

			logicThread = new Thread(() => {
				GameContext context = new GameContext();
				SFML.System.Clock clock = new SFML.System.Clock();
				uiwindow.onInitialize();

				context.ui = uiwindow;
				context.input = input;
				context.entities = set;
				context.collision = map;
				context.time = time;

				if (context.entities != null && context.collision != null) {
					context.entities.OnEntityCreated += context.collision.onEntityCreated;
					context.entities.OnEntityDestroyed += context.collision.onEntityCreated;
				}

				logicInitialized(context);

				clock.Restart();
				while (!exitFlag) {
					//Thread.Sleep(1);
					var t = clock.Restart();
					context.deltaTime = ((float)t.AsMicroseconds()) / 100000f;
					_logicUpdate(context);

					var pressedKeys = context.input.getPressedKeyboardKeys().ToList();
					foreach (var key in pressedKeys)
						if (uiwindow.handleKeyboardInput(key, context))
							context.input.resetFlags(key);

					var pressedButtons = context.input.getPressedMouseButtons().ToList();
					foreach (var btn in pressedButtons)
						if (uiwindow.handleMouseInputPressed(btn, context))
							context.input.resetFlags(btn);

					var releasedButtons = context.input.getReleasedMouseButtons().ToList();
					foreach (var btn in releasedButtons)
						if (uiwindow.handleMouseInputReleased(btn, context))
							context.input.resetFlags(btn);

					uiwindow.onUpdate(context);
				}
			});

			graphicsThread = new Thread(() => {
				window = new RenderWindow(new SFML.Window.VideoMode(800, 600), name);
				RenderTexture uilayer = new RenderTexture(800, 600);
				uiwindow.onGraphicsInitialize();

				window.SetVerticalSyncEnabled(vsyncEnabled);
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
				context.uiLayer = uilayer;
				context.ui = uiwindow;
				context.collision = map;
				context.time = time;

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
					_graphicsUpdate(context);
					//context.collision.drawCollisionMap(context);
					uisprite.Position = window.GetView().Center - (window.GetView().Size / 2);
					uilayer.Display();
					window.Draw(uisprite);
					window.Display();
				}
			});

			logicThread.Start();
			graphicsThread.Start();
		}

		public void setVerticalSyncEnabled(bool v) {
			vsyncEnabled = v;
			if (window != null)
				window.SetVerticalSyncEnabled(v);
		}

		public bool isRunning() {
			return (logicThread.IsAlive || graphicsThread.IsAlive);
		}

		private void _logicUpdate(GameContext context) {
			Statistics.onLogicUpdate();
			logicUpdate(context);
			context.entities.updateEntities(context);

			if(context.collision != null)
				context.collision.updateCollisionMap(context);

			context.ui.onUpdate(context);
		}

		private void _graphicsUpdate(GameContext context) {
			Statistics.onGraphicsUpdate();
			Statistics.debugDraw(context.uiLayer);

			context.entities.drawEntities(context);
			context.ui.onDraw(context, context.uiLayer);
			graphicsUpdate(context);
		}

		protected virtual void logicInitialized(GameContext context) { }

		protected virtual void graphicsInitialized(GameContext context) { }

		protected virtual void logicUpdate(GameContext context) { }

		protected virtual void graphicsUpdate(GameContext context) { }
	}

	public class GameContext {
		public RenderTarget window;
		public RenderTarget uiLayer;
		public UIWindow ui;
		public ICollisionMap collision;
		public InputController input;
		public Scene entities;
		public float deltaTime;
		public Clock time;
	}
}
