using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities.Components {
	public class SpriteRenderer : Component {
		private Sprite sprite;
		private Transform transform;
		public override void onInitialize() {
			base.onInitialize();
			transform = entity.components.Add<Transform>();
		}

		public void setSprite(Sprite sprite) {
			this.sprite = sprite;
		}

		public Sprite getSprite() {
			return sprite;
		}

		public override void onUpdate(GameContext context) {
			base.onUpdate(context);
			sprite.Position = new SFML.System.Vector2f(transform.x, transform.y);
		}

		public override void onDraw(GameContext context) {
			base.onDraw(context);
			context.window.Draw(sprite);
		}
	}
}
