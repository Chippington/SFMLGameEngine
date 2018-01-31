using SFMLEngine.Entities;
using SFMLEngine.Entities.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using SFMLEngine.Entities.Components.Physics;

namespace SFMLEngine.Entities.Collision {
	public class CollisionMapNEWNEW {
		public class Node {
			public Node() {
				activeCollisions = new List<Node>();
				newCollisions = new List<Node>();
				oldCollisions = new List<Node>();
				horizontals = new HashSet<Node>();
			}

			public IEntity entity;
			public ICollider collider;
			public BoundingBox boundingBox;
			public List<Node> activeCollisions;
			public List<Node> newCollisions;
			public List<Node> oldCollisions;
			HashSet<Node> horizontals;
			public bool hasChanged;

			public bool refresh() {
				if (entity == null)
					entity = collider.getEntity();

				if (boundingBox == null)
					boundingBox = collider.getBoundingBox();

				if (boundingBox == null)
					return false;

				hasChanged = boundingBox.hasChanged;
				boundingBox.hasChanged = false;

				//var _boundingBox = collider.getBoundingBox();
				horizontals.Clear();

				//hasChanged = false;
				//if (boundingBox.x != _boundingBox.x || boundingBox.y != _boundingBox.y)
				//	hasChanged = true;
				//else if (boundingBox.left != _boundingBox.left || boundingBox.right != _boundingBox.right)
				//	hasChanged = true;
				//else if (boundingBox.top != _boundingBox.top || boundingBox.bottom != _boundingBox.bottom)
				//	hasChanged = true;

				if (hasChanged) {
					Node c;
					for (int i = 0; i < activeCollisions.Count; i++) {
						c = activeCollisions[i];
						oldCollisions.Add(c);
						c.oldCollisions.Add(this);
					}
				}

				return hasChanged;
			}

			public void onHorizontalFound(Node other) {
				horizontals.Add(other);
				other.horizontals.Add(this);
			}

			public void onVerticalFound(Node other) {
				if (horizontals.Contains(other)) {
					oldCollisions.Remove(other);
					other.oldCollisions.Remove(this);

					if (activeCollisions.Contains(other) == false) {
						newCollisions.Add(other);
						other.newCollisions.Add(this);
					}
				}
			}

			public void updateCollisions() {

			}

			public void invokeCallbacks() {
				Node col;
				for (int i = 0; i < oldCollisions.Count; i++) {
					col = oldCollisions[i];
					collider.onLeaveCollision(col.collider);
					activeCollisions.Remove(col);
				}

				for (int i = 0; i < activeCollisions.Count; i++) {
					col = activeCollisions[i];
					collider.onStepCollision(col.collider);
				}

				for (int i = 0; i < newCollisions.Count; i++) {
					col = newCollisions[i];
					collider.onEnterCollision(col.collider);
					activeCollisions.Add(col);
				}

				newCollisions.Clear();
				oldCollisions.Clear();
			}
		}

		private Dictionary<ICollider, Node> nodeMap;
		private List<Node> nodeList;
		private List<Node> horizontal;
		private List<Node> vertical;

		public CollisionMapNEWNEW(Scene entities) {
			nodeMap = new Dictionary<ICollider, Node>();
			nodeList = new List<Node>();
			horizontal = new List<Node>();
			vertical = new List<Node>();

			changed = new List<Node>();
			hTestCols = new HashSet<ICollider>();
			_hcols = new List<Node>();
			_vcols = new List<Node>();

			entities.OnEntityCreated += onEntityCreated;
			entities.OnEntityDestroyed = onEntityDestroyed;
		}

		List<Node> changed;
		public void updateCollision(GameContext context) {
			changed.Clear();
			for (int i = 0; i < nodeList.Count; i++)
				if (nodeList[i].refresh()) {
					changed.Add(nodeList[i]);
				}

			for (int i = horizontal.Count - 1; i >= 0; i--) {
				var n1 = horizontal[i];
				var n2 = vertical[i];
				if (n1.hasChanged) {
					horizontal.RemoveAt(i);
				}
				if (n2.hasChanged) {
					vertical.RemoveAt(i);
				}
			}

			Node change;
			for (int k = 0; k < changed.Count; k++) {
				change = changed[k];
				if (horizontal.Count == 0) {
					horizontal.Add(change);
				} else {
					for (int i = 0; i < horizontal.Count; i++) {
						var cur = horizontal[i];
						if (change.boundingBox.x + change.boundingBox.left < cur.boundingBox.x + cur.boundingBox.left) {
							horizontal.Insert(i, change);
							break;
						} else if (i == horizontal.Count - 1) {
							horizontal.Add(change);
							break;
						}
					}
				}

				if (vertical.Count == 0) {
					vertical.Add(change);
				} else {
					for (int i = 0; i < vertical.Count; i++) {
						var cur = vertical[i];
						if (change.boundingBox.y + change.boundingBox.top < cur.boundingBox.y + cur.boundingBox.top) {
							vertical.Insert(i, change);
							break;
						} else if (i == vertical.Count - 1) {
							vertical.Add(change);
							break;
						}
					}
				}
			}

			_hcols.Clear();
			_vcols.Clear();
			for (int k = 0; k < changed.Count; k++) {
				change = changed[k];
				var hh = getHCols(change);
				var vv = getVCols(change, hh);

				for (int i = 0; i < hh.Count; i++)
					change.onHorizontalFound(hh[i]);

				for (int i = 0; i < vv.Count; i++)
					change.onVerticalFound(vv[i]);
			}

			for (int i = 0; i < horizontal.Count; i++)
				horizontal[i].invokeCallbacks();
		}

