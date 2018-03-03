using NetUtils.Net.Default.Providers.TCP;
using SFML.Graphics;
using SFML.System;
using SFMLEngine;
using SFMLEngine.Entities;
using SFMLEngine.Entities.Components;
using SFMLEngine.Entities.Components.Camera;
using SFMLEngine.Entities.Components.Common;
using SFMLEngine.Entities.Components.Physics;
using SFMLEngine.Entities.Graphics.Components;
using SFMLEngine.Graphics.UI;
using SFMLEngine.Graphics.UI.Controls;
using SFMLEngine.Network.Services;
using SFMLEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngineTest {
	class Program {
		public class TestGame : GameWindow {
			protected override void onLogicInitialized(GameContext context) {
				base.onLogicInitialized(context);
				context.sceneManager.registerScene<Scene>();
				context.sceneManager.setActiveScene<Scene>();
				Scene mainScene = context.sceneManager.getScene<Scene>();

				var sv = context.services.registerService<NetServerService>();
				var cl = context.services.registerService<NetClientService>();

				sv.startServer(new NetUtils.Net.NetConfig() {
					appname = "Test",
					port = 12345,
					maxclients = 32,
				}, new TCPNetworkProvider());

				cl.startClient((new NetUtils.Net.NetConfig() {
					appname = "Test",
					port = 12345,
					ipaddress = "127.0.0.1"
				}), new TCPNetworkProvider());

				for (int i = 0; i < 1; i++) {
					for (int j = 0; j < 2000; j++) {
						var test = mainScene.instantiate<TestEntity>(string.Format("Test[{0},{1}]", i, j));
						test.components.Get<Position>().x = 50 + (10 * i);
						test.components.Get<Position>().y = 50 + (10 * j);

						if (i == 0 || i == 99 || j == 0 || j == 99)
							test.components.Get<RigidBody>().setDebugDraw(true);
					}
				}

				for (int i = 0; i < 1; i++) {
					var player = mainScene.instantiate<TestPlayer>();
					player.components.Get<Position>().x = i * 6;
				}

				TestUIWindow uitest = new TestUIWindow();
				uitest.Position = new SFML.System.Vector2f(5f, 300f);

				context.ui.addControl(uitest);
			}

			protected override void onGraphicsUpdate(GameContext context) {
				base.onGraphicsUpdate(context);
			}

			protected override void onLogicUpdate(GameContext context) {
				base.onLogicUpdate(context);
			}
		}

		public class TestServer : NetServerService {
		}

		public class TestUIWindow : UIWindow {
			public TestUIWindow() : base("", 175, 80, Style.DEFAULT) {

			}

			public override void onGraphicsInitialize() {
				base.onGraphicsInitialize();
				setClearColor(new Color(255, 255, 255, 122));

				Textbox textbox = new Textbox("", "placeholder", new SFML.System.Vector2f(150f, 30f), new SFML.System.Vector2f(5f, 5f));
				textbox.Position = new SFML.System.Vector2f(15f, 15f);
				addControl(textbox);
			}
		}

		public class TestPlayer : Entity {
			RigidBody rigidbody;
			CameraComponent cam;
			Position pos;
			float vx, vy;
			public override void onInitialize(GameContext context) {
				base.onInitialize(context);
				var rb = components.Add<RigidBody>();
				rb.setBoundingBox(new BoundingBox(-16f, -16f, 16f, 16f));
				rb.setDebugDraw(true);
				rb.onCollisionStep += onCollisionStep;
				rigidbody = rb;
				pos = components.Add<Position>();

				var tex = new SFML.Graphics.Texture("Resources/Sprites/sprite.png");
				var spr = new SFML.Graphics.Sprite(tex);
				var sr = components.Add<SpriteRenderer>();
				sr.setOrigin(new SFML.System.Vector2f(16f, 16f));
				sr.setSprite(spr);

				cam = components.Add<CameraComponent>();
				cam.setSize(new SFML.System.Vector2f(800f, 600f));
				cam.setViewport(new SFML.Graphics.FloatRect(0f, 0f, 1f, 1f));

				owner.setCamera(cam);
			}

			private void onCollisionStep(CollisionEventArgs args) {
				var other = args.other.getBoundingBox();
				var bounds = rigidbody.getBoundingBox();
				var c1 = bounds.center;
				var c2 = other.center;

				float xdiff1 = bounds.max.X - other.min.X;
				float xdiff2 = bounds.min.X - other.max.X;
				float ydiff1 = bounds.max.Y - other.min.Y;
				float ydiff2 = bounds.min.Y - other.max.Y;

				Vector2f disp = new Vector2f(xdiff1, ydiff1);
				if (Math.Abs(xdiff2) < Math.Abs(xdiff1)) disp.X = xdiff2;
				if (Math.Abs(ydiff2) < Math.Abs(ydiff1)) disp.Y = ydiff2;
				if (Math.Abs(disp.X) < Math.Abs(disp.Y)) disp.Y = 0f; else disp.X = 0f;
				pos.x -= disp.X;
				pos.y -= disp.Y;
			}

			public override void onDraw(GameContext context) {
				base.onDraw(context);
			}

			public override void onUpdate(GameContext context) {
				base.onUpdate(context);
				if(context.input.isHeld(SFML.Window.Keyboard.Key.W)) { vy -= 200f; }
				if(context.input.isHeld(SFML.Window.Keyboard.Key.A)) { vx -= 200f; }
				if(context.input.isHeld(SFML.Window.Keyboard.Key.S)) { vy += 200f; }
				if(context.input.isHeld(SFML.Window.Keyboard.Key.D)) { vx += 200f; }

				var col = components.Get<RigidBody>();
				//if (context.collision.testCollision<TestEntity>(
				//	col, pos.x + (vx * context.deltaTime), pos.y)) {
				//	vx = 0f;
				//}

				//if (context.collision.testCollision<TestEntity>(
				//	col, pos.x, pos.y + (vy * context.deltaTime))) {
				//	vy = 0f;
				//}

				pos.x += vx * (float)context.time.delta;
				pos.y += vy * (float)context.time.delta;

				var newPos = new SFML.System.Vector2f(pos.x, pos.y);
				var diff = newPos - cam.getPosition();
				diff *= 0.02f;
				cam.setPosition(cam.getPosition() + diff);
				vx = vy = 0;
			}
		}

		public class TestEntity : Entity {
			private string name;
			Position position;

			public TestEntity(string name) {
				this.name = name;
			}

			public override void onInitialize(GameContext context) {
				var r = components.Add<RigidBody>();
				r.setBoundingBox(new BoundingBox() {
					left = 0f, right = 10f,
					top = 0f, bottom = 10f,
				});

				r.setIgnoreCallbacks(true);
				//r.onCollisionEnter += onCollisionEnter;
				//r.onCollisionLeave += onCollisionLeave;

				this.position = components.Add<Position>();
			}

			public override void onUpdate(GameContext context) {
				base.onUpdate(context);
				var y = position.y / 100f;
				position.x = (float)Math.Sin(y + ((float)context.clock.ElapsedTime.AsMilliseconds()) / 1000f) * 100f;
			}

			private void onCollisionLeave(CollisionEventArgs args) {
				var other = args.other.getEntity() as TestEntity;
				if (other != null) {
					Console.WriteLine($"({name}) Collision left with: " + other.name);
				}
			}

			private void onCollisionEnter(CollisionEventArgs args) {
				var other = args.other.getEntity() as TestEntity;
				if(other != null) {
					Console.WriteLine($"({name}) Collision enter with: " + other.name);
				}

				if(name == "Test")
				components.Get<Position>().x = 1000f;
			}
		}

		static void Main(string[] args) {
			Console.WindowWidth = 120;
			Console.WindowHeight = 60;
			Console.BufferHeight = 60;
			TestGame g = new TestGame();
			g.start();

			while (g.isRunning()) {
				System.Threading.Thread.Sleep(100);
			}
		}
	}
}
