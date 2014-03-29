// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Helpers
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:41

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using DungeonQuest.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using DungeonQuest.Game;
using DungeonQuest.Graphics;
using DungeonQuest.Properties;
#endregion

namespace DungeonQuest.Helpers
{
	/// <summary>
	/// Input helper class, captures all the mouse, keyboard and XBox 360
	/// controller input and provides some nice helper methods and properties.
	/// Will also keep track of the last frame states for comparison if
	/// a button was just pressed this frame, but not already in the last frame.
	/// </summary>
	class Input
	{
		#region Variables
#if !XBOX360
		/// <summary>
		/// Mouse state, set every frame in the Update method.
		/// </summary>
		private static MouseState mouseState,
			mouseStateLastFrame;
#else
		/// <summary>
		/// Just save the mouse position here, it is just virtual on the Xbox.
		/// Clicking is not possible .. we got no mouse support on the Xbox.
		/// </summary>
		private static Point mouseState,
			mouseStateLastFrame;
#endif

		/// <summary>
		/// Was a mouse detected? Returns true if the user moves the mouse.
		/// On the Xbox 360 there will be no mouse movement and theirfore we
		/// know that we don't have to display the mouse.
		/// </summary>
		private static bool mouseDetected = false;

		/// <summary>
		/// Keyboard state, set every frame in the Update method.
		/// Note: KeyboardState is a class and not a struct,
		/// we have to initialize it here, else we might run into trouble when
		/// accessing any keyboardState data before BaseGame.Update() is called.
		/// We can also NOT use the last state because everytime we call
		/// Keyboard.GetState() the old state is useless (see XNA help for more
		/// information, section Input). We store our own array of keys from
		/// the last frame for comparing stuff.
		/// </summary>
		private static KeyboardState keyboardState =
			Microsoft.Xna.Framework.Input.Keyboard.GetState();

		/// <summary>
		/// Keys pressed last frame, for comparison if a key was just pressed.
		/// </summary>
		private static List<Keys> keysPressedLastFrame = new List<Keys>();

		/// <summary>
		/// GamePad state, set every frame in the Update method.
		/// </summary>
		private static GamePadState gamePadState,
			gamePadStateLastFrame;

		/// <summary>
		/// Mouse wheel delta this frame. XNA does report only the total
		/// scroll value, but we usually need the current delta!
		/// </summary>
		/// <returns>0</returns>
#if XBOX360
		private static int mouseWheelDelta = 0;
#else
		private static int mouseWheelDelta = 0, mouseWheelValue = 0;
#endif

		/// <summary>
		/// Start dragging pos, will be set when we just pressed the left
		/// mouse button. Used for the MouseDraggingAmount property.
		/// </summary>
		private static Point startDraggingPos;
		#endregion

		#region Mouse Properties
		/// <summary>
		/// Was a mouse detected? Returns true if the user moves the mouse.
		/// On the Xbox 360 there will be no mouse movement and theirfore we
		/// know that we don't have to display the mouse.
		/// </summary>
		/// <returns>Bool</returns>
		public static bool MouseDetected
		{
			get
			{
				return mouseDetected;
			} // get
		} // MouseDetected
		
		/// <summary>
		/// Mouse position
		/// </summary>
		/// <returns>Point</returns>
		public static Point MousePos
		{
			get
			{
#if XBOX360
				return mouseState;
#else
				return new Point(mouseState.X, mouseState.Y);
#endif
			} // get
			set
			{
#if XBOX360
				mouseState = value;
#else
				Mouse.SetPosition(value.X, value.Y);
#endif
			} // set
		} // MousePos
		
#if !XBOX360
		private static bool mouseWasCentered = false;
#endif
		/// <summary>
		/// Center mouse on screen for optimal mouse movement.
		/// This is required because mouse movement is absolute and
		/// relative mode is not supported like in DirectX.
		/// We have to make sure in the next frame the relative movement will
		/// still be correct!
		/// </summary>
		public static void CenterMouse()
		{
#if !XBOX360
			mouseWasCentered = true;
#endif
		} // CenterMouse()

#if !XBOX360
		/// <summary>
		/// X and y movements of the mouse this frame
		/// </summary>
		private static float mouseXMovement, mouseYMovement,
			lastMouseXMovement, lastMouseYMovement;
#endif

