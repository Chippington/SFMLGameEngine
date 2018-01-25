using SFMLEngine;
using SFMLEngine.Entities;
using SFMLEngine.Entities.Components;
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
					test.components.Get<Transform>().x = 50 + (50 * i);
					test.components.Get<Transform>().y = 50 + (50 * i);
					test.components.Get<RigidBody>().setDebugDraw(true);
				}
				for (int i = 0; i < 100; i++) {
					var player = context.entities.instantiate<TestPlayer>();
					player.components.Get<Transform>().x = i * 6;
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
			Transform pos;
			float vx, vy;

			public TestPlayer() {
				var rb = components.Add<RigidBody>();
				rb.setBoundingBox(new BoundingBox(-5f, -5f, 5f, 5f));
				rb.setDebugDraw(true);
				pos = components.Add<Transform>();
			}

			public override void onUpdate(GameContext context) {
				base.onUpdate(context);
				vx = vy = 0;
				if(context.input.isHeld(SFML.Window.Keyboard.Key.W)) { vy -= 1f; }
				if(context.input.isHeld(SFML.Window.Keyboard.Key.A)) { vx -= 1f; }
				if(context.input.isHeld(SFML.Window.Keyboard.Key.S)) { vy += 1f; }
				if(context.input.isHeld(SFML.Window.Keyboard.Key.D)) { vx += 1f; }

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
				components.Get<Transform>().x = 1000f;
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