		List<Node> _vcols;
		private List<Node> getVCols(Node node, List<Node> hCollisions) {
			var collider = node.collider;
			var newBB = node.boundingBox;

			_vcols.Clear();
			if (hCollisions.Count == 0)
				return _vcols;

			int vind;
			for (vind = 0; vind < vertical.Count; vind++)
				if (vertical[vind].collider == collider)
					break;

			for (int i = vind; i < vertical.Count; i++) {
				if (vertical[i].collider == collider)
					continue;

				var otherBB = vertical[i].collider.getBoundingBox();

				float y1, y2, y3, y4;
				y1 = newBB.y + newBB.top;
				y2 = newBB.y + newBB.bottom;
				y3 = otherBB.y + otherBB.top;
				y4 = otherBB.y + otherBB.bottom;

				if (y4 > y1 && y3 < y2) {
					_vcols.Add(vertical[i]);
				} else {
					break;
				}
			}

			for (int i = vind; i >= 0; i--) {
				if (vertical[i].collider == collider)
					continue;

				var otherBB = vertical[i].collider.getBoundingBox();

				float y1, y2, y3, y4;
				y1 = newBB.y + newBB.top;
				y2 = newBB.y + newBB.bottom;
				y3 = otherBB.y + otherBB.top;
				y4 = otherBB.y + otherBB.bottom;

				if (y4 > y1 && y3 < y2) {
					_vcols.Add(vertical[i]);
				} else {
					break;
				}
			}

			return _vcols;
		}

		List<Node> _hcols;
		private List<Node> getHCols(Node node) {
			var collider = node.collider;
			var newBB = node.boundingBox;

			int hind;
			for (hind = 0; hind < horizontal.Count; hind++)
				if (horizontal[hind].collider == collider)
					break;

			_hcols.Clear();
			for (int i = hind; i < horizontal.Count; i++) {
				if (horizontal[i].collider == collider)
					continue;

				var otherBB = horizontal[i].collider.getBoundingBox();

				float x1, x2, x3, x4;
				x1 = newBB.x + newBB.left;
				x2 = newBB.x + newBB.right;
				x3 = otherBB.x + otherBB.left;
				x4 = otherBB.x + otherBB.right;

				if (x4 > x1 && x3 < x2) {
					_hcols.Add(horizontal[i]);
				} else {
					break;
				}
			}

			for (int i = hind; i >= 0; i--) {
				if (horizontal[i].collider == collider)
					continue;

				var otherBB = horizontal[i].collider.getBoundingBox();

				float x1, x2, x3, x4;
				x1 = newBB.x + newBB.left;
				x2 = newBB.x + newBB.right;
				x3 = otherBB.x + otherBB.left;
				x4 = otherBB.x + otherBB.right;

				if (x4 > x1 && x3 < x2) {
					_hcols.Add(horizontal[i]);
				} else {
					break;
				}
			}

			return _hcols;
		}

		public bool testCollision(ICollider one, ICollider two) {
			var n1 = nodeMap[one];
			var n2 = nodeMap[two];

			return testCollision(n1.boundingBox, n2.boundingBox);
		}

		HashSet<ICollider> hTestCols;
		public bool testCollision<T>(ICollider collider, float newX, float newY) {
			if (horizontal.Count == 0) return false;
			if (vertical.Count == 0) return false;

			BoundingBox newBB = collider.getBoundingBox();
			newBB.x = newX;
			newBB.y = newY;

			int hind, vind;
			for (hind = 0; hind < horizontal.Count; hind++)
				if (horizontal[hind].collider == collider)
					break;

			for (vind = 0; vind < vertical.Count; vind++)
				if (vertical[vind].collider == collider)
					break;

			hTestCols.Clear();
			for (int i = hind; i < horizontal.Count; i++) {
				if (horizontal[i].collider == collider)
					continue;

				if (horizontal[i].entity.GetType() != typeof(T))
					continue;

				var otherBB = horizontal[i].collider.getBoundingBox();

				float x1, x2, x3, x4;
				x1 = newBB.x + newBB.left;
				x2 = newBB.x + newBB.right;
				x3 = otherBB.x + otherBB.left;
				x4 = otherBB.x + otherBB.right;

				if (x4 > x1 && x3 < x2) {
					hTestCols.Add(horizontal[i].collider);
				} else {
					break;
				}
			}

			for (int i = hind; i >= 0; i--) {
				if (horizontal[i].collider == collider)
					continue;

				if (horizontal[i].entity.GetType() != typeof(T))
					continue;

				var otherBB = horizontal[i].collider.getBoundingBox();

				float x1, x2, x3, x4;
				x1 = newBB.x + newBB.left;
				x2 = newBB.x + newBB.right;
				x3 = otherBB.x + otherBB.left;
				x4 = otherBB.x + otherBB.right;

				if (x4 > x1 && x3 < x2) {
					hTestCols.Add(horizontal[i].collider);
				} else {
					break;
				}
			}

			for (int i = vind; i < vertical.Count; i++) {
				if (vertical[i].collider == collider)
					continue;

				if (vertical[i].entity.GetType() != typeof(T))
					continue;

				var otherBB = vertical[i].collider.getBoundingBox();

				float y1, y2, y3, y4;
				y1 = newBB.y + newBB.top;
				y2 = newBB.y + newBB.bottom;
				y3 = otherBB.y + otherBB.top;
				y4 = otherBB.y + otherBB.bottom;

				if (y4 > y1 && y3 < y2) {
					if (hTestCols.Contains(vertical[i].collider))
						if (vertical[i].entity.GetType() == typeof(T))
							return true;
				} else {
					break;
				}
			}

			for (int i = vind; i >= 0; i--) {
				if (vertical[i].collider == collider)
					continue;

				if (vertical[i].entity.GetType() != typeof(T))
					continue;

				var otherBB = vertical[i].collider.getBoundingBox();

				float y1, y2, y3, y4;
				y1 = newBB.y + newBB.top;
				y2 = newBB.y + newBB.bottom;
				y3 = otherBB.y + otherBB.top;
				y4 = otherBB.y + otherBB.bottom;

				if (y4 > y1 && y3 < y2) {
					if (hTestCols.Contains(vertical[i].collider))
						if (vertical[i].entity.GetType() == typeof(T))
							return true;
				} else {
					break;
				}
			}

			return false;
		}