		/// <summary>
		/// Mouse x movement
		/// </summary>
		/// <returns>Float</returns>
		public static float MouseXMovement
		{
			get
			{
#if XBOX360
				return 0;
#else
				return mouseXMovement;
#endif
			} // get
		} // MouseXMovement

		/// <summary>
		/// Mouse y movement
		/// </summary>
		/// <returns>Float</returns>
		public static float MouseYMovement
		{
			get
			{
#if XBOX360
				return 0;
#else
				return mouseYMovement;
#endif
			} // get
		} // MouseYMovement

		/// <summary>
		/// Mouse left button pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool MouseLeftButtonPressed
		{
			get
			{
#if XBOX360
				return false;
#else
				return mouseState.LeftButton == ButtonState.Pressed;
#endif
			} // get
		} // MouseLeftButtonPressed

		/// <summary>
		/// Mouse right button pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool MouseRightButtonPressed
		{
			get
			{
#if XBOX360
				return false;
#else
				return mouseState.RightButton == ButtonState.Pressed;
#endif
			} // get
		} // MouseRightButtonPressed

		/// <summary>
		/// Mouse middle button pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool MouseMiddleButtonPressed
		{
			get
			{
#if XBOX360
				return false;
#else
				return mouseState.MiddleButton == ButtonState.Pressed;
#endif
			} // get
		} // MouseMiddleButtonPressed

		/// <summary>
		/// Mouse left button just pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool MouseLeftButtonJustPressed
		{
			get
			{
#if XBOX360
				return false;
#else
				return mouseState.LeftButton == ButtonState.Pressed &&
					mouseStateLastFrame.LeftButton == ButtonState.Released;
#endif
			} // get
		} // MouseLeftButtonJustPressed

		/// <summary>
		/// Mouse right button just pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool MouseRightButtonJustPressed
		{
			get
			{
#if XBOX360
				return false;
#else
				return mouseState.RightButton == ButtonState.Pressed &&
					mouseStateLastFrame.RightButton == ButtonState.Released;
#endif
			} // get
		} // MouseRightButtonJustPressed

		/// <summary>
		/// Mouse dragging amount
		/// </summary>
		/// <returns>Point</returns>
		public static Point MouseDraggingAmount
		{
			get
			{
				return new Point(
					startDraggingPos.X - MousePos.X,
					startDraggingPos.Y - MousePos.Y);
			} // get
		} // MouseDraggingAmount

		/// <summary>
		/// Reset mouse dragging amount
		/// </summary>
		public static void ResetMouseDraggingAmount()
		{
			startDraggingPos = MousePos;
		} // ResetMouseDraggingAmount()

		/// <summary>
		/// Mouse wheel delta
		/// </summary>
		/// <returns>Int</returns>
		public static int MouseWheelDelta
		{
			get
			{
				return mouseWheelDelta;
			} // get
		} // MouseWheelDelta

		/// <summary>
		/// Mouse in box
		/// </summary>
		/// <param name="rect">Rectangle</param>
		/// <returns>Bool</returns>
		public static bool MouseInBox(Rectangle rect)
		{
			bool ret = mouseState.X >= rect.X &&
				mouseState.Y >= rect.Y &&
				mouseState.X < rect.Right &&
				mouseState.Y < rect.Bottom;
			return ret;
		} // MouseInBox(rect)

		/// <summary>
		/// Mouse was not in rectangle last frame
		/// </summary>
		/// <param name="rect">Rectangle</param>
		public static bool MouseWasNotInRectLastFrame(Rectangle rect)
		{
			// Check the opposite of MouseInBox.
			bool lastRet = mouseStateLastFrame.X >= rect.X &&
				mouseStateLastFrame.Y >= rect.Y &&
				mouseStateLastFrame.X < rect.Right &&
				mouseStateLastFrame.Y < rect.Bottom;
			return !lastRet;
		} // MouseWasNotInRectLastFrame(rect)
		#endregion

		#region Keyboard Properties
		/// <summary>
		/// Keyboard
		/// </summary>
		/// <returns>Keyboard state</returns>
		public static KeyboardState Keyboard
		{
			get
			{
				return keyboardState;
			} // get
		} // Keyboard

