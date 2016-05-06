using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JohnStriker.Camera;
using JohnStriker.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace JohnStriker.Models
{
    public class SkySphere  : IModel
    {

        private CModel model;
        Effect effect;
        GraphicsDevice graphics;
        public ChaseCamera chaseCamera { get; set; }
        public SkySphere(ContentManager Content,
            GraphicsDevice graphicsDevice, TextureCube texture)          
        {         
            model=new CModel(Content.Load<Model>("Models/skysphere_mesh"),
                 Vector3.Zero, Vector3.Zero, new Vector3(100000), graphicsDevice);

            effect = Content.Load<Effect>("Shaders/skysphere_effect");
            effect.Parameters["CubeMap"].SetValue(texture);

           model.SetModelEffect(effect, false);

            this.graphics = graphicsDevice;
        }


        public void Update(GameTime time)
        {
            
        }

        public void Draw(ChaseCamera chaseCamera)
        {
            // Disable the depth buffer
            graphics.DepthStencilState = DepthStencilState.None;

            // Move the model with the sphere
            model.Position = chaseCamera.Position;

            model.Draw(chaseCamera);

            graphics.DepthStencilState = DepthStencilState.Default;
        }

        public void SetClipPlane(Vector4? Plane)
        {
            effect.Parameters["ClipPlaneEnabled"].SetValue(Plane.HasValue);

            if (Plane.HasValue)
                effect.Parameters["ClipPlane"].SetValue(Plane.Value);
        }
    }
}