		public bool testCollision(BoundingBox bb1, BoundingBox bb2) {
			float x1, x2, x3, x4, y1, y2, y3, y4;
			x1 = bb1.x + bb1.left;
			x2 = bb1.x + bb1.right;
			x3 = bb2.x + bb2.left;
			x4 = bb2.x + bb2.right;
			y1 = bb1.y + bb1.top;
			y2 = bb1.y + bb1.bottom;
			y3 = bb2.y + bb2.top;
			y4 = bb2.y + bb2.bottom;

			return (x4 > x1 && x3 < x2) && (y4 > y1 && y3 < y2);
		}

		private void onEntityCreated(SceneEventArgs args) {
			var comps = args.entity.getComponents();
			foreach (var comp in comps.Values) {
				var collider = comp as ICollider;
				if (collider != null) {
					var newNode = new Node() {
						collider = collider,
					};

					nodeMap.Add(collider, newNode);
					nodeList.Add(newNode);
					return;
				}
			}

			args.entity.components.OnComponentAdded += onEntityComponentAdded;
		}

		private void onEntityComponentAdded(ComponentEventArgs args) {
		}

		private void onEntityDestroyed(SceneEventArgs args) {
		}
	}
	public class CollisionMapNEW {
		public class Node {
			public Node(ICollider collider) {
				this.collider = collider;
				this.entity = collider.getEntity();
				this.bounds = collider.getBoundingBox();

				activeCollisions = new List<Node>();
				oldCollisions = new List<Node>();
				newCollisions = new List<Node>();
				pendingCollisions = new List<Node>();
				check = new HashSet<Node>();
			}

			public BoundingBox bounds;
			public ICollider collider;
			public IEntity entity;

			public Node left, right, top, bottom;

			public List<Node> activeCollisions;
			public List<Node> oldCollisions;
			public List<Node> newCollisions;
			public List<Node> pendingCollisions;
			private HashSet<Node> check;
			static int ii = 0;
			public void update() {
				bounds = collider.getBoundingBox();

				if (bounds.hasChanged == false)
					return;

				while (right != null && right.bounds.x + right.bounds.left < bounds.x + bounds.left)
					shiftRight();

				while (left != null && left.bounds.x + left.bounds.left > bounds.x + bounds.left)
					shiftLeft();

				while (top != null && top.bounds.y + top.bounds.top > bounds.y + bounds.top)
					shiftUp();

				while (bottom != null && bottom.bounds.y + bottom.bounds.top < bounds.y + bounds.top)
					shiftDown();

				check.Clear();
				pendingCollisions.Clear();

				Node cur = right;
				while (cur != null && cur.bounds.intersects(bounds)) {
					if (check.Contains(cur) == false) {
						pendingCollisions.Add(cur);
						check.Add(cur);
					}

					cur = cur.right;
				}

				cur = bottom;
				while (cur != null && cur.bounds.intersects(bounds)) {
					if (check.Contains(cur) == false) {
						pendingCollisions.Add(cur);
						check.Add(cur);
					}

					cur = cur.bottom;
				}

				cur = left;
				while (cur != null && cur.bounds.intersects(bounds)) {
					if (check.Contains(cur) == false) {
						pendingCollisions.Add(cur);
						check.Add(cur);
					}

					cur = cur.left;
				}

				cur = top;
				while (cur != null && cur.bounds.intersects(bounds)) {
					if (check.Contains(cur) == false) {
						pendingCollisions.Add(cur);
						check.Add(cur);
					}

					cur = cur.top;
				}
			}