		public static bool IsSpecialKey(Keys key)
		{
			// All keys except A-Z, 0-9 and `-\[];',./= (and space) are special keys.
			// With shift pressed this also results in this keys:
			// ~_|{}:"<>? !@#$%^&*().
			int keyNum = (int)key;
			if ((keyNum >= (int)Keys.A && keyNum <= (int)Keys.Z) ||
				(keyNum >= (int)Keys.D0 && keyNum <= (int)Keys.D9) ||
				key == Keys.Space || // well, space ^^
				key == Keys.OemTilde || // `~
				key == Keys.OemMinus || // -_
				key == Keys.OemPipe || // \|
				key == Keys.OemOpenBrackets || // [{
				key == Keys.OemCloseBrackets || // ]}
				key == Keys.OemSemicolon || // ;:
				key == Keys.OemQuotes || // '"
				key == Keys.OemComma || // ,<
				key == Keys.OemPeriod || // .>
				key == Keys.OemQuestion || // /?
				key == Keys.OemPlus) // =+
				return false;

			// Else is is a special key
			return true;
		} // static bool IsSpecialKey(Keys key)

		/// <summary>
		/// Keys to char helper conversion method.
		/// Note: If the keys are mapped other than on a default QWERTY
		/// keyboard, this method will not work properly. Most keyboards
		/// will return the same for A-Z and 0-9, but the special keys
		/// might be different. Sorry, no easy way to fix this with XNA ...
		/// For a game with chat (windows) you should implement the
		/// windows events for catching keyboard input, which are much better!
		/// </summary>
		/// <param name="key">Keys</param>
		/// <returns>Char</returns>
		public static char KeyToChar(Keys key, bool shiftPressed)
		{
			// If key will not be found, just return space
			char ret = ' ';
			int keyNum = (int)key;
			if (keyNum >= (int)Keys.A && keyNum <= (int)Keys.Z)
			{
				if (shiftPressed)
					ret = key.ToString()[0];
				else
					ret = key.ToString().ToLower()[0];
			} // if (keyNum)
			else if (keyNum >= (int)Keys.D0 && keyNum <= (int)Keys.D9 &&
				shiftPressed == false)
			{
				ret = (char)((int)'0' + (keyNum - Keys.D0));
			} // else if
			else if (key == Keys.D1 && shiftPressed)
				ret = '!';
			else if (key == Keys.D2 && shiftPressed)
				ret = '@';
			else if (key == Keys.D3 && shiftPressed)
				ret = '#';
			else if (key == Keys.D4 && shiftPressed)
				ret = '$';
			else if (key == Keys.D5 && shiftPressed)
				ret = '%';
			else if (key == Keys.D6 && shiftPressed)
				ret = '^';
			else if (key == Keys.D7 && shiftPressed)
				ret = '&';
			else if (key == Keys.D8 && shiftPressed)
				ret = '*';
			else if (key == Keys.D9 && shiftPressed)
				ret = '(';
			else if (key == Keys.D0 && shiftPressed)
				ret = ')';
			else if (key == Keys.OemTilde)
				ret = shiftPressed ? '~' : '`';
			else if (key == Keys.OemMinus)
				ret = shiftPressed ? '_' : '-';
			else if (key == Keys.OemPipe)
				ret = shiftPressed ? '|' : '\\';
			else if (key == Keys.OemOpenBrackets)
				ret = shiftPressed ? '{' : '[';
			else if (key == Keys.OemCloseBrackets)
				ret = shiftPressed ? '}' : ']';
			else if (key == Keys.OemSemicolon)
				ret = shiftPressed ? ':' : ';';
			else if (key == Keys.OemQuotes)
				ret = shiftPressed ? '"' : '\'';
			else if (key == Keys.OemComma)
				ret = shiftPressed ? '<' : '.';
			else if (key == Keys.OemPeriod)
				ret = shiftPressed ? '>' : ',';
			else if (key == Keys.OemQuestion)
				ret = shiftPressed ? '?' : '/';
			else if (key == Keys.OemPlus)
				ret = shiftPressed ? '+' : '=';

			// Return result
			return ret;
		} // KeyToChar(key)

