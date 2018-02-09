using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Window;

namespace SFMLEngine.Graphics.UI.Controls {
	public class Textbox : Button {
		public override Vector2f Position {
			get => base.Position; set {
				base.Position = value;
				if (sprite != null)
					sprite.Position = value;
			}
		}

		private bool active;
		private string placeholder;
		private static RenderTexture _textTexture;

		private Sprite sprite;
		private static RenderTexture textTexture {
			get {
				if (_textTexture == null) {
					_textTexture = new RenderTexture(1000, 100);
				}

				return _textTexture;
			}
		}

		public Textbox()
			: this("") { }

		public Textbox(string text)
			: this(text, "") { }

		public Textbox(string text, string placeholder)
			: this(text, placeholder, new Vector2f()) {	}

		public Textbox(string text, string placeholder, Vector2f size)
			: this(text, placeholder, size, new Vector2f()) { }

		public Textbox(string text, string placeholder, Vector2f size, Vector2f offset)
			: base(text, size, offset) {

			log("Creating target render texture ");
			this.placeholder = placeholder;
			this.sprite = new Sprite(
				textTexture.Texture, 
				new IntRect(0, 0, (int)size.X - (int)offset.X, (int)size.Y - (int)offset.Y));

			this.sprite.Position = Position;
			setBackgroundColor(new Color(122, 122, 122, 122));
			setTextColor(new Color(255, 255, 255));
			setFontSize(24);

			this.OnButtonPressedEvent += onMouseButtonPressed;
		}

		private void onMouseButtonPressed() {
			active = true;
		}

		public override void drawControl(GameContext context, RenderTarget target) {
			_textTexture.Clear();
			base.drawControl(context, target);

			var bounds = text.GetGlobalBounds();
			var width = bounds.Width;

			if (width > getSize().X) {
				//subtract text pos so x + width = 0
				text.Position = new Vector2f(0f - width - offset.X - 3f, 0f);

				//add text pos + (diff = width of textbox)
				text.Position += new Vector2f(getSize().X, 0f);
			} else {
				text.Position = new Vector2f(0f, 0f);
			}

			target.Draw(sprite);
		}

		protected override bool handleInputPressed(Mouse.Button button, GameContext context) {
			if (button == Mouse.Button.Left)
				active = false;

			return base.handleInputPressed(button, context);
		}

		protected override bool handleInput(Keyboard.Key key, GameContext context) {
			if (active == false)
				return base.handleInput(key, context);

			string ch = key.ToString().ToLower();
			if(ch.Length > 1) {
				ch = ch.Replace("num", "");
			}

			if (ch.Length > 1)
				return base.handleInput(key, context);

			if (context.input.isHeld(Keyboard.Key.LShift) || context.input.isHeld(Keyboard.Key.RShift))
				ch = ch.ToUpper();

			var text = getText() + ch;
			setText(text);
			return true;
		}

		protected override void drawText(GameContext context, RenderTarget uiWindow) {
			textTexture.Clear(new Color(0, 0, 0, 0));
			base.drawText(context, textTexture);
			textTexture.Display();
		}
	}
}
