using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JohnStriker.Camera;
using Microsoft.Xna.Framework;

namespace JohnStriker.Models
{
    public interface IModel
    {
        void Update(GameTime time);
        void Draw(ChaseCamera chaseCamera);
        void SetClipPlane(Vector4? Plane);
    }
}
