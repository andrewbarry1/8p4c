using System;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace JamSeven
{
	public class InputHandler
	{

		private Dictionary<Keys,bool> keyStates;
        private int leftClick;

        Dictionary<char, bool> gp1Buttons;
        Dictionary<char, bool> gp2Buttons;
        Dictionary<char, bool> gp3Buttons;
        Dictionary<char, bool> gp4Buttons;
        public Vector2[] sticks;


        public Vector2 mousePosition;

		public InputHandler ()
		{
			keyStates = new Dictionary<Keys, bool>();
            gp1Buttons = new Dictionary<char, bool>();
            gp2Buttons = new Dictionary<char, bool>();
            gp3Buttons = new Dictionary<char, bool>();
            gp4Buttons = new Dictionary<char, bool>();
            sticks = new Vector2[8];
		}

        public void reset()
        {
            keyStates = new Dictionary<Keys, bool>();
            gp1Buttons = new Dictionary<char, bool>();
            gp2Buttons = new Dictionary<char, bool>();
            gp3Buttons = new Dictionary<char, bool>();
            gp4Buttons = new Dictionary<char, bool>();
        }

		// update key states
		public void update(KeyboardState state) {
			Keys[] pressedKeys = state.GetPressedKeys ();


			List<Keys> handledKeys = new List<Keys>(keyStates.Keys);
			foreach (Keys k in handledKeys) {
				if (!((IList<Keys>)pressedKeys).Contains (k))
					keyStates.Remove (k);
			}

			foreach (Keys k in pressedKeys) {
				if (keyStates.ContainsKey (k)) {
					bool isPressed = keyStates [k];	
					if (isPressed)	
						keyStates [k] = false;
				} else {
					keyStates.Add (k, true);
				}
			}

		}


		// return true if a single keypress event is allowed.
		public bool allowSinglePress(Keys k) {
			if (keyStates.ContainsKey(k))
				return keyStates [k];
			return false;
		}

		// return true if key is being held down
		public bool allowMultiPress(Keys k) {
			return (keyStates.ContainsKey(k));
		}

        public void updateMouse(MouseState state)
        {
            if (state.LeftButton == ButtonState.Released)
            {
                leftClick = 0;
            }
            else if (state.LeftButton == ButtonState.Pressed)
            {
                if (leftClick == 0) leftClick = 1;
                else leftClick = 2;
            }

            mousePosition = new Vector2(state.Position.X, state.Position.Y);
        }

        public bool allowSingleClick()
        {
            return (leftClick == 1);
        }

        public bool allowMultiClick()
        {
            return (leftClick != 0);
        }

        void updateSinglePad(GamePadState state, Dictionary<char, bool> dict)
        {
            List<char> handledButtons = new List<char>(dict.Keys);

            if (!state.IsConnected) // controller disconnected, thus pressing nothing.
            {
                foreach (char c in handledButtons)
                {
                    dict.Remove(c);
                }
                return;
            }

            // LEFT BUMPER
            if (state.Buttons.LeftShoulder == ButtonState.Pressed)
            {
                if (handledButtons.Contains('l') && dict['l']) {
                    dict['l'] = false;
                }
                else if (!handledButtons.Contains('l'))
                {
                    dict['l'] = true;
                }
            }
            else if (handledButtons.Contains('l'))
            {
                dict.Remove('l');
            }
            // RIGHT BUMPER
            if (state.Buttons.RightShoulder == ButtonState.Pressed)
            {
                if (handledButtons.Contains('r') && dict['r'])
                {
                    dict['r'] = false;
                }
                else if (!handledButtons.Contains('r'))
                {
                    dict['r'] = true;
                }
            }
            else if (handledButtons.Contains('r'))
            {
                dict.Remove('r');
            }
            // LEFT STICK
            if (state.Buttons.LeftStick == ButtonState.Pressed)
            {
                if (handledButtons.Contains('j') && dict['j'])
                {
                    dict['j'] = false;
                }
                else if (!handledButtons.Contains('j'))
                {
                    dict['j'] = true;
                }
            }
            else if (handledButtons.Contains('j'))
            {
                dict.Remove('j');
            }
            // RIGHT STICK
            if (state.Buttons.RightStick == ButtonState.Pressed)
            {
                if (handledButtons.Contains('k') && dict['k'])
                {
                    dict['k'] = false;
                }
                else if (!handledButtons.Contains('k'))
                {
                    dict['k'] = true;
                }
            }
            else if (handledButtons.Contains('k'))
            {
                dict.Remove('k');
            }
        }

        public void updateGamepads(GamePadState one, GamePadState two, GamePadState three, GamePadState four)
        {
            updateSinglePad(one, gp1Buttons);
            sticks[0] = one.ThumbSticks.Left;
            sticks[1] = one.ThumbSticks.Right;
            updateSinglePad(two, gp2Buttons);
            sticks[2] = two.ThumbSticks.Left;
            sticks[3] = two.ThumbSticks.Right;
            updateSinglePad(three, gp3Buttons);
            sticks[4] = three.ThumbSticks.Left;
            sticks[5] = three.ThumbSticks.Right;
            updateSinglePad(four, gp4Buttons);
            sticks[6] = four.ThumbSticks.Left;
            sticks[7] = four.ThumbSticks.Right;
        }

        public bool allowSingleGPpress(int gamepad, char button)
        {
            switch (gamepad)
            {
                case 1: return (gp1Buttons.ContainsKey(button) && gp1Buttons[button]);
                case 2: return (gp2Buttons.ContainsKey(button) && gp2Buttons[button]);
                case 3: return (gp3Buttons.ContainsKey(button) && gp2Buttons[button]);
                case 4: return (gp4Buttons.ContainsKey(button) && gp2Buttons[button]);
            }
            return false; // send wrong gamepad number idiot
        }
    }
}

