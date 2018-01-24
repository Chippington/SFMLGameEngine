using SFML.Graphics;
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

		private BoundingBox _bounds;
		private BoundingBox bounds {
			get {
				_bounds.x = position.x;
				_bounds.y = position.y;
				return _bounds;
			}
		}

		private bool hasChangedData;
		private Position position;

		public override void onInitialize() {
			base.onInitialize();
			hasChangedData = true;

			dbgCols = new HashSet<ICollider>();
			position = entity.components.Add<Position>();
		}

		public virtual BoundingBox getBoundingBox() {
			return bounds;
		}

		public virtual void setBoundingBox(BoundingBox newBounds) {
			hasChangedData = true;
			_bounds = newBounds;
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
		}

		public override void onDraw(GameContext context) {
			if (dbgRect == null)
				return;

			base.onDraw(context);
			dbgRect.Position = new SFML.System.Vector2f(_bounds.x + _bounds.left, _bounds.y + bounds.top);
			dbgRect.Size = new SFML.System.Vector2f(_bounds.right - _bounds.left, _bounds.bottom - _bounds.top);
			dbgRect.FillColor = new Color(0, 0, 0, 0);
			dbgRect.OutlineColor = new Color(0, 255, 0, 255);
			if (dbgCols.Count > 0)
				dbgRect.OutlineColor = new Color(255, 0, 0, 255);
			dbgRect.OutlineThickness = 1f;

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
