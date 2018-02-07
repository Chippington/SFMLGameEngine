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
	public class GameWindow : ObjectBase {
		private string name;
		private bool exitFlag;
		private bool vsyncEnabled;
		private uint width, height;
		private Thread logicThread;
		private Thread graphicsThread;
		private RenderWindow window;
		public GameWindow() : this("") { }

		public GameWindow(string name) : this(name, 800, 600) { }

		public GameWindow(string name, uint width, uint height) {
			this.name = name;
			this.width = width;
			this.height = height;
			vsyncEnabled = false;
		}

		public void start() {
			log("Initializing Game Context...");
			Clock time = new Clock();
			InputController input = new InputController();
			SceneManager scenes = new SceneManager();
			ICollisionMap map = new SweepAndPrune();
			UIWindow uiwindow = new UIWindow("", width, height, UIWindow.Style.NONE);

			log("Initializing Game Threads...");
			logicThread = new Thread(() => {
				GameContext context = new GameContext();
				SFML.System.Clock clock = new SFML.System.Clock();
				uiwindow.onInitialize();

				context.sceneManager = scenes;
				context.input = input;
				context.ui = uiwindow;
				context.time = time;

				logicInitialized(context);

				clock.Restart();
				log("Logic initialized.");
				while (!exitFlag) {
					Thread.Sleep(1);
					var t = clock.Restart();
					context.deltaTime = ((float)t.AsMicroseconds()) / 1000000f;
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
				log(string.Format("Creating window... [Resolution: {0}x{1}]", width, height));
				window = new RenderWindow(new SFML.Window.VideoMode(width, height), name);

				log("Initializing UI Layer...");
				RenderTexture uilayer = new RenderTexture(width, height);
				Sprite uisprite = new Sprite(uilayer.Texture);
				uiwindow.onGraphicsInitialize();

				window.SetVerticalSyncEnabled(vsyncEnabled);
				window.Closed += (sender, args) => {
					var win = sender as RenderWindow;
					exitFlag = true;
					if (win != null)
						win.Close();
				};

				SFML.System.Clock clock = new SFML.System.Clock();
				GameContext context = new GameContext();
				context.sceneManager = scenes;
				context.uiLayer = uilayer;
				context.window = window;
				context.input = input;
				context.ui = uiwindow;
				context.time = clock;

				input.setHooks(window);
				graphicsInitialized(context);
				Statistics.initializeDebugDraw(context);
				log("Graphics initialized.");
				while (!exitFlag) {
					Thread.Sleep(1);
					var t = clock.Restart();
					context.deltaTime = ((float)t.AsMicroseconds()) / 1000000f;

					window.DispatchEvents();
					input.updateInput();

					window.Clear();
					uilayer.Clear(new Color(0,0,0,0));
					_graphicsUpdate(context);
					uisprite.Position = window.GetView().Center - (window.GetView().Size / 2);
					uilayer.Display();
					window.Draw(uisprite);
					window.Display();
				}
			});

			log("Starting threads...");
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
			context.sceneManager.onUpdate(context);
			context.ui.onUpdate(context);
		}

		private void _graphicsUpdate(GameContext context) {
			Statistics.onGraphicsUpdate();
			Statistics.debugDraw(context.uiLayer);

			context.sceneManager.onDraw(context);
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

		public SceneManager sceneManager;
		public InputController input;
		public float deltaTime;
		public Clock time;
	}
}