			public void updatePending() {
				if (pendingCollisions.Count == 0)
					return;

				oldCollisions.Clear();

				Node cur;
				for (int i = 0; i < activeCollisions.Count; i++) {
					cur = activeCollisions[i];
					if (pendingCollisions.Contains(cur) == false) {
						oldCollisions.Add(cur);
						activeCollisions.Remove(cur);
					}
				}

				for (int i = 0; i < pendingCollisions.Count; i++) {
					cur = pendingCollisions[i];
					if (newCollisions.Contains(cur)) {
						newCollisions.Remove(cur);
						activeCollisions.Add(cur);
					} else {
						if (activeCollisions.Contains(cur) == false)
							newCollisions.Add(cur);
					}
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

			private void shiftRight() {
				Console.WriteLine("{0} shift right", entity.GetType().Name);
				var l = left;
				var r = right;
				var rr = right.right;

				if (l != null)
					l.right = r;
				if (r != null)
					r.left = l;

				this.left = r.right;
				this.right = rr;
				r.right = this;
			}

			private void shiftLeft() {
				Console.WriteLine("{0} shift left", entity.GetType().Name);
				var l = left;
				var r = right;
				var ll = left.left;

				if (l != null)
					l.right = r;
				if (r != null)
					r.left = l;

				this.right = l;
				this.left = ll;
				l.left = this;
			}

			private void shiftUp() {
				Console.WriteLine("{0} shift up", entity.GetType().Name);
				var t = top;
				var b = bottom;
				var tt = top.top;

				if (t != null)
					t.bottom = b;
				if (b != null)
					b.top = t;

				this.bottom = t;
				this.top = tt;
				t.top = this;
			}

			private void shiftDown() {
				Console.WriteLine("{0} shift down", entity.GetType().Name);
				var t = top;
				var b = bottom;
				var bb = bottom.bottom;

				if (t != null)
					t.bottom = b;
				if (b != null)
					b.top = t;

				this.top = b;
				this.bottom = bb;
				b.bottom = this;
			}
		}

		Node leftMost;
		Node topMost;
		List<Node> nodeList;
		Dictionary<ICollider, Node> nodeMap;

		public CollisionMapNEW(Scene scene) {
			nodeMap = new Dictionary<ICollider, Node>();
			nodeList = new List<Node>();

			scene.OnEntityCreated += onEntityCreated;
		}

		private void onEntityCreated(SceneEventArgs args) {
			if (nodeList.Count == 0) {
				var firstNode = new Node(args.entity.components.Get<RigidBody>());
				nodeMap.Add(firstNode.collider, firstNode);
				nodeList.Add(firstNode);
				leftMost = topMost = nodeList[0];
				return;
			}

			var newNode = new Node(args.entity.components.Get<RigidBody>());
			leftMost.left = newNode;
			topMost.top = newNode;

			newNode.right = leftMost;
			newNode.bottom = topMost;

			leftMost = topMost = newNode;
			nodeMap.Add(newNode.collider, newNode);
			nodeList.Add(newNode);

			for (int i = 0; i < nodeList.Count; i++) {
				nodeList[i].update();
			}
		}

		public void updateCollision(GameContext context) {
			for (int i = 0; i < nodeList.Count; i++) {
				nodeList[i].update();
			}

			for (int i = 0; i < nodeList.Count; i++) {
				nodeList[i].updatePending();
			}

			for (int i = 0; i < nodeList.Count; i++) {
				nodeList[i].invokeCallbacks();
			}
		}

		public bool testCollision<T>(ICollider collider, float newX, float newY) where T : IEntity {
			var node = nodeMap[collider];
			float oldX = node.bounds.x;
			float oldY = node.bounds.y;

			node.bounds.x = newX;
			node.bounds.y = newY;
			node.update();

			bool ret = false;
			if(node.pendingCollisions.Count > 0) {
				for(int i = 0; i < node.pendingCollisions.Count; i++) {
					var type = node.pendingCollisions[i].entity.GetType();
					if (type == typeof(T) || type.IsSubclassOf(typeof(T)))
						ret = true;
				}
			}

			node.bounds.x = oldX;
			node.bounds.y = oldY;
			node.pendingCollisions.Clear();
			node.update();

			return ret;
		}
	}

	public class CollisionMap {
		public class Node {
			public Node() {
				activeCollisions = new List<Node>();
				newCollisions = new List<Node>();
				oldCollisions = new List<Node>();
				horizontals = new HashSet<Node>();
			}

			public IEntity entity;
			public ICollider collider;
			public BoundingBox boundingBox;
			public List<Node> activeCollisions;
			public List<Node> newCollisions;
			public List<Node> oldCollisions;
			HashSet<Node> horizontals;
			public bool hasChanged;

			public bool refresh() {
				if (entity == null)
					entity = collider.getEntity();

				if (boundingBox == null)
					boundingBox = collider.getBoundingBox();

				if (boundingBox == null)
					return false;

				hasChanged = boundingBox.hasChanged;
				boundingBox.hasChanged = false;

				//var _boundingBox = collider.getBoundingBox();
				horizontals.Clear();

				//hasChanged = false;
				//if (boundingBox.x != _boundingBox.x || boundingBox.y != _boundingBox.y)
				//	hasChanged = true;
				//else if (boundingBox.left != _boundingBox.left || boundingBox.right != _boundingBox.right)
				//	hasChanged = true;
				//else if (boundingBox.top != _boundingBox.top || boundingBox.bottom != _boundingBox.bottom)
				//	hasChanged = true;

				if(hasChanged) {
					Node c;
					for (int i = 0; i < activeCollisions.Count; i++) { 
						c = activeCollisions[i];
						oldCollisions.Add(c);
						c.oldCollisions.Add(this);
					}
				}

				return hasChanged;
			}

			public void onHorizontalFound(Node other) {
				horizontals.Add(other);
				other.horizontals.Add(this);
			}

			public void onVerticalFound(Node other) {
				if(horizontals.Contains(other)) {
					oldCollisions.Remove(other);
					other.oldCollisions.Remove(this);

					if (activeCollisions.Contains(other) == false) {
						newCollisions.Add(other);
						other.newCollisions.Add(this);
					}
				}
			}

			public void updateCollisions() {

			}

			public void invokeCallbacks() {
				Node col;
				for (int i = 0; i < oldCollisions.Count; i++) { 
					col = oldCollisions[i];
					collider.onLeaveCollision(col.collider);
					activeCollisions.Remove(col);
				}

				for (int i = 0; i < activeCollisions.Count; i++) {
					col = activeCollisions[i];
					collider.onStepCollision(col.collider);
				}

				for (int i = 0; i < newCollisions.Count; i++) {
					col = newCollisions[i];
					collider.onEnterCollision(col.collider);
					activeCollisions.Add(col);
				}

				newCollisions.Clear();
				oldCollisions.Clear();
			}
		}

		private Dictionary<ICollider, Node> nodeMap;
		private List<Node> nodeList;
		private List<Node> horizontal;
		private List<Node> vertical;

