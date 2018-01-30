using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Graphics.UI {
	public class UIControl {
		protected List<UIControl> controls;
		protected Queue<UIControl> graphicsInitQueue;

		private Vector2f position;
		public virtual Vector2f Position { get => position; set => position = value; }

		public virtual void onInitialize() {
			controls = new List<UIControl>();
			graphicsInitQueue = new Queue<UIControl>();
		}

		public virtual void onGraphicsInitialize() { }

		public virtual void onUpdate(GameContext context) {
			if (controls == null)
				return;

			for (int i = 0; i < controls.Count; i++) {
				controls[i].onUpdate(context);
			}
		}

		public virtual void onDraw(GameContext context, RenderTarget target) {
			if (controls == null)
				return;

			while (graphicsInitQueue.Count > 0)
				graphicsInitQueue.Dequeue().onGraphicsInitialize();

			for (int i = 0; i < controls.Count; i++) {
				controls[i].onDraw(context, target);
			}
		}

		public void addControl(UIControl control) {
			control.onInitialize();
			controls.Add(control);
			graphicsInitQueue.Enqueue(control);
		}

		public void addControl<T>(params object[] parameters) where T : UIControl {
			T inst = (T)Activator.CreateInstance(typeof(T), parameters);
			addControl(inst);
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
	}
}
