using JohnStriker.Camera;
using JohnStriker.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace JohnStriker.Models
{
    public abstract class BasicModel
    {
        private readonly Matrix bobbingInverse = Matrix.Identity; // inverse of bobbing matrix
        private readonly bool camera3rdPerson; // is camera in 3rd person mode?
        private readonly Matrix[] modelTransform;
        private readonly SpriteBatch spriteBatch;
        private readonly Matrix viewOffset = Matrix.CreateTranslation(GameOptions.CameraViewOffset);
        public Matrix World = Matrix.Identity;
        private BoundingSphere boundingSphere;
        private ChaseCamera chaseCamera;
        private Vector2 fontPos;

        protected Model model;

        // The entity's position and rotation information

        protected Matrix transform; // the player transform matrix (position/rotation)

        protected Matrix transformInverse; // inverse of player transform matrix
        public Vector3 v3Position;
        public Vector3 v3Rotation;
        public Material Material { get; set; }
        #region Accessors and Mutators

        //===== POSITION ===================|
        // Returns the entity's position.   |
        //==================================|

        public BoundingSphere BoundingSphere
        {
            get
            {
                //no need for rotation as this is a sphere
                Matrix WorldTransform = Matrix.CreateScale(Scale)*Matrix.CreateTranslation(v3Position);
                BoundingSphere transform = boundingSphere;
                transform = transform.Transform(WorldTransform);
                return transform;
            }
        }

        /// <summary>
        ///     True if camera in 3rd person mode
        /// </summary>
        public bool Camera3rdPerson
        {
            get { return camera3rdPerson; }
        }

        /// <summary>
        ///     Get camera up vector
        /// </summary>
        public Vector3 ViewUp
        {
            get
            {
                // if 3rd person mode
                if (camera3rdPerson)
                    // return chase camera up vector
                    return chaseCamera.View.Up;
                return transform.Up;
            }
        }

        public Vector3 Scale { get; set; }

        public Model Model { get; protected set; }

        public SpriteFont SpriteFont { get; protected set; }

        private GraphicsDevice GraphicsDevice { get; set; }

        /// <summary>
        ///     Get current camera positon in world space
        /// </summary>
        public Vector3 CameraPosition
        {
            get
            {
                // return player position
                return v3Position;
            }
        }

        /// <summary>
        ///     Get the camera view matrix
        /// </summary>
        public Matrix ViewMatrix
        {
            get
            {
                // return player view matrix including bobing and view offset
                return transformInverse*bobbingInverse*viewOffset;
            }
        }

        public Vector3 Position()
        {
            return v3Position;
        }

        //===== ROTATION ===================|
        // Returns the entity's rotation.   |
        //==================================|
        public Vector3 Rotation()
        {
            return v3Rotation;
        }

        #endregion

        public BasicModel()
        {
        }

        public BasicModel(string text)
        {
        }

        public BasicModel(Model m)
        {
            Model = m;
        }


        public BasicModel(Model model, Vector3 position, Vector3 direction, float yaw, float pitch, float roll)
        {
        }

        public BasicModel(Model model, SpriteFont font, Vector3 position, Vector3 rotation, Vector3 scale,
            GraphicsDevice graphicsDevice)
        {
            spriteBatch = new SpriteBatch(graphicsDevice);
            Model = model;
            v3Position = position;
            v3Rotation = rotation;
            SpriteFont = font;
            Scale = scale;
            GraphicsDevice = graphicsDevice;
            modelTransform = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelTransform);
            BuildBoundingSphere();

            //setup 3rd person camera parameters
            camera3rdPerson = false;
        }

      


     

        public void BuildBoundingSphere()
        {
            //Collision detection. The two methods that are most commonly used are bounding boxes and bounding spheres. With these methods, 
            //you would basically build a box or sphere around a model that completely covers the model. There will obviously be area that is outside of the model,
            //but still inside of the bounding box or sphere, but an ideal bounding region will limit this as much as possible.
            var boundingSphere = new BoundingSphere(Vector3.Zero, 0);

            //merge all models built in bounding sphere
            foreach (ModelMesh mesh in Model.Meshes)
            {
                BoundingSphere transform =
                    mesh.BoundingSphere.Transform(modelTransform[mesh.ParentBone.Index]);

                boundingSphere = BoundingSphere.CreateMerged(boundingSphere, transform);
            }
            this.boundingSphere = boundingSphere;
        }

        public virtual Matrix GetWorld()
        {
            return World;
        }


        public virtual void Update(GameTime gameTime, string screen)
        {
        }

        public virtual void Control(string sCommand)
        {
        }

        public virtual void Draw(Camera.Camera camera)
        {
            var Transform = new Matrix[Model.Bones.Count];

            Model.CopyAbsoluteBoneTransformsTo(Transform);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect be in mesh.Effects)
                {
                    be.EnableDefaultLighting();
                    be.Projection = camera.Projection;
                    be.View = camera.View;
                    be.World = GetWorld()*mesh.ParentBone.Transform;
                }
                mesh.Draw();
            }
        }

       

        public virtual void DrawModel(ChaseCamera camera)
        {
            //calculate the base transformation by combining rotation, scale, translation
            Matrix baseworld = Matrix.CreateScale(Scale)*
                               Matrix.CreateFromYawPitchRoll(v3Rotation.Y, v3Rotation.X, v3Rotation.Z)*
                               Matrix.CreateTranslation(v3Position);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                Matrix localWorld = modelTransform[mesh.ParentBone.Index]*baseworld;

                foreach (ModelMeshPart be in mesh.MeshParts)
                {
                    var effect = (BasicEffect) be.Effect;
                    effect.EnableDefaultLighting();
                    effect.Projection = camera.Projection;
                    effect.View = camera.View;
                    //effect.World = GetWorld() * mesh.ParentBone.Transform;
                    effect.World = localWorld;
                }
                mesh.Draw();
            }
        }

        public virtual void DrawText(string text)
        {
            // Initial position for text rendering.
            fontPos = new Vector2(1.0f, 1.0f);
            //spriteBatch.Begin(SpriteSortMode.Deferred,BlendState.AlphaBlend);
            //My change -- this is used draw the 2d text on the 3d screen.
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.Default,
                RasterizerState.CullCounterClockwise);
            spriteBatch.DrawString(SpriteFont, text, fontPos, Color.White);
            spriteBatch.End();
        }
    }
}