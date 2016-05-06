using System;
using JohnStriker.Camera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JohnStriker.Models
{
    public class EnemyShips
    {
        public readonly Vector3 _direction;
  
        public readonly Vector3 _position;

        public Vector3 Position { get; set; }
        private Matrix _rotation = Matrix.Identity;
        public Matrix _world = Matrix.Identity;
        public Model Model { get; protected set; }
        public Vector3 Scale { get; set; }
        private Matrix[] modelTransforms;
        Matrix viewMatrix;

        //control the speed limit
        private float M16DSpeed = 150.0f;
        // Movement thrusters
        internal float fThrusters = 0.0f;
        // Rotation information
        internal float fRotation = 0.0f;
        // Pitch information
        internal float fPitch = 0.0f;

        // Sine wave information (floating behavior)
        private float fFloatStep = 0.0f;
        private float fLastFloat = 0.0f;
        
     
        public Vector3 v3Position;
        public Vector3 v3Rotation;
       

        public ChaseCamera chaseCamera { get; set; }

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

        // This vector controls how much the camera's position is offset from the
        // tank. This value can be changed to move the camera further away from or
        // closer to the tank.
        //readonly Vector3 CameraPositionOffset = new Vector3(0, 40, 150);

        // This value controls the point the camera will aim at. This value is an offset
        // from the tank's position.
        //readonly Vector3 CameraTargetOffset = new Vector3(0, 30, 0);

        public EnemyShips(Model m, Vector3 position,
            Vector3 direction, float yaw, float pitch, float roll,Vector3 scale,GraphicsDevice graphicsDevice)
        {
            //position = position;
            //_world = Matrix.CreateTranslation(position);
            modelTransforms = new Matrix[m.Bones.Count];
            m.CopyAbsoluteBoneTransformsTo(modelTransforms);           
            _direction = direction;
            Model = m;
            //_position = position;
            v3Position = position;
            //v3Rotation = direction;
            Scale = scale;

            chaseCamera = new ChaseCamera(new Vector3(0, 500, 2000), new Vector3(0, 200, 0), new Vector3(0, 0, 0),
         graphicsDevice);
        }

      

        public void Update()
        {
            // Rotate model
            //_rotation *= Matrix.CreateFromYawPitchRoll(_yawAngle,
            //    _pitchAngle, _rollAngle);
            // Move model
            //_world *= Matrix.CreateTranslation(_direction);

            //fThrusters = -10f;
            UpdatePitch();
            UpdateRoll();
            UpdateRotation();
            UpdateFloat();
            UpdateThrusters();
            chaseCamera.Move(v3Position, _direction);
            chaseCamera.Update();        
            //UpdateCamera();
        }

        //===== CONTROL ====================|
        // Manually overrides control of an |
        // entity.  Used mainly for         |
        // controlling the player's avatar. |
        //==================================|
        public  void Control(Vector3 sCommand)
        {
            string sCommands = " ";
            if (sCommand == new Vector3(1, 1, 1))
            {
                sCommands = "thrusters forward";
            }
            else if (sCommand == new Vector3(2, 2, 2))
            {
                sCommands = "rotate left";
            }
            else if (sCommand == new Vector3(3, 3, 3))
            {
                sCommands = "rotate right";
            }
            else if (sCommand == new Vector3(4, 4, 4))
            {
                sCommands = "ascend";
            }
            // Switch statement to evaluate our command
            switch (sCommands)
            {

                case "rotate right":
                    fRotation -= 0.0001f;
                    break;
                case "rotate left":
                    fRotation += 0.0001f;
                    break;
                case "thrusters forward":
                    fThrusters += 0.025f;
                    break;
                case "thrusters default forward":
                    fThrusters = 1f;
                    break;
                case "thrusters backward":
                    fThrusters -= 0.025f;
                    break;
                case "ascend":
                    if (v3Position.Y < 12000.0f)
                    {
                        fPitch += 0.005f;
                    }
                    break;
                case "descend":
                    if (v3Position.Y > 500.0f)
                    {
                        fPitch -= 0.005f;
                    }
                    break;
            
                default:
                    break;
            }

        }

        //===== UPDATEROTATION =============|
        // Uses the thrusters to calculate  |
        // a new rotation for the           |
        // dirigible.                       |
        //==================================|
        private void UpdateRotation()
        {

            // Limit rotation
            if (fRotation > 0.01f)
            {
                fRotation = 0.01f;
            }
            else if (fRotation < -0.01f)
            {
                fRotation = -0.01f;
            }

            // Slow rotation
            if (fRotation > 0.0f)
            {
                fRotation -= 0.00005f;
            }
            else if (fRotation < 0.0f)
            {
                fRotation += 0.00005f;
            }

            //if (SpeedOne == true)
            //{
            //    // Slow rotation
            //    if (fRotation > 0.0f)
            //    {
            //        fRotation -= 0.00005f;
            //    }
            //    else if (fRotation < 0.0f)
            //    {
            //        fRotation += 0.00005f;
            //    }

            //}
            //else if (SpeedTwo == true)
            //{
            //    // Slow rotation
            //    if (fRotation > 0.0f)
            //    {
            //        fRotation -= 0.00005f;
            //    }
            //    else if (fRotation < 0.2f)
            //    {
            //        fRotation += 0.00005f;
            //    }
            //}



            // Apply rotation
            v3Rotation.Y += fRotation;
            //rotationMatrix = rotationMatrix * fRotation;

        }


        //===== UPDATEFLOAT ================|
        // Creates a floating behavior for  |
        // the dirigible based on a sine    |
        // wave.  WARNING: TRIG!            |
        //==================================|
        private void UpdateFloat()
        {

            // Increase the step
            fFloatStep += 0.01f;

            // Store new sine wave value
            float fVariation = 10.0f * (float)Math.Sin(fFloatStep);

            // Alter the dirigible's position
            v3Position.Y -= fLastFloat;
            v3Position.Y += fVariation;

            // Store old sine wave value
            fLastFloat = fVariation;

        }


        //===== UPDATETHRUSTERS ============|
        // Creates a new position for the   |
        // dirigible based upon the         |
        // thrusters and angle of rotation. |
        // WARNING: TRIG!                   |
        //==================================|
        private void UpdateThrusters()
        {

            // Limit thrusters
            if (fThrusters > M16DSpeed)
            {
                //fThrusters = 2.0f;
                fThrusters = M16DSpeed;
            }
            else if (fThrusters < -M16DSpeed)
            {
                //fThrusters = -2.0f;
                fThrusters = -M16DSpeed;
            }

            // Slow thrusters
            if (fThrusters > 0.0f)
            {
                fThrusters -= 0.0025f;
            }
            else if (fThrusters < 0.0f)
            {
                fThrusters += 0.0025f;
            }

            // Stop thrusters
            if (Math.Abs(fThrusters) < 0.0005)
            {
                fThrusters = 0.0f;
            }

            // Apply thrusters
            v3Position.X += -(float)Math.Sin(v3Rotation.Y) * fThrusters;
            v3Position.Z += -(float)Math.Cos(v3Rotation.Y) * fThrusters;

        }

        //===== UPDATEROLL =================|
        // Adjusts the roll of the          |
        // dirigible according to the       |
        // thrusters and rotation.          |
        //==================================|
        private void UpdateRoll()
        {
            //original 
            //v3Rotation.Z = (fRotation * fThrusters) * 15.0f;
            //my changes --- i changed this for the terrain vibration when it turning from high speed
            v3Rotation.Z = (fRotation * (6.0f)) * 15.0f;
        }


        //===== UPDATEPITCH ================|
        // Adjusts the pitch of the         |
        // dirigible.                       |
        //==================================|
        private void UpdatePitch()
        {

            // Slow rate of pitch
            if (fPitch > 0.0f)
            {
                fPitch -= 0.0025f;
            }
            else if (fPitch < 0.0f)
            {
                fPitch += 0.0025f;
            }

            // Limit rate of pitch
            //if (fPitch > 1.0f)
            //my change
            if (fPitch > 10.0f)
            {
                //fPitch = 1.0f;
                //my change
                fPitch = 10.0f;
            }
            //else if (fPitch < -1.0f)
            else if (fPitch < -10.0f)
            {
                //fPitch = -1.0f;
                fPitch = -10.0f;
            }

            // Update position and pitch
            v3Position.Y += fPitch;
            //Original lines
            //v3Rotation.X = (fPitch * fThrusters) / 4.0f;
            //My changes
            v3Rotation.X = (fPitch) / 4.0f;

        }


        private void UpdateCamera()
        {
            // The camera's position depends on the tank's facing direction: when the
            // tank turns, the camera needs to stay behind it. So, we'll calculate a
            // rotation matrix using the tank's facing direction, and use it to
            // transform the two offset values that control the camera.
            //Matrix cameraFacingMatrix = Matrix.CreateRotationY(tank.FacingDirection);
            //Vector3 positionOffset = Vector3.Transform(CameraPositionOffset,
            //    cameraFacingMatrix);
            //Vector3 targetOffset = Vector3.Transform(CameraTargetOffset,
            //    cameraFacingMatrix);

            // once we've transformed the camera's position offset vector, it's easy to
            // figure out where we think the camera should be.
            Vector3 cameraPosition = _position ;

            //// We don't want the camera to go beneath the heightmap, so if the camera is
            //// over the terrain, we'll move it up.
            //if (heightMapInfo.IsOnHeightmap(cameraPosition))
            //{
            //    // we don't want the camera to go beneath the terrain's height +
            //    // a small offset.
            //    float minimumHeight;
            //    Vector3 normal;
            //    heightMapInfo.GetHeightAndNormal
            //        (cameraPosition, out minimumHeight, out normal);

            //    minimumHeight += CameraPositionOffset.Y;

            //    if (cameraPosition.Y < minimumHeight)
            //    {
            //        cameraPosition.Y = minimumHeight;
            //    }
            //}

            // next, we need to calculate the point that the camera is aiming it. That's
            // simple enough - the camera is aiming at the tank, and has to take the 
            // targetOffset into account.
            Vector3 cameraTarget = _position + _direction;


            // with those values, we'll calculate the viewMatrix.
            viewMatrix = Matrix.CreateLookAt(cameraPosition,
                                              cameraTarget,
                                              Vector3.Up);
        }

        public void Draw(ChaseCamera chaseCamera)
        {
            //calculating baseworld by combining rotation , scale and translation
            Matrix baseWorld = Matrix.CreateScale(Scale)*
                               Matrix.CreateFromYawPitchRoll(v3Rotation.Y, v3Rotation.X, v3Rotation.Z) *
                               Matrix.CreateTranslation(v3Position);
            //var transforms = new Matrix[Model.Bones.Count];
            //Model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                Matrix localWorld = modelTransforms[mesh.ParentBone.Index]*baseWorld;
                foreach (BasicEffect be in mesh.Effects)
                {
                    be.EnableDefaultLighting();
                    be.Projection = chaseCamera.Projection;
                    be.View = chaseCamera.View;
                    //be.World = GetWorld()*mesh.ParentBone.Transform;
                    be.World = localWorld;
                }

                mesh.Draw();
            }
        }

    

        public Matrix GetWorld()
        {
            return _rotation*_world;
        }
    }


    public class LeveLInfo
    {
        //spawn variables
        public LeveLInfo(int minSpawntime, int maxSpawnTime, int numberEnemies, int minSpeed, int maxSpeed,
            int missessAllowed)
        {
            MinSpawntime = minSpawntime;
            MaxSpawnTime = maxSpawnTime;
            NumberEnemies = numberEnemies;
            MinSpeed = minSpeed;
            MaxSpeed = maxSpeed;
            MissessAllowed = MissessAllowed;
        }

        public int MinSpawntime { get; set; }

        public int MaxSpawnTime { get; set; }

        public int NumberEnemies { get; set; }

        public int MinSpeed { get; set; }

        public int MaxSpeed { get; set; }

        public int MissessAllowed { get; set; }
    }
}