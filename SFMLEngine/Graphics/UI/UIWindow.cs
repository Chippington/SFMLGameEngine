using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.System;

namespace SFMLEngine.Graphics.UI {
	public class UIWindow : UIControl {
		public enum Style {
			DEFAULT, NONE,
		}

		public override Vector2f Position { get => base.Position; set {
				base.Position = value;
				if (sprite != null)
					sprite.Position = value;
			}
		}

		private Sprite sprite;
		private Color clearColor;
		private RenderTexture renderTexture;
		private RenderTexture tempRenderTexture;
		private uint width, height;

		public UIWindow()
			: this("", 0, 0, Style.DEFAULT) { }

		public UIWindow(string title, uint width, uint height)
			: this(title, width, height, Style.DEFAULT) { }

		public UIWindow(string title, uint width, uint height, Style style) {
			this.width = width;
			this.height = height;
		}

		public override void onGraphicsInitialize() {
			base.onGraphicsInitialize();
			renderTexture = new RenderTexture(width, height);
			sprite = new Sprite(renderTexture.Texture);
			sprite.Position = this.Position;
			clearColor = new Color(0, 0, 0, 0);
		}

		public void setSize(uint width, uint height) {
			tempRenderTexture = new RenderTexture(width, height);
		}

		public void setClearColor(Color color) {
			clearColor = color;
		}

		public Color getClearColor() {
			return clearColor;
		}

		public override void onUpdate(GameContext context) {
			if (controls == null)
				return;

			for (int i = 0; i < controls.Count; i++) {
				controls[i].onUpdate(context);
			}
		}

		public override void onDraw(GameContext context, RenderTarget target) {
			if (controls == null)
				return;

			while (graphicsInitQueue.Count > 0)
				graphicsInitQueue.Dequeue().onGraphicsInitialize();

			if (tempRenderTexture != null) {
				renderTexture = tempRenderTexture;
				tempRenderTexture = null;
			}

			renderTexture.Clear(new Color(0,0,0,0));
			for(int i = 0; i < controls.Count; i++) {
				controls[i].onDraw(context, renderTexture);
			}

			renderTexture.Display();
			target.Draw(sprite);
		}
	}
}