		public CollisionMap(Scene entities) {
			nodeMap = new Dictionary<ICollider, Node>();
			nodeList = new List<Node>();
			horizontal = new List<Node>();
			vertical = new List<Node>();

			changed = new List<Node>();
			hTestCols = new HashSet<ICollider>();
			_hcols = new List<Node>();
			_vcols = new List<Node>();

			entities.OnEntityCreated += onEntityCreated;
			entities.OnEntityDestroyed = onEntityDestroyed;
		}

		List<Node> changed;
		public void updateCollision(GameContext context) {
			for (int i = 0; i < nodeList.Count; i++)
				if (nodeList[i].refresh()) {
					changed.Add(nodeList[i]);
				}

			for (int i = horizontal.Count - 1; i >= 0; i--) {
				var n1 = horizontal[i];
				var n2 = vertical[i];
				if (n1.hasChanged) {
					horizontal.RemoveAt(i);
				}
				if (n2.hasChanged) {
					vertical.RemoveAt(i);
				}
			}

			Node change;
			for(int k = 0; k < changed.Count; k++) {
				change = changed[k];
				if (horizontal.Count == 0) {
					horizontal.Add(change);
				} else {
					for (int i = 0; i < horizontal.Count; i++) {
						var cur = horizontal[i];
						if (change.boundingBox.x + change.boundingBox.left < cur.boundingBox.x + cur.boundingBox.left) {
							horizontal.Insert(i, change);
							break;
						} else if(i == horizontal.Count - 1) {
							horizontal.Add(change);
							break;
						}
					}
				}

				if (vertical.Count == 0) {
					vertical.Add(change);
				} else {
					for (int i = 0; i < vertical.Count; i++) {
						var cur = vertical[i];
						if (change.boundingBox.y + change.boundingBox.top < cur.boundingBox.y + cur.boundingBox.top) {
							vertical.Insert(i, change);
							break;
						} else if(i == vertical.Count - 1) {
							vertical.Add(change);
							break;
						}
					}
				}
			}

			_hcols.Clear();
			_vcols.Clear();
			for (int k = 0; k < changed.Count; k++) {
				change = changed[k];
				var hh = getHCols(change);
				var vv = getVCols(change, hh);

				for(int i = 0; i < hh.Count; i++)
					change.onHorizontalFound(hh[i]);

				for(int i = 0; i < vv.Count; i++)
					change.onVerticalFound(vv[i]);
			}

			for(int i = 0; i < horizontal.Count; i++)
				horizontal[i].invokeCallbacks();
		}

		List<Node> _vcols;
		private List<Node> getVCols(Node node, List<Node> hCollisions) {
			var collider = node.collider;
			var newBB = node.boundingBox;

			_vcols.Clear();
			if (hCollisions.Count == 0)
				return _vcols;

			int vind;
			for (vind = 0; vind < vertical.Count; vind++)
				if (vertical[vind].collider == collider)
					break;

			for (int i = vind; i < vertical.Count; i++) {
				if (vertical[i].collider == collider)
					continue;

				var otherBB = vertical[i].collider.getBoundingBox();

				float y1, y2, y3, y4;
				y1 = newBB.y + newBB.top;
				y2 = newBB.y + newBB.bottom;
				y3 = otherBB.y + otherBB.top;
				y4 = otherBB.y + otherBB.bottom;

				if (y4 > y1 && y3 < y2) {
					_vcols.Add(vertical[i]);
				} else {
					break;
				}
			}

			for (int i = vind; i >= 0; i--) {
				if (vertical[i].collider == collider)
					continue;

				var otherBB = vertical[i].collider.getBoundingBox();

				float y1, y2, y3, y4;
				y1 = newBB.y + newBB.top;
				y2 = newBB.y + newBB.bottom;
				y3 = otherBB.y + otherBB.top;
				y4 = otherBB.y + otherBB.bottom;

				if (y4 > y1 && y3 < y2) {
					_vcols.Add(vertical[i]);
				} else {
					break;
				}
			}

			return _vcols;
		}

		List<Node> _hcols;
		private List<Node> getHCols(Node node) {
			var collider = node.collider;
			var newBB = node.boundingBox;

			int hind;
			for (hind = 0; hind < horizontal.Count; hind++)
				if (horizontal[hind].collider == collider)
					break;

			_hcols.Clear();
			for (int i = hind; i < horizontal.Count; i++) {
				if (horizontal[i].collider == collider)
					continue;

				var otherBB = horizontal[i].collider.getBoundingBox();

				float x1, x2, x3, x4;
				x1 = newBB.x + newBB.left;
				x2 = newBB.x + newBB.right;
				x3 = otherBB.x + otherBB.left;
				x4 = otherBB.x + otherBB.right;

				if (x4 > x1 && x3 < x2) {
					_hcols.Add(horizontal[i]);
				} else {
					break;
				}
			}

			for (int i = hind; i >= 0; i--) {
				if (horizontal[i].collider == collider)
					continue;

				var otherBB = horizontal[i].collider.getBoundingBox();

				float x1, x2, x3, x4;
				x1 = newBB.x + newBB.left;
				x2 = newBB.x + newBB.right;
				x3 = otherBB.x + otherBB.left;
				x4 = otherBB.x + otherBB.right;

				if (x4 > x1 && x3 < x2) {
					_hcols.Add(horizontal[i]);
				} else {
					break;
				}
			}

			return _hcols;
		}

		public bool testCollision(ICollider one, ICollider two) {
			var n1 = nodeMap[one];
			var n2 = nodeMap[two];

			return testCollision(n1.boundingBox, n2.boundingBox);
		}