		/// <summary>
		/// Handle keyboard input helper method to catch keyboard input
		/// for an input text. Only used to enter the player name in the game.
		/// </summary>
		/// <param name="inputText">Input text</param>
		public static void HandleKeyboardInput(ref string inputText)
		{
			// Is a shift key pressed (we have to check both, left and right)
			bool isShiftPressed =
				keyboardState.IsKeyDown(Keys.LeftShift) ||
				keyboardState.IsKeyDown(Keys.RightShift);

			// Go through all pressed keys
			foreach (Keys pressedKey in keyboardState.GetPressedKeys())
				// Only process if it was not pressed last frame
				if (keysPressedLastFrame.Contains(pressedKey) == false)
				{
					// No special key?
					if (IsSpecialKey(pressedKey) == false &&
						// Max. allow 32 chars
						inputText.Length < 32)
					{
						// Then add the letter to our inputText.
						// Check also the shift state!
						inputText += KeyToChar(pressedKey, isShiftPressed);
					} // if (IsSpecialKey)
					else if (pressedKey == Keys.Back &&
						inputText.Length > 0)
					{
						// Remove 1 character at end
						inputText = inputText.Substring(0, inputText.Length - 1);
					} // else if
				} // foreach if (WasKeyPressedLastFrame)
		} // HandleKeyboardInput(inputText)

		/// <summary>
		/// Keyboard key just pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool KeyboardKeyJustPressed(Keys key)
		{
			return keyboardState.IsKeyDown(key) &&
				keysPressedLastFrame.Contains(key) == false;
		} // KeyboardSpaceJustPressed

		/// <summary>
		/// Keyboard space just pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool KeyboardSpaceJustPressed
		{
			get
			{
				return keyboardState.IsKeyDown(Keys.Space) &&
					keysPressedLastFrame.Contains(Keys.Space) == false;
			} // get
		} // KeyboardSpaceJustPressed

		/// <summary>
		/// Keyboard enter just pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool KeyboardEnterJustPressed
		{
			get
			{
				return keyboardState.IsKeyDown(Keys.Enter) &&
					keysPressedLastFrame.Contains(Keys.Enter) == false;
			} // get
		} // KeyboardEnterJustPressed

		/// <summary>
		/// Keyboard F1 just pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool KeyboardF1JustPressed
		{
			get
			{
				return keyboardState.IsKeyDown(Keys.F1) &&
					keysPressedLastFrame.Contains(Keys.F1) == false;
			} // get
		} // KeyboardF1JustPressed

		/// <summary>
		/// Keyboard F2 just pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool KeyboardF2JustPressed
		{
			get
			{
				return keyboardState.IsKeyDown(Keys.F2) &&
					keysPressedLastFrame.Contains(Keys.F2) == false;
			} // get
		} // KeyboardF2JustPressed

		/// <summary>
		/// Keyboard escape just pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool KeyboardEscapeJustPressed
		{
			get
			{
				return keyboardState.IsKeyDown(Keys.Escape) &&
					keysPressedLastFrame.Contains(Keys.Escape) == false;
			} // get
		} // KeyboardEscapeJustPressed

		/// <summary>
		/// Keyboard left just pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool KeyboardLeftJustPressed
		{
			get
			{
				return keyboardState.IsKeyDown(Keys.Left) &&
					keysPressedLastFrame.Contains(Keys.Left) == false;
			} // get
		} // KeyboardLeftJustPressed

		/// <summary>
		/// Keyboard right just pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool KeyboardRightJustPressed
		{
			get
			{
				return keyboardState.IsKeyDown(Keys.Right) &&
					keysPressedLastFrame.Contains(Keys.Right) == false;
			} // get
		} // KeyboardRightJustPressed

		/// <summary>
		/// Keyboard up just pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool KeyboardUpJustPressed
		{
			get
			{
				return keyboardState.IsKeyDown(Keys.Up) &&
					keysPressedLastFrame.Contains(Keys.Up) == false;
			} // get
		} // KeyboardUpJustPressed

