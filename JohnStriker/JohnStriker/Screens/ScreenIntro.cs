using System;
using System.Collections.Generic;
using JohnStriker.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JohnStriker.Screens
{
    public class ScreenIntro : Screen
    {
        private const int numberMenuItems = 3;
        private readonly GameManager _gameManager;
        private readonly ScreenManager _screenManager;
        private readonly List<BasicModel> basicModelsList = new List<BasicModel>();
        //string[] menuItems = new string[numberMenuItems] { "Menu_Play", "Menu_Help", "Menu_Quit" };


        //Menu with hover
        private readonly Texture2D[] menuHover = new Texture2D[numberMenuItems];
        private readonly String[] menuItems = new String[numberMenuItems] {"menu_play", "menu_help", "menu_quit"};

        //menu without hover
        private readonly Texture2D[] withoutHover = new Texture2D[numberMenuItems];

        private List<SpinningEnemy> _spinningEnemyList = new List<SpinningEnemy>();
        private Game1 game1;
        private int menuSelection;
        private int menuTime;
        private Texture2D textureCursorAnim;
        private Texture2D textureCursorArrow;
        private Texture2D textureCursorBullet;
        private Texture2D textureLens;
        private Texture2D textureLogo;

        public ScreenIntro(ScreenManager screenManager, GameManager gameManager)
        {
            _screenManager = screenManager;
            _gameManager = gameManager;
            game1 = new Game1();
        }

        public override void Setfocus(ContentManager content, bool focus, GraphicsDevice graphicsDevice)
        {          

            if (focus)
            {
                basicModelsList.Add(new SpinningEnemy(content.Load<Model>(@"Models/spaceship")));
                textureLogo = content.Load<Texture2D>(@"Screens/game_logo");
                textureLens = content.Load<Texture2D>(@"Screens/intro_lens");

                textureCursorAnim = content.Load<Texture2D>(@"Screens/cursor_anim");
                textureCursorBullet = content.Load<Texture2D>(@"Screens/cursor_arrow");
                textureCursorArrow = content.Load<Texture2D>(@"Screens/cursor_bullet");

                //MENU ITEMS
                for (int i = 0; i < numberMenuItems; i++)
                {
                    withoutHover[i] = content.Load<Texture2D>(@"Screens/" + menuItems[i]);

                    menuHover[i] = content.Load<Texture2D>(@"Screens/" + menuItems[i] + "_Hover");
                }
            }
            else
            {
                textureLogo = null;
                textureLens = null;
                textureCursorAnim = null;
                textureCursorBullet = null;
                textureCursorArrow = null;

                for (int i = 0; i < numberMenuItems; i++)
                {
                    withoutHover[i] = null;

                    menuHover[i] = null;
                }
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
                //when up , down key is used to change menu
                if (inputManager.IsKeyPressed(i, Keys.Up))
                {
                    menuSelection = (menuSelection == 0 ? numberMenuItems - 1 : menuSelection - 1);
                    _gameManager.PlaySound("menu_change");
                }
                if (inputManager.IsKeyPressed(i, Keys.Down))
                {
                    menuSelection = ((menuSelection + 1)%numberMenuItems);
                    _gameManager.PlaySound("menu_change");
                }

                if (inputManager.IsKeyPressed(i, Keys.Enter) || inputManager.IsKeyPressed(i, Keys.Space))
                {
                    switch (menuSelection)
                    {
                        case 0:                           
                            _screenManager.SetNextScreen(ScreenType.ScreenGameContent);
                            break;
                        case 1:
                            _screenManager.SetNextScreen(ScreenType.ScreenHelp);
                            break;
                        case 2:
                            _screenManager.Exit();
                            break;
                    }
                    _gameManager.PlaySound("menu_select");
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < basicModelsList.Count; i++)
            {
                basicModelsList[i].Update(gameTime, "ScreenIntro");
            }
            menuTime += gameTime.ElapsedGameTime.Seconds;
        }

        public override void Draw3D(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }

            //Clear background animation
            graphicsDevice.Clear(Color.Black);
            //draw background animation
            _screenManager.DrawBackGround(graphicsDevice);

            foreach (BasicModel bm in basicModelsList)
            {
                bm.Draw(_screenManager.camera);
            }
        }

        public override void Draw2d(GraphicsDevice graphicsDevice, FontManager font)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }

            var rectangle = new Rectangle(graphicsDevice.Viewport.X, graphicsDevice.Viewport.Y,
                graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
            var rectangleLogo = new Rectangle(graphicsDevice.Viewport.X/2, graphicsDevice.Viewport.Y/4,
                graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height/4);
            //draw logo texture
            _screenManager.DrawTexture(textureLogo, rectangleLogo, Color.White, BlendState.Additive);
            //draw menu iterm
            int y = rectangle.Height - 200;
            int x = 240;
            for (int i = 0; i < numberMenuItems; i++)
            {
                if (i == menuSelection)
                {
                    rectangle.X = x;
                    rectangle.Y = y;
                    rectangle.Width = menuHover[i].Width;
                    rectangle.Height = menuHover[i].Height;

                    _screenManager.DrawTexture(menuHover[i], rectangle, Color.White, BlendState.AlphaBlend);
                    // draw cursor left of selected item
                    //  DrawCursor(rect.X - 60, rect.Y + 19);
                    //y += 50;
                    x += 350;
                }
                else
                {
                    rectangle.X = x;
                    rectangle.Y = y;
                    rectangle.Width = withoutHover[i].Width;
                    rectangle.Height = withoutHover[i].Height;

                    _screenManager.DrawTexture(withoutHover[i], rectangle, Color.White, BlendState.AlphaBlend);

                    //y += 40;

                    x += 300;
                }
            }
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