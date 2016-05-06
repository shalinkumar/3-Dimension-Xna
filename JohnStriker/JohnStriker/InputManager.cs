using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace JohnStriker
{
    public class InputState
    {
        public KeyboardState[] keyboardState;

        public InputState()
        {
            keyboardState=new KeyboardState[2];
            GetInput(false);
        }
        public void GetInput(bool Player)
        {
            if (Player)
            {
                keyboardState[0] = Keyboard.GetState();
               
            }
            else
            {
                keyboardState[1] = Keyboard.GetState();
            }
        }

        public void CopyInput(InputState state)
        {
            keyboardState[0] = state.keyboardState[0];
            keyboardState[1] = state.keyboardState[1];
        }
    }
    public class InputManager
    {
        private InputState currentState; //Current frame input state
        private InputState lastState; //last frame input state

        /// <summary>
        /// Get the current input state
        /// </summary>
        public InputState CurrentState
        {
            get { return currentState; }
        }

        /// <summary>
        /// Get last frame input state
        /// </summary>
        public InputState LastState
        {
            get { return lastState; }
        }

        public InputManager()
        {
            currentState=new InputState();
            lastState=new InputState();
        }
        /// <summary>
        /// Check if a key is pressed down in this player and up in this player.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsKeyPressed(int player, Keys key)
        {
            return currentState.keyboardState[player].IsKeyDown(key) && lastState.keyboardState[player].IsKeyUp(key);
        }

        /// <summary>
        /// Begin input (aqruire input from all controlls)
        /// </summary>
        public void BeginInputProcessing(bool singlePlayer)
        {
            currentState.GetInput(singlePlayer);
        }

        /// <summary>
        /// End input (save current input to last frame input)
        /// </summary>
        public void EndInputProcessing()
        {
            lastState.CopyInput(currentState);
        }

    }
}
