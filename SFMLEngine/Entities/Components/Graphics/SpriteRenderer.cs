using SFML.Graphics;
using SFML.System;
using SFMLEngine.Entities.Components;
using SFMLEngine.Entities.Components.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities.Graphics.Components {
	public class SpriteRenderer : Component {
		private Sprite sprite;
		private Position transform;
		private Vector2f origin;
		private Vector2f scale;

		public override void onInitialize() {
			base.onInitialize();
			scale = new Vector2f(1f, 1f);
			origin = new Vector2f(0f, 0f);
			transform = entity.components.Add<Position>();
		}

		public void setSprite(Sprite sprite) {
			this.sprite = new Sprite(sprite);
			this.sprite.Origin = origin;
			this.sprite.Scale = scale;
		}

		public Sprite getSprite() {
			return sprite;
		}

		public void setOrigin(Vector2f origin) {
			this.origin = origin;
			if (sprite != null)
				sprite.Origin = origin;
		}

		public Vector2f getOrigin() {
			return origin;
		}

		public void setScale(Vector2f scale) {
			this.scale = scale;
			if (sprite != null)
				sprite.Scale = scale;
		}

		public Vector2f getScale() {
			return scale;
		}

		public override void onUpdate(GameContext context) {
			base.onUpdate(context);
			if (sprite == null)
				return;

			sprite.Position = new SFML.System.Vector2f(transform.x, transform.y);
			sprite.Rotation = transform.angle;
		}

		public override void onDraw(GameContext context) {
			base.onDraw(context);

			if(sprite != null)
				context.window.Draw(sprite);
		}
	}
}
