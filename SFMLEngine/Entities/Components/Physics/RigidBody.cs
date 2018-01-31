using SFML.Graphics;
using SFML.System;
using SFMLEngine.Entities.Components.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities.Components.Physics {
	public interface ICollider : IComponent {
		BoundingBox getBoundingBox();
		void onEnterCollision(ICollider other);
		void onLeaveCollision(ICollider other);
		void onStepCollision(ICollider other);
		bool hasChanged();
	}

	public delegate void CollisionEvent(CollisionEventArgs args);
	public class CollisionEventArgs {
		public ICollider other;
	}

	public class RigidBody : Component, ICollider {
		public CollisionEvent onCollisionEnter;
		public CollisionEvent onCollisionLeave;
		public CollisionEvent onCollisionStep;
		private HashSet<ICollider> dbgCols;
		private RectangleShape dbgRect;
		private Vector2f dbgPosition;
		private Vector2f dbgSize;

		private BoundingBox _bounds;
		private BoundingBox bounds {
			get {
				return _bounds;
			}
		}

		private bool hasChangedData;
		private Position transform;

		public override void onInitialize() {
			base.onInitialize();

			if(_bounds == null)
				_bounds = new BoundingBox();

			hasChangedData = true;
			dbgCols = new HashSet<ICollider>();
			transform = entity.components.Add<Position>();

			dbgPosition = new Vector2f(transform.x + _bounds.left, transform.y + _bounds.top);
			dbgSize = new Vector2f(_bounds.right - _bounds.left, _bounds.bottom - _bounds.top);
		}

		public override void onUpdate(GameContext context) {
			if (_bounds.x != transform.x || _bounds.y != transform.y)
				_bounds.hasChanged = true;

			if (_bounds.hasChanged) {
				_bounds.x = transform.x;
				_bounds.y = transform.y;

				if (dbgRect != null) {
					dbgPosition.X = transform.x + _bounds.left;
					dbgPosition.Y = transform.y + _bounds.top;
					dbgRect.Position = dbgPosition;
				}
			}
		}

		public virtual BoundingBox getBoundingBox() {
			return bounds;
		}

		public virtual void setBoundingBox(BoundingBox newBounds) {
			hasChangedData = true;
			_bounds.copy(newBounds);
			newBounds.hasChanged = true;

			dbgSize.X = _bounds.right - _bounds.left;
			dbgSize.Y = _bounds.bottom - _bounds.top;
			if(dbgRect != null)
				dbgRect.Size = dbgSize;
		}

		public virtual void onEnterCollision(ICollider other) {
			onCollisionEnter?.Invoke(new CollisionEventArgs() {
				other = other,
			});

			if (dbgRect == null)
				return;

			dbgCols.Add(other);
		}

		public virtual void onLeaveCollision(ICollider other) {
			onCollisionLeave?.Invoke(new CollisionEventArgs() {
				other = other,
			});

			if (dbgRect == null)
				return;

			dbgCols.Remove(other);
		}

		public virtual void onStepCollision(ICollider other) {
			onCollisionStep?.Invoke(new CollisionEventArgs() {
				other = other,
			});
		}

		public void setDebugDraw(bool draw) {
			if (draw == false)
				dbgRect = null;

			dbgRect = new RectangleShape();
			dbgRect.FillColor = new Color(0, 0, 0, 0);
			dbgRect.OutlineColor = new Color(0, 255, 0, 255);
			dbgRect.OutlineThickness = 1f;
			dbgRect.Position = dbgPosition;
			dbgRect.Size = dbgSize;
		}

		public override void onDraw(GameContext context) {
			if (dbgRect == null)
				return;

			base.onDraw(context);
			if (dbgRect.OutlineColor.G == 0)
				if (dbgCols.Count == 0)
					dbgRect.OutlineColor = new Color(0, 255, 0, 255);

			if (dbgRect.OutlineColor.G == 255)
				if (dbgCols.Count > 0)
					dbgRect.OutlineColor = new Color(255, 0, 0, 255);

			context.window.Draw(dbgRect);
		}

		public bool hasChanged() {
			return hasChangedData;
		}

		public void resetChangedFlag() {
			hasChangedData = false;
		}
	}

	public class BoundingBox {
		public BoundingBox() : this(0f, 0f, 0f, 0f) {
		}

		public BoundingBox(float left, float top, float right, float bottom) {
			this.top = top;
			this.left = left;
			this.bottom = bottom;
			this.right = right;

			this.x = this.y = 0f;
			hasChanged = true;
		}

		public BoundingBox(float left, float top, float right, float bottom, float x, float y) 
			: this(left, top, right, bottom){

			this.x = x;
			this.y = y;
		}

		public bool intersects(BoundingBox other) {
			return ((other.x + other.right > x + left) 
				&& (other.x + other.left < x + right) 
				&& (other.y + other.bottom > y + top) 
				&& (other.y + other.top < y + bottom));
		}

		public void copy(BoundingBox other) {
			this.top = other.top;
			this.left = other.left;
			this.right = other.right;
			this.bottom = other.bottom;
			this.x = other.x;
			this.y = other.y;
			hasChanged = true;
		}

		public float top, left, bottom, right, x, y;
		public bool hasChanged;
	}
}