		HashSet<ICollider> hTestCols;
		public bool testCollision<T>(ICollider collider, float newX, float newY) {
			if (horizontal.Count == 0) return false;
			if (vertical.Count == 0) return false;

			BoundingBox newBB = new BoundingBox();
			newBB.copy(collider.getBoundingBox());
			newBB.x = newX;
			newBB.y = newY;

			int hind, vind;
			for (hind = 0; hind < horizontal.Count; hind++)
				if (horizontal[hind].collider == collider)
					break;

			for (vind = 0; vind < vertical.Count; vind++)
				if (vertical[vind].collider == collider)
					break;

			hTestCols.Clear();
			for (int i = hind; i < horizontal.Count; i++) {
				if (horizontal[i].collider == collider)
					continue;

				if (horizontal[i].entity.GetType() != typeof(T))
					continue;

				var otherBB = horizontal[i].collider.getBoundingBox();

				float x1, x2, x3, x4;
				x1 = newBB.x + newBB.left;
				x2 = newBB.x + newBB.right;
				x3 = otherBB.x + otherBB.left;
				x4 = otherBB.x + otherBB.right;

				if (x4 > x1 && x3 < x2) {
					hTestCols.Add(horizontal[i].collider);
				} else {
					break;
				}
			}

			for (int i = hind; i >= 0; i--) {
				if (horizontal[i].collider == collider)
					continue;

				if (horizontal[i].entity.GetType() != typeof(T))
					continue;

				var otherBB = horizontal[i].collider.getBoundingBox();

				float x1, x2, x3, x4;
				x1 = newBB.x + newBB.left;
				x2 = newBB.x + newBB.right;
				x3 = otherBB.x + otherBB.left;
				x4 = otherBB.x + otherBB.right;

				if (x4 > x1 && x3 < x2) {
					hTestCols.Add(horizontal[i].collider);
				} else {
					break;
				}
			}

			for (int i = vind; i < vertical.Count; i++) {
				if (vertical[i].collider == collider)
					continue;

				if (vertical[i].entity.GetType() != typeof(T))
					continue;

				var otherBB = vertical[i].collider.getBoundingBox();

				float y1, y2, y3, y4;
				y1 = newBB.y + newBB.top;
				y2 = newBB.y + newBB.bottom;
				y3 = otherBB.y + otherBB.top;
				y4 = otherBB.y + otherBB.bottom;

				if (y4 > y1 && y3 < y2) {
					if (hTestCols.Contains(vertical[i].collider))
						if(vertical[i].entity.GetType() == typeof(T))
							return true;
				} else {
					break;
				}
			}

			for (int i = vind; i >= 0; i--) {
				if (vertical[i].collider == collider)
					continue;

				if (vertical[i].entity.GetType() != typeof(T))
					continue;

				var otherBB = vertical[i].collider.getBoundingBox();

				float y1, y2, y3, y4;
				y1 = newBB.y + newBB.top;
				y2 = newBB.y + newBB.bottom;
				y3 = otherBB.y + otherBB.top;
				y4 = otherBB.y + otherBB.bottom;

				if (y4 > y1 && y3 < y2) {
					if (hTestCols.Contains(vertical[i].collider))
						if(vertical[i].entity.GetType() == typeof(T))
							return true;
				} else {
					break;
				}
			}

			return false;
		}

		public bool testCollision(BoundingBox bb1, BoundingBox bb2) {
			float x1, x2, x3, x4, y1, y2, y3, y4;
			x1 = bb1.x + bb1.left;
			x2 = bb1.x + bb1.right;
			x3 = bb2.x + bb2.left;
			x4 = bb2.x + bb2.right;
			y1 = bb1.y + bb1.top;
			y2 = bb1.y + bb1.bottom;
			y3 = bb2.y + bb2.top;
			y4 = bb2.y + bb2.bottom;

			return (x4 > x1 && x3 < x2) && (y4 > y1 && y3 < y2);
		}

		private void onEntityCreated(SceneEventArgs args) {
			var comps = args.entity.getComponents();
			foreach (var comp in comps.Values) {
				var collider = comp as ICollider;
				if (collider != null) {
					var newNode = new Node() {
						collider = collider,
					};

					nodeMap.Add(collider, newNode);
					nodeList.Add(newNode);
					return;
				}
			}

			args.entity.components.OnComponentAdded += onEntityComponentAdded;
		}

		private void onEntityComponentAdded(ComponentEventArgs args) {
		}

		private void onEntityDestroyed(SceneEventArgs args) {
		}
	}

	public class CollisionMapOLD {
		public class Node {
			public Node() {
				activeCollisions = new HashSet<ICollider>();
				newCollisions = new HashSet<ICollider>();
				oldCollisions = new HashSet<ICollider>();
			}

			public float start;
			public float end;
			public ICollider collider;

			public HashSet<ICollider> activeCollisions;
			public HashSet<ICollider> newCollisions;
			public HashSet<ICollider> oldCollisions;

			public void onCollisionFound(ICollider other) {
				if (activeCollisions.Contains(other))
					return;

				newCollisions.Add(other);
			}

			public void updateCollisions() {

			}

			public void invokeCallbacks() {
				foreach (var col in newCollisions)
					collider.onEnterCollision(col);

				foreach (var col in oldCollisions)
					collider.onLeaveCollision(col);

				foreach (var col in activeCollisions)
					collider.onStepCollision(col);
			}
		}

		private class SortedLinkedList<T> : ICollection<T> where T : Node {
			private ListNode start, end;
			private Dictionary<T, ListNode> nodeMap;
			private List<ListNode> nodeList;

			public int Count => nodeMap.Count;

			public bool IsReadOnly => false;

