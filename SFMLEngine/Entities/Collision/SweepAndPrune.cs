using SFMLEngine.Entities.Components;
using SFMLEngine.Entities.Components.Physics;
using SFMLEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities.Collision {
	public class SweepAndPrune : ObjectBase, ICollisionMap {
		public class Node : ObjectBase {
			private Node col;
			public SweepPoint top, left, bottom, right;

			public IEntity entity;
			public ICollider collider;
			public BoundingBox bounds;

			public List<Node> activeCollisions;
			public List<Node> oldCollisions;
			public List<Node> newCollisions;
			public List<Node> pendingCollisions;

			public Node(ICollider comp) {
				this.entity = comp.getEntity();

				pendingCollisions = new List<Node>();
				activeCollisions = new List<Node>();
				oldCollisions = new List<Node>();
				newCollisions = new List<Node>();

				this.collider = comp;
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
				if (collider.getIgnoreCallbacks())
					return;

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

		private Dictionary<ICollider, Node> removeMap;
		private Queue<Tuple<Node, Node>> fullOverlaps;
		private List<Node> nodeList;
		private List<SweepPoint> haxis;
		private List<SweepPoint> vaxis;

		public SweepAndPrune() {
			removeMap = new Dictionary<ICollider, Node>();
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

		public void onUpdate(GameContext context) {
			for (int i = 0; i < nodeList.Count; i++)
				nodeList[i].update();

			sortAxis(haxis);
			sortAxis(vaxis);
			for (int i = 0; i < nodeList.Count; i++) {
				nodeList[i].invokeCallbacks();
			}
		}

		public void onDraw(GameContext context) {
		}

		HashSet<ICollider> t = new HashSet<ICollider>();
		public void onEntityComponentAdded(SceneEventArgs args) {
			var c = args.component as ICollider;
			if (c == null) return;

			t.Add(c);

			addNode(c);
		}

		public void onEntityComponentDestroyed(SceneEventArgs args) {
			var c = args.component as ICollider;
			if (c == null) return;

			removeNode(c);
		}

		public void onEntityCreated(SceneEventArgs args) {
			//var comps = args.entity.components.Get().ToList();
			//for (int i = 0; i < comps.Count; i++) {
			//	var col = comps[i] as ICollider;

			//	if(col != null)
			//		addNode(col);
			//}
		}

		public void onEntityDestroyed(SceneEventArgs args) {
			//var comps = args.entity.components.Get().ToList();
			//for (int i = 0; i < comps.Count; i++) {
			//	var col = comps[i] as ICollider;

			//	if (col != null)
			//		removeNode(col);
			//}
		}

		private Node addNode(ICollider comp) {
			Node newNode = new Node(comp);
			removeMap.Add(comp, newNode);
			nodeList.Add(newNode);
			vaxis.Add(newNode.top);
			haxis.Add(newNode.left);
			haxis.Add(newNode.right);
			vaxis.Add(newNode.bottom);
			return newNode;
		}

		private Node removeNode(ICollider comp) {
			Node oldNode = removeMap[comp];
			nodeList.Remove(oldNode);
			vaxis.Remove(oldNode.top);
			haxis.Remove(oldNode.left);
			haxis.Remove(oldNode.right);
			vaxis.Remove(oldNode.bottom);
			removeMap.Remove(comp);
			return oldNode;
		}

		public void onInitialize(GameContext context) {
			fullOverlaps.Clear();
			nodeList.Clear();
			haxis.Clear();
			vaxis.Clear();

			log("SAP Collision Map initialized");
		}

		public void onDispose(GameContext context) {
			fullOverlaps.Clear();
			nodeList.Clear();
			haxis.Clear();
			vaxis.Clear();

			fullOverlaps = null;
			haxis = vaxis = null;
			nodeList = null;
		}
	}
}
