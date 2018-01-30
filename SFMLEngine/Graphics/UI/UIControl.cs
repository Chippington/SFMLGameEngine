using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Graphics.UI {
	public class UIControl {
		private Dictionary<int, List<UIControl>> controlMap;
		private Queue<UIControl> graphicsInitQueue;
		private List<UIControl> controls;
		private Vector2f position;
		private List<int> indices;
		private UIControl parent;
		private int depth;

		public virtual Vector2f Position { get => position; set => position = value; }

		public virtual void onInitialize() {
			graphicsInitQueue = new Queue<UIControl>();

			controls = new List<UIControl>();
			controlMap = new Dictionary<int, List<UIControl>>();
			indices = new List<int>();
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

			for(int i = 0; i < indices.Count; i++) {
				var list = controlMap[i];
				for(int j = 0; j < list.Count; j++) {
					list[j].onDraw(context, target);
				}
			}
		}

		public void addControl(UIControl control) {
			control.parent = this;
			control.onInitialize();
			controls.Add(control);
			graphicsInitQueue.Enqueue(control);

			var index = control.getDepth();
			if (controlMap.ContainsKey(index) == false)
				controlMap.Add(index, new List<UIControl>());

			controlMap[index].Add(control);
			indices = controlMap.Keys
				.Distinct()
				.OrderBy(i => i)
				.ToList();
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

		public void setDepth(int depth) {
			if (parent != null)
				parent.changeDepth(this, this.depth, depth);

			this.depth = depth;
		}

		public int getDepth() {
			return depth;
		}

		public UIControl getParent() {
			return parent;
		}

		private void changeDepth(UIControl control, int old, int ind) {
			if (controlMap.ContainsKey(old) == false)
				return;

			controlMap[old].Remove(control);
			if (controlMap.ContainsKey(ind) == false)
				controlMap.Add(ind, new List<UIControl>());

			controlMap[ind].Add(control);
		}
	}
}
