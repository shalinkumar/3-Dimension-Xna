using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JohnStriker.Models
{
    public class SnowTerrain : BasicModel
    {
        public SnowTerrain(Model m)
            : base(m)
        {
        }

        public SnowTerrain(Model model, SpriteFont font, Vector3 position, Vector3 rotation, Vector3 scale, GraphicsDevice graphicsDevice)
            : base(model, font, position, rotation, scale, graphicsDevice)
        {
        }

        public override void Update(GameTime gameTime, string screen)
        {
            base.Update(gameTime, screen);
        }

        public override void Draw(Camera.Camera camera)
        {
            base.Draw(camera);
        }

    }
}
