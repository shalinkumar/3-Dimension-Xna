using System;
using JohnStriker.Camera;
using JohnStriker.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace JohnStriker.Screens
{
    internal class ScreenGame : Screen
    {
        private readonly GameManager gameManager;

        //List<BasicModel> basicModelsList = new List<BasicModel>();
        //List<FlightModel> flightModellList = new List<FlightModel>();

        // Shot stuff
        private M16D[] players = new M16D[GameOptions.MaxPlayers];
        private ScreenManager screenManager;


        //public EntityList FlightEntityList
        //{
        //    get
        //    {
        //        return flightEntityList;
        //    }
        //    set
        //    {
        //        flightEntityList = value; 
        //    }
        //}
        //public ScreenGame(EntityList entities)
        //{
        //   // flightEntityList = new EntityList();
        //    flightEntityList = entities;
        //}

        public ScreenGame(ScreenManager screenManager, GameManager gameManager)
        {
            this.screenManager = screenManager;
            this.gameManager = gameManager;
        }

        public ChaseCamera chaseCamera { get; set; }

        //called before screen shows
        public override void Setfocus(ContentManager content, bool focus, GraphicsDevice graphicsDevice)
        {
            if (focus)
            {
                gameManager.LoadFiles(content, graphicsDevice);

                //basicModelsList.Add(new SpinningEnemy(content.Load<Model>(@"Models/CrossHair")));
            }
            else
            {
                gameManager.UnloadFiles();
            }
        }


        public override void ProcessInput(GameTime gameTime, InputManager inputManager)
        {
            if (inputManager == null)
            {
                throw new ArgumentNullException("input");
            }

            gameManager.ProcessInput(gameTime, inputManager);
        }


        public override void Update(GameTime gameTime)
        {
            gameManager.Update(gameTime);

            //for (int i = 0; i < basicModelsList.Count; i++)
            //{
            //    basicModelsList[i].Update(elapsedTime, "JohnStriker.Screens.ScreenGame");
            //}         
        }


        public override void Draw3D(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }

            //clear the background
            graphicsDevice.Clear(Color.Black);

            //this is used to draw the backgroun like sky

            gameManager.Draw3D(graphicsDevice);

            //foreach (BasicModel bm in basicModelsList)
            //{
            //    bm.Draw(screenManager.CrossHaircamera);
            //}
        }


        public override void Draw2d(GraphicsDevice graphicsDevice, FontManager font)
        {
            gameManager.Draw2D(font, graphicsDevice);
        }


        public override void DrawText(string text)
        {
            gameManager.DrawText(text);
        }
    }
}