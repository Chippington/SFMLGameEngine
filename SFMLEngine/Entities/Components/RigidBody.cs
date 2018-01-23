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
		}

		public virtual void onLeaveCollision(ICollider other) {
			onCollisionLeave?.Invoke(new CollisionEventArgs() {
				other = other,
			});
		}

		public virtual void onStepCollision(ICollider other) {
			onCollisionStep?.Invoke(new CollisionEventArgs() {
				other = other,
			});
		}

		public bool hasChanged() {
			return hasChangedData;
		}

		public void resetChangedFlag() {
			hasChangedData = false;
		}
	}

	public struct BoundingBox {
		public float top, left, bottom, right;
		public float x, y;
	}
}
