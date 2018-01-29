using SFML.Graphics;
using SFML.System;
using SFMLEngine.Entities.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Entities.Components.Camera {
	public class CameraComponent : Component {
		private RenderTexture renderTexture;
		private View view;

		public override void onInitialize() {
			base.onInitialize();
			view = new View();
		}

		public void setViewport(FloatRect viewport) {
			view.Viewport = viewport;
		}

		public void setPosition(Vector2f position) {
			view.Center = position;
		}

		public void setSize(Vector2f size) {
			view.Size = size;
		}

		public FloatRect getViewport() {
			return view.Viewport;
		}

		public Vector2f getPosition() {
			return view.Center;
		}

		public Vector2f getSize() {
			return view.Size;
		}

		public View getView() {
			return view;
		}
	}
}
