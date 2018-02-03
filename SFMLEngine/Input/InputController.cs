using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Window;
using SFML.System;

namespace SFMLEngine.Input {
	public class InputController {
		private HashSet<Keyboard.Key> heldMap;
		private HashSet<Keyboard.Key> pressedMap;
		private HashSet<Keyboard.Key> releasedMap;
		private HashSet<Keyboard.Key> tempPressedMap;
		private HashSet<Keyboard.Key> tempReleasedMap;

		private HashSet<Mouse.Button> mouseHeldMap;
		private HashSet<Mouse.Button> mousePressedMap;
		private HashSet<Mouse.Button> mouseReleasedMap;
		private HashSet<Mouse.Button> tempMousePressedMap;
		private HashSet<Mouse.Button> tempMouseReleasedMap;

		private Vector2i mousePos;

		public InputController() {
			heldMap = new HashSet<Keyboard.Key>();
			pressedMap = new HashSet<Keyboard.Key>();
			releasedMap = new HashSet<Keyboard.Key>();
			tempPressedMap = new HashSet<Keyboard.Key>();
			tempReleasedMap = new HashSet<Keyboard.Key>();

			mouseHeldMap = new HashSet<Mouse.Button>();
			mousePressedMap = new HashSet<Mouse.Button>();
			mouseReleasedMap = new HashSet<Mouse.Button>();
			tempMousePressedMap = new HashSet<Mouse.Button>();
			tempMouseReleasedMap = new HashSet<Mouse.Button>();
		}

		public void resetFlags(Keyboard.Key key) {
			heldMap.Remove(key);
			pressedMap.Remove(key);
			releasedMap.Remove(key);
			tempPressedMap.Remove(key);
			tempReleasedMap.Remove(key);
		}

		public void resetFlags(Mouse.Button button) {
			mouseHeldMap.Remove(button);
			mousePressedMap.Remove(button);
			mouseReleasedMap.Remove(button);
			tempMousePressedMap.Remove(button);
			tempMouseReleasedMap.Remove(button);
		}

		public void setHooks(RenderWindow window) {
			window.SetKeyRepeatEnabled(false);
			window.MouseButtonPressed += onMouseButtonPressed;
			window.MouseButtonReleased += onMouseButtonReleased;
			window.KeyPressed += onKeyPressed;
			window.KeyReleased += onKeyReleased;
			window.MouseMoved += onMouseMoved;
		}

		private void onMouseMoved(object sender, MouseMoveEventArgs e) {
			mousePos = new Vector2i(e.X, e.Y);
		}

		private void onMouseButtonPressed(object sender, MouseButtonEventArgs e) {
			if (tempMousePressedMap.Contains(e.Button))
				return;

			if (mousePressedMap.Contains(e.Button))
				return;

			tempMousePressedMap.Add(e.Button);
		}

		private void onMouseButtonReleased(object sender, MouseButtonEventArgs e) {
			if (tempMouseReleasedMap.Contains(e.Button))
				return;

			if (mouseReleasedMap.Contains(e.Button))
				return;

			tempMouseReleasedMap.Add(e.Button);
		}

		public bool isHeld(Keyboard.Key key) {
			lock (heldMap)
				return heldMap.Contains(key);
		}

		public bool isPressed(Keyboard.Key key) {
			lock (pressedMap)
				return pressedMap.Contains(key);
		}

		public bool isReleased(Keyboard.Key key) {
			lock (releasedMap)
				return releasedMap.Contains(key);
		}

		public IEnumerable<Keyboard.Key> getPressedKeyboardKeys() {
			return pressedMap.AsEnumerable();
		}

		public IEnumerable<Mouse.Button> getPressedMouseButtons() {
			return mousePressedMap.AsEnumerable();
		}

		public IEnumerable<Mouse.Button> getReleasedMouseButtons() {
			return mouseReleasedMap.AsEnumerable();
		}

		public Vector2i getMousePosition() {
			return mousePos;
		}

		public void updateInput() {
			lock (pressedMap)
				lock (heldMap)
					foreach (var v in pressedMap)
						if(heldMap.Contains(v) == false)
							heldMap.Add(v);

			lock (releasedMap) lock (tempPressedMap) {
					releasedMap = new HashSet<Keyboard.Key>(tempReleasedMap);
					tempReleasedMap.Clear();
				}

			lock (pressedMap) lock (tempPressedMap) {
					pressedMap = new HashSet<Keyboard.Key>(tempPressedMap);
					tempPressedMap.Clear();
				}

			lock (mousePressedMap) lock (tempMousePressedMap) {
					mousePressedMap = new HashSet<Mouse.Button>(tempMousePressedMap);
					tempMousePressedMap.Clear();
				}

			lock (mouseReleasedMap) lock (tempMouseReleasedMap) {
					mouseReleasedMap = new HashSet<Mouse.Button>(tempMouseReleasedMap);
					tempMouseReleasedMap.Clear();
				}

			lock (releasedMap)
				lock (heldMap)
					foreach (var v in releasedMap)
						if (heldMap.Contains(v))
							heldMap.Remove(v);
		}

		private void onKeyReleased(object sender, KeyEventArgs e) {
			if (tempReleasedMap.Contains(e.Code))
				return;

			if (releasedMap.Contains(e.Code))
				return;

			tempReleasedMap.Add(e.Code);
		}

		private void onKeyPressed(object sender, KeyEventArgs e) {
			if (tempPressedMap.Contains(e.Code))
				return;

			if (pressedMap.Contains(e.Code))
				return;

			tempPressedMap.Add(e.Code);
		}
	}
}
