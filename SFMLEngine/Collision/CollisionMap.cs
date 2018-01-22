using SFMLEngine.Entities;
using SFMLEngine.Entities.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Collision {
	public class CollisionMap {
		private class CollisionList {
			public class Node : IComparable<Node> {
				public float start, end;
				public ICollider collider;

				public int CompareTo(Node other) {
					var c = start.CompareTo(other.start);
					if (c == 0) return 1;
					return c;
				}
			}

			public SortedSet<Node> list;
			private Func<ICollider, float> nodeStartFunc;
			private Func<ICollider, float> nodeEndFunc;
			public CollisionList(
				Func<ICollider, float> nodeStartFunc, 
				Func<ICollider, float> nodeEndFunc) {

				list = new SortedSet<Node>();
				this.nodeStartFunc = nodeStartFunc;
				this.nodeEndFunc = nodeEndFunc;
			}

			public void addNode(ICollider collider) {
				Node n = new Node {
					collider = collider,
					start = nodeStartFunc(collider),
					end = nodeEndFunc(collider)
				};

				list.Add(n);
			}

			public void updateNodes() {
				var oldList = list;
				list = new SortedSet<Node>();

				foreach (var n in oldList)
					addNode(n.collider);
			}
		}

		private CollisionList horizontal;
		private CollisionList vertical;
		public CollisionMap(EntitySet entities) {
			horizontal = new CollisionList(n => n.getBoundingBox().left, n => n.getBoundingBox().right);
			vertical = new CollisionList(n => n.getBoundingBox().top, n => n.getBoundingBox().bottom);

			entities.OnEntityCreated += onEntityCreated;
			entities.OnEntityDestroyed = onEntityDestroyed;
		}

		public void updateMap() {
			horizontal.updateNodes();
			vertical.updateNodes();
		}

		private void onEntityCreated(EntitySetEventArgs args) {
			var comps = args.entity.getComponents();
			foreach(var comp in comps) {
				var collider = comp as ICollider;
				if(collider != null) {
					horizontal.addNode(collider);
					vertical.addNode(collider);
					return;
				}
			}

			args.entity.OnComponentAdded += onEntityComponentAdded;
		}

		private void onEntityComponentAdded(EntityEventArgs args) {

		}

		private void onEntityDestroyed(EntitySetEventArgs args) {

		}

	}
}
