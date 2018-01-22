using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Window;

namespace SFMLEngine.Input {
	public class InputController {
		private HashSet<Keyboard.Key> heldMap;
		private HashSet<Keyboard.Key> pressedMap;
		private HashSet<Keyboard.Key> releasedMap;
		private HashSet<Keyboard.Key> tempPressedMap;
		private HashSet<Keyboard.Key> tempReleasedMap;

		public InputController() {
			heldMap = new HashSet<Keyboard.Key>();
			pressedMap = new HashSet<Keyboard.Key>();
			releasedMap = new HashSet<Keyboard.Key>();
			tempPressedMap = new HashSet<Keyboard.Key>();
			tempReleasedMap = new HashSet<Keyboard.Key>();
		}

		public void setHooks(RenderWindow window) {
			window.KeyPressed += onKeyPressed;
			window.KeyReleased += onKeyReleased;
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

		public void updateInput() {
			foreach (var v in pressedMap)
				if(heldMap.Contains(v) == false)
					heldMap.Add(v);

			releasedMap = new HashSet<Keyboard.Key>(tempReleasedMap);
			pressedMap = new HashSet<Keyboard.Key>(tempPressedMap);

			foreach (var v in releasedMap)
				if (heldMap.Contains(v))
					heldMap.Remove(v);

			tempReleasedMap.Clear();
			tempPressedMap.Clear();
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
