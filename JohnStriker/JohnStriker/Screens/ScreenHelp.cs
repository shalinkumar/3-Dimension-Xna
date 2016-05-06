using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace JohnStriker.Screens
{
    class ScreenHelp:Screen
    {
        private ScreenManager _screenManager;
        private GameManager _gameManager;

        public ScreenHelp(ScreenManager screenManager, GameManager gameManager)
        {
            _screenManager = screenManager;
            _gameManager = gameManager;          
        }

        public override void Setfocus(ContentManager content, bool focus, GraphicsDevice graphicsDevice)
        {
            throw new NotImplementedException();
        }

        public override void ProcessInput(GameTime gameTime, InputManager inputManager)
        {
            throw new NotImplementedException();
        }

        public override void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public override void Draw3D(GraphicsDevice graphicsDevice)
        {
            throw new NotImplementedException();
        }

        public override void Draw2d(GraphicsDevice graphicsDevice, FontManager font)
        {
            throw new NotImplementedException();
        }

        public override void DrawText(string text)
        {
            throw new NotImplementedException();
        }
     
    }
}
