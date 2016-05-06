using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JohnStriker.Camera;
using JohnStriker.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JohnStriker.Models
{
    public class CModel
    {
        private readonly bool camera3rdPerson; // is camera in 3rd person mode?
        protected Matrix[] modelTransforms;
        private GraphicsDevice graphicsDevice;
        private BoundingSphere boundingSphere;
        private readonly SpriteBatch spriteBatch;
        protected Matrix transform; // the player transform matrix (position/rotation)
        protected Matrix transformInverse; // inverse of player transform matrix
        private readonly Matrix bobbingInverse = Matrix.Identity; // inverse of bobbing matrix
        private readonly Matrix viewOffset = Matrix.CreateTranslation(GameOptions.CameraViewOffset);
        public Vector3 v3Position;
        public Vector3 v3Rotation;
        // player rotation
      //  public Matrix rotationMatrix;
        private ChaseCamera chaseCamera;
        private Vector2 fontPos;
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public Model Model { get; private set; }              
        public Material Material { get; set; }
        public SpriteFont SpriteFont { get; protected set; }



        public Matrix missileRotation;
        // rotation velocities around each local player axis
        public Vector3 rotationVelocityAxis;

        // rotation forces around each local player axis
        public Vector3 rotationForce;
        public float dampingRotationForce;    // damping rotation force
        public float maxRotationVelocity;   // maximum player rotation velocity
        // maximum rotation force created by input stick
        public float inputRotationForce;   

        public Vector3 Vector3Position()
        {
            return v3Position;
        }

        //===== ROTATION ===================|
        // Returns the entity's rotation.   |
        //==================================|
        public Vector3 Vector3Rotation()
        {
            return v3Rotation;
        }

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
        /// <summary>
        ///     Get the camera view matrix
        /// </summary>
        public Matrix ViewMatrix
        {
            get
            {
                // return player view matrix including bobing and view offset
                return transformInverse * bobbingInverse * viewOffset;
            }
        }
        public BoundingSphere BoundingSphere
        {
            get
            {
                // No need for rotation, as this is a sphere
                Matrix worldTransform = Matrix.CreateScale(Scale)
                    * Matrix.CreateTranslation(Position);

                BoundingSphere transform = boundingSphere;
                transform = transform.Transform(worldTransform);

                return transform;
            }
        }

        public CModel( )
        {
        }

        public CModel(Model m)
        {
            Model = m;
        }

        public CModel(Model model, Vector3 position, Vector3 rotation,
            Vector3 scale, GraphicsDevice graphicsDevice)
        {
            this.Model = model;

            modelTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelTransforms);

            BuildBoundingSphere();
            generateTags();

            this.Position = position;
            this.Rotation = rotation;
            this.Scale = scale;

            this.graphicsDevice = graphicsDevice;

            this.Material = new Material();

           // rotationMatrix = Matrix.Identity;

            rotationVelocityAxis = Vector3.Zero;
            rotationForce = Vector3.Zero;
            dampingRotationForce = GameOptions.MovementRotationForceDamping;
            maxRotationVelocity = GameOptions.MovementRotationVelocity;
            inputRotationForce = GameOptions.MovementRotationForce;
        }

        public CModel(Model model, SpriteFont font, Vector3 vector3position, Vector3 vector3rotation, Vector3 scale,
            GraphicsDevice gd)
        {
            spriteBatch = new SpriteBatch(gd);
            Model = model;
            v3Position = vector3position;
            v3Rotation = vector3rotation;
            SpriteFont = font;
            Scale = scale;
            graphicsDevice = gd;
            modelTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelTransforms);
            BuildBoundingSphere();

            // setup 3rd person camera parameters
            camera3rdPerson = false;
        }

        private void BuildBoundingSphere()
        {
            BoundingSphere sphere = new BoundingSphere(Vector3.Zero, 0);

            // Merge all the model's built in bounding spheres
            foreach (ModelMesh mesh in Model.Meshes)
            {
                BoundingSphere transformed = mesh.BoundingSphere.Transform(
                    modelTransforms[mesh.ParentBone.Index]);

                sphere = BoundingSphere.CreateMerged(sphere, transformed);
            }

            this.boundingSphere = sphere;
        }
        public virtual void Control(string sCommand)
        {
        }
        public virtual void Update(GameTime gameTime, string screen)
        {
        }

        public void Draw(ChaseCamera chaseCamera)
        {
            // Calculate the base transformation by combining
            // translation, rotation, and scaling
            Matrix baseWorld = Matrix.CreateScale(Scale)
                * Matrix.CreateFromYawPitchRoll(
                    Rotation.Y, Rotation.X, Rotation.Z)
                * Matrix.CreateTranslation(Position);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                Matrix localWorld = modelTransforms[mesh.ParentBone.Index]
                    * baseWorld;

                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    Effect effect = meshPart.Effect;

                    if (effect is BasicEffect)
                    {
                        ((BasicEffect)effect).World = localWorld;
                        ((BasicEffect)effect).View = chaseCamera.View;
                        ((BasicEffect)effect).Projection = chaseCamera.Projection;
                        ((BasicEffect)effect).EnableDefaultLighting();
                    }
                    else
                    {
                        setEffectParameter(effect, "World", localWorld);
                        setEffectParameter(effect, "View",chaseCamera.View);
                        setEffectParameter(effect, "Projection",chaseCamera.Projection);
                        setEffectParameter(effect, "CameraPosition",chaseCamera.Position);

                     //   Material.SetEffectParameters(effect);
                    }
                }

                mesh.Draw();
            }
        }

        public virtual void DrawModel(ChaseCamera camera)
        {
            //calculate the base transformation by combining rotation, scale, translation
            Matrix baseworld = Matrix.CreateScale(Scale) *
                               Matrix.CreateFromYawPitchRoll(v3Rotation.Y, v3Rotation.X, v3Rotation.Z) *
                               Matrix.CreateTranslation(v3Position);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                Matrix localWorld = modelTransforms[mesh.ParentBone.Index] * baseworld;

                foreach (ModelMeshPart be in mesh.MeshParts)
                {
                    var effect = (BasicEffect)be.Effect;
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

        // Sets the specified effect parameter to the given effect, if it
        // has that parameter
        void setEffectParameter(Effect effect, string paramName, object val)
        {
            if (effect.Parameters[paramName] == null)
                return;

            if (val is Vector3)
                effect.Parameters[paramName].SetValue((Vector3)val);
            else if (val is bool)
                effect.Parameters[paramName].SetValue((bool)val);
            else if (val is Matrix)
                effect.Parameters[paramName].SetValue((Matrix)val);
            else if (val is Texture2D)
                effect.Parameters[paramName].SetValue((Texture2D)val);
        }

        public void SetModelEffect(Effect effect, bool CopyEffect)
        {
            foreach (ModelMesh mesh in Model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    Effect toSet = effect;

                    // Copy the effect if necessary
                    if (CopyEffect)
                        toSet = effect.Clone();

                    MeshTag tag = ((MeshTag)part.Tag);

                    // If this ModelMeshPart has a texture, set it to the effect
                    if (tag.Texture != null)
                    {
                        setEffectParameter(toSet, "BasicTexture", tag.Texture);
                        setEffectParameter(toSet, "TextureEnabled", true);
                    }
                    else
                        setEffectParameter(toSet, "TextureEnabled", false);

                    // Set our remaining parameters to the effect
                    setEffectParameter(toSet, "DiffuseColor", tag.Color);
                    setEffectParameter(toSet, "SpecularPower", tag.SpecularPower);

                    part.Effect = toSet;
                }
        }

        private void generateTags()
        {
            foreach (ModelMesh mesh in Model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    if (part.Effect is BasicEffect)
                    {
                        BasicEffect effect = (BasicEffect)part.Effect;
                        MeshTag tag = new MeshTag(effect.DiffuseColor,
                            effect.Texture, effect.SpecularPower);
                        part.Tag = tag;
                    }
        }

        // Store references to all of the model's current effects
        public void CacheEffects()
        {
            foreach (ModelMesh mesh in Model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    ((MeshTag)part.Tag).CachedEffect = part.Effect;
        }

        // Restore the effects referenced by the model's cache
        public void RestoreEffects()
        {
            foreach (ModelMesh mesh in Model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    if (((MeshTag)part.Tag).CachedEffect != null)
                        part.Effect = ((MeshTag)part.Tag).CachedEffect;
        }
    }

    public class MeshTag
    {
        public Vector3 Color;
        public Texture2D Texture;
        public float SpecularPower;
        public Effect CachedEffect = null;

        public MeshTag(Vector3 Color, Texture2D Texture, float SpecularPower)
        {
            this.Color = Color;
            this.Texture = Texture;
            this.SpecularPower = SpecularPower;
        }
    }
}
