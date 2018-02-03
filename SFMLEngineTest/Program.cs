using SFMLEngine;
using SFMLEngine.Entities;
using SFMLEngine.Entities.Components;
using SFMLEngine.Entities.Components.Camera;
using SFMLEngine.Entities.Components.Common;
using SFMLEngine.Entities.Components.Physics;
using SFMLEngine.Entities.Graphics.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngineTest {
	class Program {
		public class TestGame : GameWindow {
			protected override void logicInitialized(GameContext context) {
				base.logicInitialized(context);
				for(int i = 0; i < 1; i++) 
					for(int j = 0; j < 1000; j++) {
					var test = context.entities.instantiate<TestEntity>(string.Format("Test[{0},{1}]", i, j));
					test.components.Get<Position>().x = 52 + (52 * i);
					test.components.Get<Position>().y = 52 + (52 * j);

					if(i == 0 || i == 99 || j == 0 || j == 99)
					test.components.Get<RigidBody>().setDebugDraw(true);
				}
				for (int i = 0; i < 1; i++) {
					var player = context.entities.instantiate<TestPlayer>();
					player.components.Get<Position>().x = i * 6;
				}
			}

			protected override void graphicsUpdate(GameContext context) {
				base.graphicsUpdate(context);
			}

			protected override void logicUpdate(GameContext context) {
				base.logicUpdate(context);
			}
		}

		public class TestPlayer : Entity {
			CameraComponent cam;
			Position pos;
			float vx, vy;

			public override void onInitialize() {
				base.onInitialize();
				var rb = components.Add<RigidBody>();
				rb.setBoundingBox(new BoundingBox(-16f, -16f, 16f, 16f));
				rb.setDebugDraw(true);
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

			public override void onUpdate(GameContext context) {
				base.onUpdate(context);
				vx = vy = 0;
				if(context.input.isHeld(SFML.Window.Keyboard.Key.W)) { vy -= 30f; }
				if(context.input.isHeld(SFML.Window.Keyboard.Key.A)) { vx -= 30f; }
				if(context.input.isHeld(SFML.Window.Keyboard.Key.S)) { vy += 30f; }
				if(context.input.isHeld(SFML.Window.Keyboard.Key.D)) { vx += 30f; }

				var col = components.Get<RigidBody>();
				//if (context.collision.testCollision<TestEntity>(
				//	col, pos.x + (vx * context.deltaTime), pos.y)) {
				//	vx = 0f;
				//}

				//if (context.collision.testCollision<TestEntity>(
				//	col, pos.x, pos.y + (vy * context.deltaTime))) {
				//	vy = 0f;
				//}

				pos.x += vx * context.deltaTime;
				pos.y += vy * context.deltaTime;

				var newPos = new SFML.System.Vector2f(pos.x, pos.y);
				var diff = newPos - cam.getPosition();
				diff *= 0.02f;
				cam.setPosition(cam.getPosition() + diff);
			}
		}

		public class TestEntity : Entity {
			private string name;
			Position position;

			public TestEntity(string name) {
				this.name = name;
			}

			public override void onInitialize() {
				var r = components.Add<RigidBody>();
				r.setBoundingBox(new BoundingBox() {
					left = 0f, right = 50f,
					top = 0f, bottom = 50f,
				});

				r.onCollisionEnter += onCollisionEnter;
				r.onCollisionLeave += onCollisionLeave;

				this.position = components.Add<Position>();
			}

			public override void onUpdate(GameContext context) {
				base.onUpdate(context);
				var y = position.y / 100f;
				position.x = (float)Math.Sin(y + ((float)context.time.ElapsedTime.AsMilliseconds()) / 1000f) * 100f;
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
			TestGame g = new TestGame();
			g.start();

			while(g.isRunning()) {
				System.Threading.Thread.Sleep(100);
			}
		}
	}
}
