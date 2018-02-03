using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.System;
using SFML.Window;

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
		private Queue<Drawable> drawQueue;

		public UIWindow()
			: this("", 0, 0, Style.DEFAULT) { }

		public UIWindow(string title, uint width, uint height)
			: this(title, width, height, Style.DEFAULT) { }

		public UIWindow(string title, uint width, uint height, Style style) {
			this.width = width;
			this.height = height;

			drawQueue = new Queue<Drawable>();
			clearColor = new Color(0, 0, 0, 0);
		}

		public override void onGraphicsInitialize() {
			base.onGraphicsInitialize();
			renderTexture = new RenderTexture(width, height);
			sprite = new Sprite(renderTexture.Texture);
			sprite.Position = this.Position;
		}

		public void setSize(uint width, uint height) {
			tempRenderTexture = new RenderTexture(width, height);
		}

		public Vector2u getSize() {
			return new Vector2u(width, height);
		}

		public uint getWidth() {
			return width;
		}

		public uint getHeight() {
			return height;
		}

		public void setClearColor(Color color) {
			clearColor = color;
		}

		public Color getClearColor() {
			return clearColor;
		}

		internal override void onDraw(GameContext context, RenderTarget target) {
			if (tempRenderTexture != null) {
				renderTexture = tempRenderTexture;
				tempRenderTexture = null;
			}

			renderTexture.Clear(clearColor);
			base.onDraw(context, renderTexture);
			renderTexture.Display();
			while (drawQueue.Count > 0)
				renderTexture.Draw(drawQueue.Dequeue());

			target.Draw(sprite);
		}

		public virtual void draw(Drawable item) {
			drawQueue.Enqueue(item);
		}
	}
}
