using SFML.Graphics;
using SFML.System;
using SFMLEngine.Entities;
using SFMLEngine.Entities.Collision;
using SFMLEngine.Entities.Components;
using SFMLEngine.Graphics.UI;
using SFMLEngine.Scenes;
using SFMLEngine.Services;
using SFMLEngine.Services.Input;
using SFMLEngine.Services.Statistics;
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
		private bool logicInit;
		private bool graphicsInit;
		private bool vsyncEnabled;
		private uint width, height;
		private Thread logicThread;
		private Thread graphicsThread;
		private RenderWindow window;
		private GameContext context;

		public GameWindow() : this("") {
			this.name = "NONAME - Headless";
			graphicsInit = true;
		}

		public GameWindow(string name) {
			this.name = name;
			graphicsInit = true;
		}

		public GameWindow(string name, uint width, uint height) : this(name) {
			this.width = width;
			this.height = height;
			vsyncEnabled = false;
			logicInit = false;
			graphicsInit = false;
		}

		public GameWindow(GameContext context) {
			this.name = context.name;
			this.width = context.screenWidth;
			this.height = context.screenHeight;

			this.context = context;
			logicInit = false;
			graphicsInit = false;
		}

		public void start() {
			log("Initializing Game Context");
			if (context == null) {
				context = new GameContext();
				context.screenHeight = height;
				context.screenWidth = width;
				context.name = name;
			}

			Clock time = new Clock();
			context.clock = time;

			log("Creating services");
			ServiceManager services = new ServiceManager();
			context.services = services;
			context.services.onInitialize(context);

			onRegisterServices(context);

			if (context.services.hasService<ISceneManager>() == false)
				context.services.registerService<SceneManager>();

			if (context.services.hasService<InputController>() == false)
				context.services.registerService<InputController>();

			if (context.services.hasService<StatisticsService>() == false)
				context.services.registerService<StatisticsService>();

			log("Creating window");
			UIWindow uiwindow = new UIWindow("", context.screenWidth, context.screenHeight, UIWindow.Style.NONE);
			context.ui = uiwindow;

			log("Game context initialized");
			log("Initializing Game Threads");
			logicThread = new Thread(() => {
				log("Creating logic context");
				Clock lgcClock = new Clock();
				uiwindow.onInitialize();

				log("Initializing services");
				services.onInitialize(context);
				onLogicInitialized(context);

				log("Logic initialized");
				logicInit = true;

				while (graphicsInit == false)
					Thread.Sleep(10);

				context.lockContext();
				lgcClock.Restart();
				while (!exitFlag) {
					Thread.Sleep(1);
					var t = lgcClock.Restart();
					context.time = new GameContext.Time();
					context.time.delta = ((double)t.AsMicroseconds()) / 1000000f;
					context.time.seconds = context.clock.ElapsedTime.AsSeconds();
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

			if (graphicsInit == true) {
				log("Starting in HEADLESS MODE");
				log("Starting threads");
				logicThread.Start();
				return;
			}

			graphicsThread = new Thread(() => {
				log("Waiting for logic thread");
				while (logicInit == false) System.Threading.Thread.Sleep(10);

				log(string.Format("Creating window [Resolution: {0}x{1}]", context.screenWidth, context.screenHeight));
				window = new RenderWindow(new SFML.Window.VideoMode(context.screenWidth, context.screenHeight), name);

				log("Initializing UI Layer");
				RenderTexture uilayer = new RenderTexture(context.screenWidth, context.screenHeight);
				Sprite uisprite = new Sprite(uilayer.Texture);
				uiwindow.onGraphicsInitialize();

				window.SetVerticalSyncEnabled(vsyncEnabled);
				window.Closed += (sender, args) => {
					var win = sender as RenderWindow;
					exitFlag = true;
					if (win != null)
						win.Close();
				};

				log("Creating graphics context");
				Clock gpuClock = new Clock();
				context.uiLayer = uilayer;
				context.window = window;

				log("Creating input hooks");
				context.input.setHooks(window);
				onGraphicsInitialized(context);
				context.services.getService<StatisticsService>()
					.initializeDebugDraw(context);

				log("Graphics initialized");
				graphicsInit = true;

				gpuClock.Restart();
				while (!exitFlag) {
					Thread.Sleep(1);
					var t = gpuClock.Restart();
					context.time = new GameContext.Time();
					context.time.delta = ((double)t.AsMicroseconds()) / 1000000f;
					context.time.seconds = context.clock.ElapsedTime.AsSeconds();

					window.DispatchEvents();
					window.Clear();
					uilayer.Clear(new Color(0,0,0,0));
					_graphicsUpdate(context);
					uisprite.Position = window.GetView().Center - (window.GetView().Size / 2);
					uilayer.Display();
					window.Draw(uisprite);
					window.Display();
				}
			});

			log("Starting threads");
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
			onLogicUpdate(context);

			context.services.onUpdate(context);
			context.ui.onUpdate(context);
		}

		private void _graphicsUpdate(GameContext context) {
			context.services.onDraw(context);
			context.ui.onDraw(context, context.uiLayer);
			onGraphicsUpdate(context);
		}

		protected virtual void onRegisterServices(GameContext context) { }

		protected virtual void onLogicInitialized(GameContext context) { }

		protected virtual void onGraphicsInitialized(GameContext context) { }

		protected virtual void onLogicUpdate(GameContext context) { }

		protected virtual void onGraphicsUpdate(GameContext context) { }
	}

	public class GameContext : ObjectBase {
		internal bool locked = false;
		internal void lockContext() {
			locked = true;
		}

		public string name { get; internal set; }
		public uint screenWidth { get; internal set; }
		public uint screenHeight { get; internal set; }

		public class Time {
			public float seconds;
			public double delta;
		}

		public RenderTarget window { get; internal set; }
		public RenderTarget uiLayer { get; internal set; }
		public UIWindow ui { get; internal set; }

		public ServiceManager services { get; internal set; }
		public Clock clock { get; internal set; }
		public Time time { get; internal set; }

		public ISceneManager sceneManager => services.getService<ISceneManager>();
		public InputController input => services.getService<InputController>();
	}
}
