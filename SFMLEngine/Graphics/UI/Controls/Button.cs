using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.Window;
using SFML.System;

namespace SFMLEngine.Graphics.UI.Controls {
	public delegate void ButtonEvent();

	public class Button : Label {
		public ButtonEvent OnButtonPressedEvent;
		public ButtonEvent OnButtonReleasedEvent;

		protected RectangleShape rectangleShape;
		protected Vector2f offset;
		protected Vector2f size;
		protected bool pressed;

		public override Vector2f Position { get => base.Position; set {
				base.Position = value;
				resetRect();
			}
		}

		public Button() : this("", new Vector2f()) { }

		public Button(string text, Vector2f size) : this(text, size, new Vector2f(5f, 5f)) { }

		public Button(string text, Vector2f size, Vector2f offset) : base(text) {
			log("Creating button drawable");
			this.rectangleShape = new RectangleShape();
			this.rectangleShape.OutlineColor = new Color(222,222,222);
			this.rectangleShape.OutlineThickness = 2f;
			this.rectangleShape.FillColor = new Color(122, 122, 122);
			this.offset = offset;
			this.size = size;

			this.setTextColor(new Color(0, 0, 0));
			this.setFontSize(14);
			resetRect();
		}

		public virtual void setSize(Vector2f size) {
			this.size = size;
			resetRect();
		}

		public virtual Vector2f getSize() {
			return size;
		}

		public virtual void setOffset(Vector2f offset) {
			this.offset = offset;
			resetRect();
		}

		public virtual Vector2f getOffset() {
			return offset;
		}

		public virtual void setBackgroundColor(Color color) {
			this.rectangleShape.FillColor = color;
		}

		public virtual Color getBackgroundColor() {
			return this.rectangleShape.FillColor;
		}

		public virtual void setBorderColor(Color color) {
			this.rectangleShape.OutlineColor = color;
		}

		public virtual Color getBorderColor() {
			return this.rectangleShape.OutlineColor;
		}

		public virtual void setBorderThickness(float thickness) {
			this.rectangleShape.OutlineThickness = thickness;
		}

		public virtual float getBorderThickness() {
			return this.rectangleShape.OutlineThickness;
		}

		private void resetRect() {
			if (rectangleShape == null)
				return;

			rectangleShape.Position = Position - offset;
			rectangleShape.Size = offset + size;
		}

		protected override bool handleInputPressed(Mouse.Button button, GameContext context) {
			var _mousePos = getRelativeMousePosition(context.input.getMousePosition());
			Vector2f mousePos = new Vector2f((float)_mousePos.X, (float)_mousePos.Y);

			if(mousePos.X > 0f - offset.X && mousePos.X < size.X &&
				mousePos.Y > 0f - offset.Y && mousePos.Y < size.Y) {
				this.pressed = true;
				OnButtonPressedEvent?.Invoke();
				return true;
			}

			return base.handleInputPressed(button, context);
		}

		protected override bool handleInputReleased(Mouse.Button button, GameContext context) {
			if(this.pressed) {
				var _mousePos = getRelativeMousePosition(context.input.getMousePosition());
				Vector2f mousePos = new Vector2f((float)_mousePos.X, (float)_mousePos.Y);

				if (mousePos.X > 0f - offset.X && mousePos.X < size.X &&
					mousePos.Y > 0f - offset.Y && mousePos.Y < size.Y) {
					OnButtonReleasedEvent?.Invoke();
				}

				this.pressed = false;
				return true;
			}

			this.pressed = false;
			return base.handleInputReleased(button, context);
		}

		public override void drawControl(GameContext context, RenderTarget target) {
			target.Draw(rectangleShape);
			base.drawControl(context, target);
		}
	}
}
