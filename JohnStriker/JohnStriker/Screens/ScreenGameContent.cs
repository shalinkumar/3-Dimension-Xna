using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JohnStriker.Screens
{
    public class ScreenGameContent : Screen
    {
        private ScreenManager _screenManager;
        private GameManager _gameManager;

        private Texture2D _gameContenTexture2D;

        private int menuTime;
        private Game1 game1;
        public ScreenGameContent(ScreenManager screenManager, GameManager gameManager)
        {
            this._screenManager = screenManager;
            this._gameManager = gameManager;
            game1=new Game1();

        }
        public override void Setfocus(ContentManager content, bool focus, GraphicsDevice graphicsDevice)
        {
            if (focus)
            {
                _gameContenTexture2D = content.Load<Texture2D>(@"Screens/game_content_1");
            }
            else 
            {
                _gameContenTexture2D = null;
            }
        }

        public override void ProcessInput(GameTime gameTime, InputManager inputManager)
        {
            if (inputManager == null)
            {
                throw new ArgumentNullException("inputManager");
            }

            for (int i = 0; i < 2; i++)
            {
                if (inputManager.IsKeyPressed(i, Keys.Enter))
                {
                    _screenManager.SetNextScreen(ScreenType.ScreenGame);
                    _gameManager.PlaySound("menu_select");
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            //accumulate elapsed time
            menuTime += gameTime.ElapsedGameTime.Seconds;
        }

        public override void Draw3D(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }
            //clear background
            graphicsDevice.Clear(Color.Black);

            _screenManager.DrawBackGround(graphicsDevice);

        }

        public override void Draw2d(GraphicsDevice graphicsDevice, FontManager font)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }
            Rectangle rectangle = new Rectangle(graphicsDevice.Viewport.X, graphicsDevice.Viewport.Y , graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height/3);            
            rectangle.Y = rectangle.Height - 100;
            _screenManager.DrawTexture(_gameContenTexture2D, rectangle, Color.White, BlendState.Additive);
        }

        public override void DrawText(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
        }     
    }
}
