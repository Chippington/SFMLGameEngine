using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Graphics.UI {
	public class UIControl : ObjectBase {
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

			log("UI Control initialized");
		}

		public virtual void onGraphicsInitialize() { }

		internal virtual void onUpdate(GameContext context) {
			this.updateControl(context);
			if (controls == null)
				return;

			for (int i = 0; i < controls.Count; i++) {
				controls[i].onUpdate(context);
			}
		}

		internal virtual void onDraw(GameContext context, RenderTarget target) {
			this.drawControl(context, target);
			if (controls == null)
				return;

			while (graphicsInitQueue.Count > 0)
				graphicsInitQueue.Dequeue().onGraphicsInitialize();

			for (int i = 0; i < indices.Count; i++) {
				var list = controlMap[i];
				for (int j = 0; j < list.Count; j++) {
					list[j].onDraw(context, target);
				}
			}
		}

		public virtual void drawControl(GameContext context, RenderTarget target) {

		}

		public virtual void updateControl(GameContext context) {

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

		internal bool handleKeyboardInput(Keyboard.Key key, GameContext context) {
			if (handleInput(key, context))
				return true;

			foreach (var control in getControls())
				if (control.handleKeyboardInput(key, context))
					return true;

			return false;
		}

		internal bool handleMouseInputPressed(Mouse.Button button, GameContext context) {
			if (handleInputPressed(button, context))
				return true;

			foreach (var control in getControls())
				if (control.handleMouseInputPressed(button, context))
					return true;

			return false;
		}

		internal bool handleMouseInputReleased(Mouse.Button button, GameContext context) {
			if (handleInputReleased(button, context))
				return true;

			foreach (var control in getControls())
				if (control.handleMouseInputReleased(button, context))
					return true;

			return false;
		}

		internal bool handleMouseInput(Mouse.Wheel wheel, GameContext context) {
			if (handleInput(wheel, context))
				return true;

			foreach (var control in getControls())
				if (control.handleMouseInput(wheel, context))
					return true;

			return false;
		}

		protected virtual bool handleInput(Keyboard.Key key, GameContext context) {
			return false;
		}

		protected virtual bool handleInputPressed(Mouse.Button button, GameContext context) {
			return false;
		}

		protected virtual bool handleInputReleased(Mouse.Button button, GameContext context) {
			return false;
		}

		protected virtual bool handleInput(Mouse.Wheel wheel, GameContext context) {
			return false;
		}

		public virtual Vector2i getRelativeMousePosition(Vector2i rawMousePos) {
			if (parent != null) {
				Vector2i parentPos = parent.getRelativeMousePosition(rawMousePos);
				return new Vector2i(
					parentPos.X - (int)position.X, 
					parentPos.Y - (int)position.Y);
			}

			return new Vector2i(
				rawMousePos.X - (int)position.X, 
				rawMousePos.Y - (int)position.Y);
		}
	}
}
