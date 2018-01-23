﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities.Components {
	public class Component : IComponent {
		public IEntity entity;

		public virtual void onInitialize() { }
		public virtual void onDestroy() { }
		public virtual void onUpdate() { }
		public virtual void onDraw() { }
		public void setEntity(IEntity owner) {
			this.entity = owner;
		}
		public IEntity getEntity() {
			return this.entity;
		}
	}
}