		/// <summary>
		/// Keyboard down just pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool KeyboardDownJustPressed
		{
			get
			{
				return keyboardState.IsKeyDown(Keys.Down) &&
					keysPressedLastFrame.Contains(Keys.Down) == false;
			} // get
		} // KeyboardDownJustPressed

		/// <summary>
		/// Keyboard left pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool KeyboardLeftPressed
		{
			get
			{
				return keyboardState.IsKeyDown(Keys.Left);
			} // get
		} // KeyboardLeftPressed

		/// <summary>
		/// Keyboard right pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool KeyboardRightPressed
		{
			get
			{
				return keyboardState.IsKeyDown(Keys.Right);
			} // get
		} // KeyboardRightPressed

		/// <summary>
		/// Keyboard up pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool KeyboardUpPressed
		{
			get
			{
				return keyboardState.IsKeyDown(Keys.Up);
			} // get
		} // KeyboardUpPressed

		/// <summary>
		/// Keyboard down pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool KeyboardDownPressed
		{
			get
			{
				return keyboardState.IsKeyDown(Keys.Down);
			} // get
		} // KeyboardDownPressed
		#endregion

		#region GamePad Properties
		/// <summary>
		/// Game pad
		/// </summary>
		/// <returns>Game pad state</returns>
		public static GamePadState GamePad
		{
			get
			{
				return gamePadState;
			} // get
		} // GamePad

		/// <summary>
		/// Is game pad connected
		/// </summary>
		/// <returns>Bool</returns>
		public static bool IsGamePadConnected
		{
			get
			{
				return gamePadState.IsConnected;
			} // get
		} // IsGamePadConnected

		/// <summary>
		/// Game pad start pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool GamePadStartPressed
		{
			get
			{
				return gamePadState.Buttons.Start == ButtonState.Pressed;
			} // get
		} // GamePadStartPressed

		/// <summary>
		/// Game pad a pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool GamePadAPressed
		{
			get
			{
				return gamePadState.Buttons.A == ButtonState.Pressed;
			} // get
		} // GamePadAPressed

		/// <summary>
		/// Game pad b pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool GamePadBPressed
		{
			get
			{
				return gamePadState.Buttons.B == ButtonState.Pressed;
			} // get
		} // GamePadBPressed

		/// <summary>
		/// Game pad x pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool GamePadXPressed
		{
			get
			{
				return gamePadState.Buttons.X == ButtonState.Pressed;
			} // get
		} // GamePadXPressed

		/// <summary>
		/// Game pad y pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool GamePadYPressed
		{
			get
			{
				return gamePadState.Buttons.Y == ButtonState.Pressed;
			} // get
		} // GamePadYPressed

		/// <summary>
		/// Game pad left pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool GamePadLeftPressed
		{
			get
			{
				return gamePadState.DPad.Left == ButtonState.Pressed ||
					gamePadState.ThumbSticks.Left.X < -0.75f;
			} // get
		} // GamePadLeftPressed

		/// <summary>
		/// Game pad right pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool GamePadRightPressed
		{
			get
			{
				return gamePadState.DPad.Left == ButtonState.Pressed ||
					gamePadState.ThumbSticks.Left.X > 0.75f;
			} // get
		} // GamePadRightPressed

		/// <summary>
		/// Game pad left just pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool GamePadLeftJustPressed
		{
			get
			{
				return (gamePadState.DPad.Left == ButtonState.Pressed &&
					gamePadStateLastFrame.DPad.Left == ButtonState.Released) ||
					(gamePadState.ThumbSticks.Left.X < -0.75f &&
					gamePadStateLastFrame.ThumbSticks.Left.X > -0.75f);
			} // get
		} // GamePadLeftJustPressed

		/// <summary>
		/// Game pad right just pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool GamePadRightJustPressed
		{
			get
			{
				return (gamePadState.DPad.Right == ButtonState.Pressed &&
					gamePadStateLastFrame.DPad.Right == ButtonState.Released) ||
					(gamePadState.ThumbSticks.Left.X > 0.75f &&
					gamePadStateLastFrame.ThumbSticks.Left.X < 0.75f);
			} // get
		} // GamePadRightJustPressed

