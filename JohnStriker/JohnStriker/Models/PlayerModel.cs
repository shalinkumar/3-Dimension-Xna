using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JohnStriker.Camera;
using JohnStriker.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JohnStriker.Models
{
    public class PlayerModel : BasicModel
    {

        private List<PlayerModel> playerModelList = new List<PlayerModel>();
        public ChaseCamera chaseCamera { get; set; }
      
        public PlayerModel(Model m)
            : base(m)
        {
        }

        public PlayerModel(Model model,SpriteFont font, Vector3 position, Vector3 rotation, Vector3 scale, GraphicsDevice graphicsDevice)
            : base(model,font, position, rotation, scale, graphicsDevice)
        {
            this.model=model;
        }

        public override void Update(GameTime gameTime, string screen)
        {
            UpdateModel(gameTime);
            UpdateCamera();
            base.Update(gameTime, screen);
        }

        private void UpdateModel(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            Vector3 rotChange = new Vector3(0, 0, 0);

            // Determine on which axes the ship should be rotated on, if any
            if (keyState.IsKeyDown(Keys.W))
                rotChange += new Vector3(1, 0, 0);
            if (keyState.IsKeyDown(Keys.S))
                rotChange += new Vector3(-1, 0, 0);
            if (keyState.IsKeyDown(Keys.A))
                rotChange += new Vector3(0, 1, 0);
            if (keyState.IsKeyDown(Keys.D))
                rotChange += new Vector3(0, -1, 0);

            playerModelList[0].v3Rotation += rotChange * .025f;


            // If space isn't down, the ship shouldn't move
            //if (keyState.IsKeyDown(Keys.Space))
            //    return;

            // Determine what direction to move in
            Matrix rotation = Matrix.CreateFromYawPitchRoll(
                playerModelList[0].v3Rotation.Y, playerModelList[0].v3Rotation.X, playerModelList[0].v3Rotation.Z);

            if (keyState.IsKeyDown(Keys.Space))
            {

                playerModelList[0].v3Position += Vector3.Transform(Vector3.Forward, rotation) * 14f;

            }
            // Move in the direction dictated by our rotation matrix
            playerModelList[0].v3Position += Vector3.Transform(Vector3.Forward, rotation)
                * gameTime.ElapsedGameTime.Seconds * 4;

        }

        private void UpdateCamera()
        {
            //move the camera to the new models position and rotation.

            ((ChaseCamera)chaseCamera).Move(playerModelList[0].v3Position, playerModelList[0].v3Rotation);
            chaseCamera.Update();

        }

        public override void DrawModel(ChaseCamera camera)
        {
            foreach (PlayerModel player in playerModelList)
            {
                if (chaseCamera.BoundingVolumeIsInView(player.BoundingSphere))
                {
                    player.DrawModel(chaseCamera);
                }

            }
            base.DrawModel(camera);
        }
    }
}