			public class ListNode {
				public ListNode next;
				public ListNode prev;

				public void setNext(ListNode nextNode) {
					if (nextNode == this)
						throw new Exception("Next node cannot be same node");

					if (nextNode != null)
						nextNode.prev = this;

					next = nextNode;
				}

				public void setPrev(ListNode prevNode) {
					if (prevNode == this)
						throw new Exception("Previous node cannot be same node");

					if (prevNode != null)
						prevNode.next = this;

					prev = prevNode;
				}

				public T data;
				public ListNode(T data) {
					this.data = data;
				}
			}

			public class SortedLinkedListEnumerator : IEnumerator<T> {
				private ListNode currentNode;
				private ListNode startNode;
				public SortedLinkedListEnumerator(ListNode start) {
					startNode = start;
					currentNode = null;
				}

				public T Current => currentNode.data;

				object IEnumerator.Current => throw new NotImplementedException();

				public void Dispose() {
					currentNode = null;
				}

				public bool MoveNext() {
					if (currentNode == null) {
						if (startNode == null)
							return false;

						currentNode = startNode;
						return true;
					}

					if (currentNode.next == null)
						return false;

					currentNode = currentNode.next;
					return true;
				}

				public bool MovePrevious() {
					if (currentNode.prev == null)
						return false;

					currentNode = currentNode.prev;
					return true;
				}

				public bool HasNext() {
					return currentNode.next != null;
				}

				public bool HasPrevious() {
					return currentNode.prev != null;
				}

				public T PeekNext() {
					if (currentNode.next == null)
						return default(T);

					return currentNode.next.data;
				}
				public T PeekPrevious() {
					if (currentNode.prev == null)
						return default(T);

					return currentNode.prev.data;
				}

				public void Reset() {
					currentNode = startNode;
				}
			}

			public SortedLinkedList() {
				nodeMap = new Dictionary<T, ListNode>();
				nodeList = new List<ListNode>();
			}

			private ListNode ShiftRight(ListNode node) {
				if (node.next == null)
					return null;

				var tmp = node.next.data;
				node.next.data = node.data;
				node.data = tmp;
				return node.next;
			}

			private ListNode ShiftLeft(ListNode node) {
				if (node.prev == null)
					return null;

				var tmp = node.prev.data;
				node.prev.data = node.data;
				node.data = tmp;
				return node.prev;
			}

			public List<ListNode> GetList() {
				return nodeList;
			}

			public Dictionary<ICollider, List<ICollider>> Update() {
				var cur = start;
				while(cur != null) { 
					//Shift right
					if (cur.next != null && cur.data.start > cur.next.data.start) {
						var next = cur.next;
						while (next.next != null && cur.data.start > next.next.data.start) {
							next = next.next;
						}

						var tmp = cur.next;
						if(cur.prev != null)
							cur.prev.setNext(cur.next);

						if (cur.next != null)
							cur.next.setPrev(cur.prev);

						cur.setNext(next.next);
						cur.setPrev(next);
						cur = tmp;
					}

					//Shift left
					if (cur.prev != null && cur.data.start < cur.prev.data.start) {
						var prev = cur.prev;
						while (prev.prev != null && cur.data.start < prev.prev.data.start) {
							prev = prev.prev;
						}

						var tmp = cur.prev;
						if (cur.prev != null)
							cur.prev.setNext(cur.next);

						if (cur.next != null)
							cur.next.setPrev(cur.prev);

						cur.setPrev(prev.prev);
						cur.setNext(prev);
						cur = tmp;
					}

					cur = cur.next;
				}

				if (start == null)
					return new Dictionary<ICollider, List<ICollider>>();

				while (end.next != null)
					end = end.next;

				while (start.prev != null)
					start = start.prev;

				Dictionary<ICollider, List<ICollider>> overlaps = new Dictionary<ICollider, List<ICollider>>();
				cur = start;

				Queue<T> cols = new Queue<T>();
				while(cur != null) {
					var colCheck = cur.next;
					while (colCheck != null && cur.data.end > colCheck.data.start) {
						cols.Enqueue(colCheck.data);
						colCheck = colCheck.next;
					}

					if(cols.Count > 0) {
						if(overlaps.ContainsKey(cur.data.collider) == false)
							overlaps.Add(cur.data.collider, new List<ICollider>());

						while (cols.Count > 0) {
							var col = cols.Dequeue();
							if(overlaps.ContainsKey(col.collider) == false)
								overlaps.Add(col.collider, new List<ICollider>());

							overlaps[col.collider].Add(cur.data.collider);
							overlaps[cur.data.collider].Add(col.collider);
						}
					}

					cols.Clear();
					cur = cur.next;
				}

				return overlaps;
			}

			public void Add(T item) {
				if (nodeMap.ContainsKey(item))
					throw new ArgumentException("Collection can only have one instance of each element");

				if (start == null) {
					start = end = new ListNode(item);
					nodeMap.Add(item, start);
					nodeList.Add(start);
					return;
				}

				var cur = start;
				while (cur.next != null && item.start > cur.data.start)
					cur = cur.next;

				var newNode = new ListNode(item);
				newNode.setNext(cur.next);
				newNode.setPrev(cur);

				//newNode.prev = cur;
				//newNode.next = cur.next;
				//if (cur.next != null)
				//	cur.next.prev = newNode;
				//cur.next = newNode;

				while (end.next != null)
					end = end.next;

				while (start.prev != null)
					start = start.prev;

				nodeMap.Add(item, newNode);
				nodeList.Add(newNode);
			}

			public void Clear() {
				start = end = null;
				nodeMap = new Dictionary<T, ListNode>();
				nodeList = new List<ListNode>();
			}