		/// <summary>
		/// Game pad up just pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool GamePadUpJustPressed
		{
			get
			{
				return (gamePadState.DPad.Up == ButtonState.Pressed &&
					gamePadStateLastFrame.DPad.Up == ButtonState.Released) ||
					(gamePadState.ThumbSticks.Left.Y > 0.75f &&
					gamePadStateLastFrame.ThumbSticks.Left.Y < 0.75f);
			} // get
		} // GamePadUpJustPressed

		/// <summary>
		/// Game pad down just pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool GamePadDownJustPressed
		{
			get
			{
				return (gamePadState.DPad.Down == ButtonState.Pressed &&
					gamePadStateLastFrame.DPad.Down == ButtonState.Released) ||
					(gamePadState.ThumbSticks.Left.Y < -0.75f &&
					gamePadStateLastFrame.ThumbSticks.Left.Y > -0.75f);
			} // get
		} // GamePadDownJustPressed
		
		/// <summary>
		/// Game pad up pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool GamePadUpPressed
		{
			get
			{
				return gamePadState.DPad.Down == ButtonState.Pressed ||
					gamePadState.ThumbSticks.Left.Y > 0.75f;
			} // get
		} // GamePadUpPressed

		/// <summary>
		/// Game pad down pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool GamePadDownPressed
		{
			get
			{
				return gamePadState.DPad.Up == ButtonState.Pressed ||
					gamePadState.ThumbSticks.Left.Y < -0.75f;
			} // get
		} // GamePadDownPressed

		/// <summary>
		/// Game pad a just pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool GamePadAJustPressed
		{
			get
			{
				return gamePadState.Buttons.A == ButtonState.Pressed &&
					gamePadStateLastFrame.Buttons.A == ButtonState.Released;
			} // get
		} // GamePadAJustPressed

		/// <summary>
		/// Game pad b just pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool GamePadBJustPressed
		{
			get
			{
				return gamePadState.Buttons.B == ButtonState.Pressed &&
					gamePadStateLastFrame.Buttons.B == ButtonState.Released;
			} // get
		} // GamePadBJustPressed

		/// <summary>
		/// Game pad x just pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool GamePadXJustPressed
		{
			get
			{
				return gamePadState.Buttons.X == ButtonState.Pressed &&
					gamePadStateLastFrame.Buttons.X == ButtonState.Released;
			} // get
		} // GamePadXJustPressed

		/// <summary>
		/// Game pad y just pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool GamePadYJustPressed
		{
			get
			{
				return gamePadState.Buttons.Y == ButtonState.Pressed &&
					gamePadStateLastFrame.Buttons.Y == ButtonState.Released;
			} // get
		} // GamePadYJustPressed

		/// <summary>
		/// Game pad back just pressed
		/// </summary>
		/// <returns>Bool</returns>
		public static bool GamePadBackJustPressed
		{
			get
			{
				return gamePadState.Buttons.Back == ButtonState.Pressed &&
					gamePadStateLastFrame.Buttons.Back == ButtonState.Released;
			} // get
		} // GamePadBackJustPressed

        /// <summary>
        /// GamePadLeftShoulderJustPressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool GamePadLeftShoulderJustPressed
        {
            get
            {
                return gamePadState.Buttons.LeftShoulder == ButtonState.Pressed &&
                    gamePadStateLastFrame.Buttons.LeftShoulder == ButtonState.Released;
            } // get
        } // GamePadLeftShoulderJustPressed

        /// <summary>
        /// GamePadRightShoulderJustPressed
        /// </summary>
        /// <returns>Bool</returns>
        public static bool GamePadRightShoulderJustPressed
        {
            get
            {
                return gamePadState.Buttons.RightShoulder == ButtonState.Pressed &&
                    gamePadStateLastFrame.Buttons.RightShoulder == ButtonState.Released;
            } // get
        } // GamePadRightShoulderJustPressed

		private static float leftRumble = 0,
			rightRumble = 0;
		/// <summary>
		/// Game pad rumble
		/// </summary>
		/// <param name="setLeftRumble">Set left rumble</param>
		/// <param name="setRightRumble">Set right rumble</param>
		public static void GamePadRumble(float setLeftRumble, float setRightRumble)
		{
			if (setLeftRumble > leftRumble)
				leftRumble = setLeftRumble;
			if (setRightRumble > rightRumble)
				rightRumble = setRightRumble;
		} // GamePadRumble(setLeftRumble, setRightRumble)
		#endregion

