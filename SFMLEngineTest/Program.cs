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
		public class TestGame : Game {
			protected override void logicInitialized(GameContext context) {
				base.logicInitialized(context);
				for(int i = 0; i < 300; i++) {
					var test = context.entities.instantiate<TestEntity>("Test"+i);
					test.components.Get<Position>().x = 50 + (50 * i);
					test.components.Get<Position>().y = 50 + (50 * i);
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
				if(context.input.isHeld(SFML.Window.Keyboard.Key.W)) { vy -= 10f; }
				if(context.input.isHeld(SFML.Window.Keyboard.Key.A)) { vx -= 10f; }
				if(context.input.isHeld(SFML.Window.Keyboard.Key.S)) { vy += 10f; }
				if(context.input.isHeld(SFML.Window.Keyboard.Key.D)) { vx += 10f; }

				var col = components.Get<RigidBody>();
				if (context.collision.testCollision<TestEntity>(
					col, pos.x + (vx * context.deltaTime), pos.y)) {
					vx = 0f;
				}

				if (context.collision.testCollision<TestEntity>(
					col, pos.x, pos.y + (vy * context.deltaTime))) {
					vy = 0f;
				}

				pos.x += vx * context.deltaTime;
				pos.y += vy * context.deltaTime;
				cam.setPosition(new SFML.System.Vector2f(pos.x, pos.y));
			}
		}

		public class TestEntity : Entity {
			private string name;
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
