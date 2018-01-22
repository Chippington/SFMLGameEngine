using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities.Components {
	public interface ICollider {
		BoundingBox getBoundingBox();
		void onEnterCollision(ICollider other);
		void onLeaveCollision(ICollider other);
		void onStepCollision(ICollider other);
		bool hasChanged();
	}

	public class RigidBody : Component, ICollider {
		private BoundingBox _bounds;
		private BoundingBox bounds {
			get {
				_bounds.x = entity.position.X;
				_bounds.y = entity.position.Y;
				return _bounds;
			}
		}

		private bool hasChangedData;

		public override void onInitialize() {
			base.onInitialize();
			hasChangedData = true;
		}

		public virtual BoundingBox getBoundingBox() {
			return bounds;
		}

		public virtual void setBoundingBox(BoundingBox newBounds) {
			hasChangedData = true;
			_bounds = newBounds;
		}

		public virtual void onEnterCollision(ICollider other) {
		}

		public virtual void onLeaveCollision(ICollider other) {
		}

		public virtual void onStepCollision(ICollider other) {
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