		#region Update
		/// <summary>
		/// Update, called from BaseGame.Update().
		/// Will catch all new states for keyboard, mouse and the gamepad.
		/// </summary>
		internal static void Update()
		{
#if XBOX360
			// No mouse support on the XBox360 yet :(
			mouseDetected = false;
			// Copy over old mouse state
			mouseStateLastFrame = mouseState;
#else
			// Handle mouse input variables
			mouseStateLastFrame = mouseState;
			mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            
			// Update mouseXMovement and mouseYMovement
			Point lastMousePos = new Point(
				mouseStateLastFrame.X, mouseStateLastFrame.Y);
			lastMouseXMovement += (mouseState.X - lastMousePos.X) * GameSettings.Default.ControllerSensibility;
            lastMouseYMovement += (mouseState.Y - lastMousePos.Y) * GameSettings.Default.ControllerSensibility;
			mouseXMovement = lastMouseXMovement / 2.0f;
			mouseYMovement = lastMouseYMovement / 2.0f;
			lastMouseXMovement -= lastMouseXMovement / 2.0f;
			lastMouseYMovement -= lastMouseYMovement / 2.0f;

			if (mouseWasCentered &&
				BaseGame.IsWindowActive)
			{
				mouseWasCentered = false;
				MousePos = new Point(BaseGame.Width / 2, BaseGame.Height / 2);
				mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
			} // if

			if (MouseLeftButtonPressed == false)
				startDraggingPos = MousePos;
			mouseWheelDelta = mouseState.ScrollWheelValue - mouseWheelValue;
			mouseWheelValue = mouseState.ScrollWheelValue;

			// Check if mouse was moved this frame if it is not detected yet.
			// This allows us to ignore the mouse even when it is captured
			// on a windows machine if just the gamepad or keyboard is used.
			if (mouseDetected == false)// &&
				//always returns false: Microsoft.Xna.Framework.Input.Mouse.IsCaptured)
				mouseDetected = mouseState.X != mouseStateLastFrame.X ||
					mouseState.Y != mouseStateLastFrame.Y ||
					mouseState.LeftButton != mouseStateLastFrame.LeftButton;
#endif

			// Handle keyboard input
			keysPressedLastFrame = new List<Keys>(keyboardState.GetPressedKeys());
			keyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();

			// And finally catch the XBox Controller input (only use 1 player here)
			gamePadStateLastFrame = gamePadState;
			gamePadState = Microsoft.Xna.Framework.Input.GamePad.GetState(
				PlayerIndex.One);
			// Try player index two
			if (gamePadState.IsConnected == false)
				gamePadState = Microsoft.Xna.Framework.Input.GamePad.GetState(
					PlayerIndex.Two);

			// Handle rumbeling
			if (leftRumble > 0 || rightRumble > 0)
			{
				if (leftRumble > 0)
					leftRumble -= 0.85f * BaseGame.MoveFactorPerSecond;
				if (rightRumble > 0)
					rightRumble -= 0.85f * BaseGame.MoveFactorPerSecond;
				if (leftRumble < 0)
					leftRumble = 0;
				if (rightRumble < 0)
					rightRumble = 0;
				Microsoft.Xna.Framework.Input.GamePad.SetVibration(
					PlayerIndex.One, leftRumble, rightRumble);
			} // if (leftRumble)
		} // Update()
		#endregion

		#region Unit testing
#if DEBUG
		public static void TestXboxControllerInput()
		{
			TestGame.Start(
				delegate
				{
					if (Input.GamePadAJustPressed)
						Input.GamePadRumble(0.35f, 0.475f);
					else if (Input.GamePadBJustPressed)
						Input.GamePadRumble(0.85f, 0.95f);

					TextureFont.WriteText(30, 60, "Press A for a little gamepad rumble"+
						"and B for heavy gamepad rumble effect");
				});
		} // TestXboxControllerInput()
#endif
		#endregion
	} // class Input
} // namespace DungeonQuest.Helpers
