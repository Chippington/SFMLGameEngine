using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.System;

namespace SFMLEngine.Graphics.UI.Controls {
	public class Label : UIControl {
		protected static Font DefaultFont = new Font("Resources/Fonts/MavenPro-Regular.ttf");
		protected Text text;

		public override Vector2f Position { get => base.Position; set {
				base.Position = value;
				if (text != null)
					text.Position = value;
			}
		}

		public Label() : this("", DefaultFont) { }

		public Label(string text) : this(text, DefaultFont) { }

		public Label(string text, Font font) {
			this.text = new Text(text, font);
			this.text.Position = this.Position;
		}

		public override void onInitialize() {
			base.onInitialize();
		}

		public override void updateControl(GameContext context) {
			base.updateControl(context);
		}

		public virtual void setText(string text) {
			this.text.DisplayedString = text;
		}

		public virtual string getText() {
			return text.DisplayedString;
		}

		public virtual void setFont(Font font) {
			this.text.Font = font;
		}

		public virtual Font getFont() {
			return this.text.Font;
		}

		public virtual void setFontSize(uint size) {
			text.CharacterSize = size;
		}

		public virtual uint getFontSize() {
			return text.CharacterSize;
		}

		public virtual void setTextColor(Color color) {
			text.FillColor = color;
		}

		public virtual Color getTextColor() {
			return text.FillColor;
		}

		public override void drawControl(GameContext context, RenderTarget uiWindow) {
			base.drawControl(context, uiWindow);
			if (text == null)
				return;

			drawText(context, uiWindow);
		}

		protected virtual void drawText(GameContext context, RenderTarget uiWindow) {
			uiWindow.Draw(text);
		}
	}
}
