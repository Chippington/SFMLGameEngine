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
				var test = context.entities.instantiate<TestEntity>("Test");
				test = context.entities.instantiate<TestEntity>("Test2");
				test.components.Get<Position>().x = 3f;
				test.components.Get<Position>().y = 3f;
			}

			protected override void graphicsUpdate(GameContext context) {
				base.graphicsUpdate(context);
			}

			protected override void logicUpdate(GameContext context) {
				base.logicUpdate(context);
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
					left = 0f, right = 5f,
					top = 0f, bottom = 5f,
				});

				var p = components.Add<Position>();
				p.x = 0f;
				p.y = 0f;

				r.onCollisionEnter += onCollisionEnter;
			}

			private void onCollisionEnter(CollisionEventArgs args) {
				var other = args.other.getEntity() as TestEntity;
				if(other != null) {
					Console.WriteLine($"({name}) Collision with: " + other.name);
				}

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
