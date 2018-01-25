using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities.Components {
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
				_bounds.x = transform.x;
				_bounds.y = transform.y;
				return _bounds;
			}
		}

		private bool hasChangedData;
		private Transform transform;

		public override void onInitialize() {
			base.onInitialize();
			hasChangedData = true;
			dbgCols = new HashSet<ICollider>();
			transform = entity.components.Add<Transform>();

			dbgPosition = new Vector2f(transform.x + _bounds.left, transform.y + _bounds.top);
			dbgSize = new Vector2f(_bounds.right - _bounds.left, _bounds.bottom - _bounds.top);
		}

		public override void onUpdate(GameContext context) {
			base.onUpdate(context);
			dbgPosition.X = transform.x + _bounds.left;
			dbgPosition.Y = transform.y + _bounds.top;
			dbgRect.Position = dbgPosition;
		}

		public virtual BoundingBox getBoundingBox() {
			return bounds;
		}

		public virtual void setBoundingBox(BoundingBox newBounds) {
			hasChangedData = true;
			_bounds = newBounds;

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

	public struct BoundingBox {
		public BoundingBox(float left, float top, float right, float bottom) {
			this.top = top;
			this.left = left;
			this.bottom = bottom;
			this.right = right;

			this.x = this.y = 0f;
		}

		public float top, left, bottom, right;
		public float x, y;
	}
}
