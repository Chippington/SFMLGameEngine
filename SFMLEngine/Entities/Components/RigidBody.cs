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
	}

	public class RigidBody : Component, ICollider {
		private BoundingBox bounds;

		public override void onInitialize() {
			base.onInitialize();
			entity.OnUpdateEvent += onUpdate;
		}

		private void onUpdate(EntityEventArgs args) {
			bounds.x = entity.position.X;
			bounds.y = entity.position.Y;
		}

		public virtual BoundingBox getBoundingBox() {
			return bounds;
		}

		public virtual void setBoundingBox(BoundingBox bounds) {
			this.bounds = bounds;
		}

		public virtual void onEnterCollision(ICollider other) {
		}

		public virtual void onLeaveCollision(ICollider other) {
		}

		public virtual void onStepCollision(ICollider other) {
		}
	}

	public struct BoundingBox {
		public float top, left, bottom, right;
		public float x, y;
	}
}
