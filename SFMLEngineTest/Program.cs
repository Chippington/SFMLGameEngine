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
				for(int i = 0; i < 10; i++) {
					var test = context.entities.instantiate<TestEntity>("Test"+i);
					test.components.Get<Position>().x = 50 + (50 * i);
					test.components.Get<Position>().y = 50;
					test.components.Get<RigidBody>().setDebugDraw(true);
				}

				var player = context.entities.instantiate<TestPlayer>();
			}

			protected override void graphicsUpdate(GameContext context) {
				base.graphicsUpdate(context);
			}

			protected override void logicUpdate(GameContext context) {
				base.logicUpdate(context);
			}
		}

		public class TestPlayer : Entity {
			Position pos;
			float vx, vy;

			public TestPlayer() {
				var rb = components.Add<RigidBody>();
				rb.setBoundingBox(new BoundingBox(-5f, -5f, 5f, 5f));
				rb.setDebugDraw(true);
				pos = components.Add<Position>();
			}

			static int _ii;
			static string ii {
				get { return (_ii++).ToString(); }
			}
			public override void onUpdate(GameContext context) {
				base.onUpdate(context);
				vx = vy = 0;
				if(context.input.isHeld(SFML.Window.Keyboard.Key.W)) { vy -= 1f; }
				if(context.input.isHeld(SFML.Window.Keyboard.Key.A)) { vx -= 1f; }
				if(context.input.isHeld(SFML.Window.Keyboard.Key.S)) { vy += 1f; }
				if(context.input.isHeld(SFML.Window.Keyboard.Key.D)) { vx += 1f; }

				pos.x += vx * context.deltaTime;
				pos.y += vy * context.deltaTime;

				if (context.collision.testCollision<TestEntity>(components.Get<RigidBody>(), pos.x, pos.y)) {
					Console.WriteLine("TEST" + ii);
				}
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