			public bool Contains(T item) {
				return nodeMap.ContainsKey(item);
			}

			public void CopyTo(T[] arr, int arrayIndex) {
				var e = GetEnumerator();
				for (int i = 0; i < arr.Length; i++) {
					e.MoveNext();
					arr[i + arrayIndex] = e.Current;
				}
			}

			public bool Remove(T item) {
				if (nodeMap.ContainsKey(item) == false)
					return false;

				var node = nodeMap[item];
				if (node.prev != null)
					node.prev.next = node.next;

				if (node.next != null)
					node.next.prev = node.prev;

				nodeMap.Remove(item);
				nodeList.Remove(node);
				return true;
			}

			public IEnumerator<T> GetEnumerator(T data) {
				if (nodeMap.ContainsKey(data) == false)
					return null;

				return new SortedLinkedListEnumerator(nodeMap[data]);
			}

			public IEnumerator<T> GetEnumerator() {
				return new SortedLinkedListEnumerator(start);
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return new SortedLinkedListEnumerator(start);
			}
		}

		private class CollisionList {
			public SortedLinkedList<Node> list;
			private Func<ICollider, float> nodeStartFunc;
			private Func<ICollider, float> nodeEndFunc;
			public CollisionList(
				Func<ICollider, float> nodeStartFunc,
				Func<ICollider, float> nodeEndFunc) {

				list = new SortedLinkedList<Node>();
				this.nodeStartFunc = nodeStartFunc;
				this.nodeEndFunc = nodeEndFunc;
			}

			public void addNode(ICollider collider) {
				Node n = new Node() {
					collider = collider,
					start = nodeStartFunc(collider),
					end = nodeEndFunc(collider),
				};

				list.Add(n);
			}

			public Dictionary<ICollider, List<ICollider>> updateNodes() {
				foreach (var n in list) {
					n.start = nodeStartFunc(n.collider);
					n.end = nodeEndFunc(n.collider);
				}

				return list.Update();
			}
		}

		private CollisionList horizontal;
		private CollisionList vertical;
		private Dictionary<ICollider, ICollider> tempActiveCollisions;
		private Dictionary<ICollider, List<ICollider>> activeCollisions;
		private Dictionary<ICollider, List<ICollider>> newCollisions;
		private Dictionary<ICollider, List<ICollider>> oldCollisions;

		public CollisionMapOLD(Scene entities) {
			horizontal = new CollisionList(
				n => { var bb = n.getBoundingBox(); return bb.x + bb.left; },
				n => { var bb = n.getBoundingBox(); return bb.x + bb.right; });
			vertical = new CollisionList(
				n => { var bb = n.getBoundingBox(); return bb.y + bb.top; },
				n => { var bb = n.getBoundingBox(); return bb.y + bb.bottom; });

			entities.OnEntityCreated += onEntityCreated;
			entities.OnEntityDestroyed = onEntityDestroyed;

			tempActiveCollisions = new Dictionary<ICollider, ICollider>();
			activeCollisions = new Dictionary<ICollider, List<ICollider>>();
			newCollisions = new Dictionary<ICollider, List<ICollider>>();
			oldCollisions = new Dictionary<ICollider, List<ICollider>>();
		}

		public void updateMap() {
			var horizontalOverlaps = horizontal.updateNodes();
			var verticalOverlaps = vertical.updateNodes();

			foreach (var key in activeCollisions.Keys) {
				foreach (var col in newCollisions[key]) {
					activeCollisions[key].Add(col);
				}

				foreach (var col in oldCollisions[key]) {
					activeCollisions[key].Remove(col);
				}

				newCollisions[key].Clear();
				oldCollisions[key].Clear();

				foreach (var col in activeCollisions[key])
					oldCollisions[key].Add(col);
			}

			foreach (var key in activeCollisions.Keys) {
				if (horizontalOverlaps.ContainsKey(key)) {
					if (verticalOverlaps.ContainsKey(key) == false)
						continue;

					var hcols = horizontalOverlaps[key];
					var vcols = verticalOverlaps[key];

					foreach (var col in hcols) {
						if (vcols.Contains(col)) {
							//'key' collides with 'col'
							handleCollision(key, col);
						}
					}
				}
			}

			foreach (var key in activeCollisions.Keys) {
				foreach (var col in newCollisions[key])
					key.onEnterCollision(col);
				foreach (var col in oldCollisions[key])
					key.onLeaveCollision(col);
				foreach (var col in activeCollisions[key])
					key.onStepCollision(col);
			}
		}

		private void handleCollision(ICollider one, ICollider two) {
			if (newCollisions[one].Contains(two) || newCollisions[two].Contains(one))
				return;

			oldCollisions[one].Remove(two);
			oldCollisions[two].Remove(one);

			if (activeCollisions[one].Contains(two) || activeCollisions[two].Contains(one))
				return;

			newCollisions[one].Add(two);
			newCollisions[two].Add(one);
		}

		private void onEntityCreated(SceneEventArgs args) {
			var comps = args.entity.getComponents();
			foreach (var comp in comps.Values) {
				var collider = comp as ICollider;
				if (collider != null) {
					horizontal.addNode(collider);
					vertical.addNode(collider);

					activeCollisions[collider] = new List<ICollider>();
					newCollisions[collider] = new List<ICollider>();
					oldCollisions[collider] = new List<ICollider>();
					return;
				}
			}

			args.entity.components.OnComponentAdded += onEntityComponentAdded;
		}

		private void onEntityComponentAdded(ComponentEventArgs args) {

		}

		private void onEntityDestroyed(SceneEventArgs args) {

		}

	}
}
