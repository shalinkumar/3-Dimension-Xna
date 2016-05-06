using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JohnStriker.Models
{
    public class SpinningEnemy :BasicModel
    {
        private Matrix rotationMatrix = Matrix.Identity;

        private float yawAngle;
        private float pitchAngle;
        private float rollAngle;
        private Vector3 direction;

        public SpinningEnemy(Model m) : base(m)
        {
        }

        public SpinningEnemy(Model model, Vector3 position, Vector3 vector3direction, float yaw, float pitch, float roll)
            : base(model, position, vector3direction, yaw, pitch, roll)
        {
            World = Matrix.CreateTranslation(position);
            yawAngle = yaw;
            pitchAngle = pitch;
            rollAngle = roll;
            direction = vector3direction;
        }

        public override void Update(GameTime gameTime, string screen)
        {
            if (screen == "JohnStriker.Screens.ScreenGame")
            {
                rotationMatrix *= Matrix.CreateRotationX(MathHelper.Pi / 180);
            }
            else if (screen == "ScreenIntro")
            {
                rotationMatrix *= Matrix.CreateRotationY(MathHelper.Pi / 180);   
            }
            else if(screen=="Ammo")
            {
                rotationMatrix = Matrix.CreateFromYawPitchRoll(yawAngle,pitchAngle,rollAngle);

                World *= Matrix.CreateTranslation(direction);
            }
            base.Update(gameTime, screen);
        }

        public override Matrix GetWorld()
        {
            return World * rotationMatrix;
            //return base.GetWorld() * rotationMatrix;
        }

        public override void Draw(Camera.Camera camera)
        {

            base.Draw(camera);
        }
    }
}
