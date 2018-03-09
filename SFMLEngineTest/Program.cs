using NetUtils.Net.Data;
using NetUtils.Net.Default.Providers.TCP;
using NetUtils.Net.Interfaces;
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
using SFMLEngine.Network;
using SFMLEngine.Network.Entities;
using SFMLEngine.Network.Scenes;
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
		public static bool startClient = false;
		public class TestGame : GameWindow {

			public TestGame() : base("Test Client", 800, 600) {

			}

			bool isServer = false;
			public TestGame(bool server) : base() {
				isServer = true;
			}

			private NetConfig config;

			protected override void onRegisterServices(GameContext context) {
				base.onRegisterServices(context);
				config = new NetConfig() {
					appname = "Test",
					port = 12345,
					maxclients = 32,
					ipaddress = "127.0.0.1"
				};

				config.registerNetEntity<TestNetEntity>();
				config.registerPacket<TestNetEntityPacket>();

				if (isServer) {
					context.services.registerService<NetServerService>();
				} else {
					context.services.registerService<NetClientService>();
				}
			}

			protected override void onLogicInitialized(GameContext context) {
				base.onLogicInitialized(context);
				context.sceneManager.registerScene<TestNetScene>();
				context.sceneManager.setActiveScene<TestNetScene>();
				TestNetScene mainScene = context.sceneManager.getScene<TestNetScene>();

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
				if (isServer) startClient = true;

				if (isServer) {
					var sv = context.services.getService<NetServerService>();
					sv.startServer(config, new TCPNetworkProvider());
				} else {
					var cl = context.services.getService<NetClientService>();
					cl.startClient(config, new TCPNetworkProvider());
				}
			}

			protected override void onGraphicsUpdate(GameContext context) {
				base.onGraphicsUpdate(context);
			}

			protected override void onLogicUpdate(GameContext context) {
				base.onLogicUpdate(context);
			}
		}

		public class TestNetEntityPacket : Packet {
			public string data;
			public override void writeTo(IDataBuffer buffer) {
				base.writeTo(buffer);
				buffer.write((string)data);
			}

			public override void readFrom(IDataBuffer buffer) {
				base.readFrom(buffer);
				data = buffer.readString();
			}
		}

		public class TestNetEntity : NetEntity {
			public string data;

			public override void onNetInitialize(NetServiceBase netService, NetworkHandler netHandler) {
				base.onNetInitialize(netService, netHandler);
				//if (netHandler.isServer()) log("Test Entity initialized on SERVER");
				//if (netHandler.isClient()) log("Test Entity initialized on CLIENT");
				if (netHandler.isServer()) queuePacket(new PacketInfo() {
					sendToAll = true,
					packet = new TestNetEntityPacket() {
						data = (netHandler.isServer()) ? "CREATED ON SERVER" : "CREATED ON CLIENT",
					},
				});

				getPacketRouter().addClientPacketCallback<TestNetEntityPacket>(cbTestCallback);
			}

			private void cbTestCallback(TestNetEntityPacket obj) {
				log("Client says: " + obj.data);
				destroy();
			}

			public override void onDestroy() {
				base.onDestroy();
				if (isServer()) log("SV DESTROY");
				if (isClient()) log("CL DESTROY");
			}
		}

		public class TestNetScene : NetScene {
			public override void onNetInitialize(NetServiceBase netService, NetworkHandler netHandler) {
				base.onNetInitialize(netService, netHandler);

				//netHandler.onClientConnected += onClientConnected;
				onClientConnected(new NetEventArgs());
			}

			private void onClientConnected(NetEventArgs args) {
				//if (isServer()) return;
				//if (isClient()) return;

				//for (int i = 0; i < 10; i++)
					instantiate<TestNetEntity>();
			}

			public override void onSceneActivated() {
				base.onSceneActivated();
			}

			public override void writeTo(IDataBuffer buffer) {
				base.writeTo(buffer);
				buffer.write((string)"Hello!");
			}

			public override void readFrom(IDataBuffer buffer) {
				base.readFrom(buffer);
				var str = buffer.readString();

				log("SERVER SAYS: " + str);
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
		static TestGame sv, cl;
		static void Main(string[] args) {
			Console.WindowWidth = 160;
			Console.WindowHeight = 60;
			Console.BufferHeight = 120;
			sv = new TestGame(true);
			cl = new TestGame();
			sv.start();
			bool started = false;
			//cl.start();
			Stopwatch sw = new Stopwatch();
			sw.Start();
			while (sv.isRunning() || cl.isRunning()) {
				System.Threading.Thread.Sleep(100);
				if (started == false && startClient == true) {
					cl.start();
					started = true;
				}
			}
		}
	}
}
