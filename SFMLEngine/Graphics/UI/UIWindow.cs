using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Graphics.UI {
	public class UIWindow {
		public enum Style {
			DEFAULT, NONE,
		}

		private List<UIControl> controls;
		private RenderTexture renderTexture;
		private RenderTexture tempRenderTexture;

		public UIWindow()
			: this("", 0, 0, Style.DEFAULT) { }

		public UIWindow(string title, uint width, uint height)
			: this(title, width, height, Style.DEFAULT) { }

		public UIWindow(string title, uint width, uint height, Style style) {
			controls = new List<UIControl>();
			renderTexture = new RenderTexture(width, height);
		}

		public void setSize(uint width, uint height) {
			tempRenderTexture = new RenderTexture(width, height);
		}

		public void addControl(UIControl control) {
			controls.Add(control);
		}

		public IEnumerable<T> getControls<T>() where T : UIControl {
			return getControls(typeof(T)).Cast<T>();
		}

		public IEnumerable<UIControl> getControls(Type type) {
			var ret = from c in controls
					  where c.GetType() == type
					  select c;

			return ret;
		}

		public IEnumerable<UIControl> getControls() {
			return controls;
		}

		public void onDraw(GameContext context) {
			if(tempRenderTexture != null) {
				renderTexture = tempRenderTexture;
				tempRenderTexture = null;
			}
		}
	}
}
