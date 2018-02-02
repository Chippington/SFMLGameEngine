using SFMLEngine.Entities.Components.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities.Collision {
	public class SweepAndPrune : ICollisionMap {
		public class Node {
			public SweepPoint top, left, bottom, right;

			public IEntity entity;
			public ICollider collider;
			public BoundingBox bounds;

			public List<Node> activeCollisions;
			public List<Node> oldCollisions;
			public List<Node> newCollisions;
			public List<Node> pendingCollisions;

			public Node(IEntity entity) {
				pendingCollisions = new List<Node>();
				activeCollisions = new List<Node>();
				oldCollisions = new List<Node>();
				newCollisions = new List<Node>();

				this.entity = entity;
				this.collider = entity.components.Get<RigidBody>();
				this.bounds = this.collider.getBoundingBox();

				top = new SweepPoint(this, SweepPoint.Type.START);
				left = new SweepPoint(this, SweepPoint.Type.START);
				right = new SweepPoint(this, SweepPoint.Type.END);
				bottom = new SweepPoint(this, SweepPoint.Type.END);
				update();
			}

			public bool update() {
				for (int i = 0; i < newCollisions.Count; i++)
					activeCollisions.Add(newCollisions[i]);

				newCollisions.Clear();
				oldCollisions.Clear();

				top.value = bounds.y + bounds.top;
				bottom.value = bounds.y + bounds.bottom;
				left.value = bounds.x + bounds.left;
				right.value = bounds.x + bounds.right;
				return true;
			}

			public void onCollisionEnter(Node other) {
				if(activeCollisions.Contains(other) == false) {
					newCollisions.Add(other);
				}
			}

			public void onCollisionLeave(Node other) {
				if (activeCollisions.Contains(other) || newCollisions.Contains(other)) {
					activeCollisions.Remove(other);
					oldCollisions.Add(other);
				} else if (newCollisions.Contains(other)) {
					newCollisions.Remove(other);
					oldCollisions.Add(other);
				}
			}

			public void invokeCallbacks() {
				Node col;
				for (int i = 0; i < oldCollisions.Count; i++) {
					col = oldCollisions[i];
					collider.onLeaveCollision(col.collider);
				}

				for (int i = 0; i < activeCollisions.Count; i++) {
					col = activeCollisions[i];
					collider.onStepCollision(col.collider);
				}

				for (int i = 0; i < newCollisions.Count; i++) {
					col = newCollisions[i];
					collider.onEnterCollision(col.collider);
				}
			}
		}

		public class SweepPoint {
			public enum Type {
				START, END
			}

			public Node node;
			public Type type;
			public float value;

			public SweepPoint(Node node, Type type) {
				this.node = node;
				this.type = type;
			}
		}

		private Queue<Tuple<Node, Node>> fullOverlaps;
		private List<Node> nodeList;
		private List<SweepPoint> haxis;
		private List<SweepPoint> vaxis;

		public SweepAndPrune() {
			fullOverlaps = new Queue<Tuple<Node, Node>>();
			nodeList = new List<Node>();
			haxis = new List<SweepPoint>();
			vaxis = new List<SweepPoint>();
		}

		private void sortAxis(List<SweepPoint> axis) {
			for (int j = 1; j < axis.Count; j++) {
				SweepPoint keyelement = axis[j];
				float key = keyelement.value;

				int i = j - 1;

				while (i >= 0 && axis[i].value > key) {
					SweepPoint swapper = axis[i];

					if (keyelement.type == SweepPoint.Type.START && swapper.type == SweepPoint.Type.END) {
						if (keyelement.node.bounds.intersects(swapper.node.bounds)) {
							lock (fullOverlaps) {
								//fullOverlaps.Enqueue(new Tuple<Node, Node>(keyelement.node, swapper.node));
								keyelement.node.onCollisionEnter(swapper.node);
								swapper.node.onCollisionEnter(keyelement.node);
							}
						}
					}

					if (keyelement.type == SweepPoint.Type.END && swapper.type == SweepPoint.Type.START) {
						lock (fullOverlaps) {
							//fullOverlaps.Remove(new Tuple<Node, Node>(keyelement.node, swapper.node));
							keyelement.node.onCollisionLeave(swapper.node);
							swapper.node.onCollisionLeave(keyelement.node);
						}
					}

					axis[i + 1] = swapper;
					i = i - 1;
				}
				axis[i + 1] = keyelement;
			}
		}

		public void updateCollisionMap(GameContext context) {
			for (int i = 0; i < nodeList.Count; i++)
				nodeList[i].update();

			sortAxis(haxis);
			sortAxis(vaxis);
			for (int i = 0; i < nodeList.Count; i++) {
				nodeList[i].invokeCallbacks();
			}
		}

		public void drawCollisionMap(GameContext context) {
		}

		public void onEntityComponentCreated(SceneEventArgs args) {
			throw new NotImplementedException();
		}

		public void onEntityComponentDestroyed(SceneEventArgs args) {
			throw new NotImplementedException();
		}

		public void onEntityCreated(SceneEventArgs args) {
			var comp = args.entity.components.Get<RigidBody>();
			if (comp != null)
				addNode(args.entity);
		}

		public void onEntityDestroyed(SceneEventArgs args) {
			throw new NotImplementedException();
		}
		private Node addNode(IEntity entity) {
			Node newNode = new Node(entity);
			nodeList.Add(newNode);
			vaxis.Add(newNode.top);
			haxis.Add(newNode.left);
			haxis.Add(newNode.right);
			vaxis.Add(newNode.bottom);
			return newNode;
		}
	}
}
