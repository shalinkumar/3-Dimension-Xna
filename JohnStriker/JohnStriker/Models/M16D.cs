using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JohnStriker.Camera;
using JohnStriker.Graphics;
using JohnStriker.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JohnStriker.Models
{
    public class M16D : CModel
    {
        private ScreenManager screenManager;
        private GameManager gameManager;
        float blaster = 0.0f;     // blaster charge (1.0 when ready to fire)
        float missile = 0.0f;      // missile charge (1.0 when ready to fire)
        int missileCount = 0;      // number of missiles available
        float shield = 1.0f;       // current shield charge (1.0 when ready to use)
        float boost = 1.0f;        // curren boost charge (1.0 when ready to use)

        float damageTime = 0.0f;   // time left showing damage screen 

    

        float bobbingTime;                    // current time for ship bobbing
        //make a quick, short movement up and down.
        Matrix bobbing = Matrix.Identity;     // bobbing matrix
        Matrix bobbingInverse = Matrix.Identity;  // inverse of bobbing matrix


       

        // Rotation information
        private float fRotation = 0.0f;

        Model m16DModel = null;            // player ship model

        // Pitch information
        private float fPitch = 0.0f;

        // Sine wave information (floating behavior)
        private float fFloatStep = 0.0f;
        private float fLastFloat = 0.0f;

        // player Matrix rotation
        public Matrix rotation;

        // Movement thrusters
        private float fThrusters = 0.0f;

        private float M16DSpeed = 150.0f;
        private float M16Dspeed = 0.0f;
        private bool SpeedOne = false;
        private bool SpeedTwo = false;
        private string KeyCommand = string.Empty;
        float deadTime = 0.4f;     // time left before plane respawn after death
        protected Matrix world = Matrix.Identity;
        public Vector3 velocity;       // velocity in local player space

        public float maxVelocity = GameOptions.MovementVelocity;         // maximum player velocity

        
        public bool IsAlive
        {
            get { return (deadTime == 0.0f); }
        }

        public float VelocityFactor
        {
            get { return velocity.Length() / maxVelocity; }
        }

        //===== INIT =======================|
        // Initializes a Game Entity.       |
        //==================================|

        public M16D()
        {
          
        }

        public M16D(Model m)
            : base(m)
        {

        }

        public M16D(Model model, SpriteFont font, Vector3 position, Vector3 rotation, Vector3 scale, GraphicsDevice graphicsDevice, GameManager game)
            : base(model, font, position, rotation, scale, graphicsDevice)
        {
            m16DModel = model;
            gameManager = game;
            //InputManager=new InputManager()
        }

        public void ProcessInput(GameTime gameTime, InputManager input, GameManager game)
        {
            if (input == null)
            {
                throw new ArgumentNullException("inputManager");
            }          

            //int i, j = (int)game.GameMode;
            //for (i = 0; i < j; i++)
            //{
            //    if (input.IsKeyPressed(i, Keys.Escape))
            //    {
            //        //gameManager.GetPlayer(i).Score = -1;
            //        screenManager.SetNextScreen(ScreenType.ScreenEnd);
            //        gameManager.PlaySound("menu_cancel");
            //    }
            //}

         
          
                if (input.IsKeyPressed(0, Keys.Z))
                {
                    EntityList ShipEnities = EntityList.Load("Ship.xml");
                    //fire blaster
                  game.FireProjectile(ProjectileType.Blaster, GameOptions.BlasterVelocity);
                    blaster = 0;
                }
           
            //if missile is ready and input activated
           
            if (input.IsKeyPressed(0, Keys.X))
            {
                game.FireProjectile(ProjectileType.Missile, GameOptions.MissileVelocity);
                AddMissile(-1);

            }
          

            //if boost ready and input activated
            if (boost == 1)
            {
                if (input.IsKeyPressed(0, Keys.C))
                {

                }
            }
            //if shield is ready and input activated
            if (shield == 1)
            {
                if (input.IsKeyPressed(0, Keys.V))
                {

                }
            }
        }


        //add missile to ship positive for adding negative for subtracting
        public void AddMissile(int value)
        {
            missileCount = Math.Max(0, Math.Min(9, missileCount + value));
        }

     

        //===== UPDATE =====================|
        // Updates a Game Entity.           |
        //==================================|
        public override void Update(GameTime gameTime, string screen)
        {
            // Update the M16D
            UpdatePitch();
            UpdateRoll();
            UpdateRotation(gameTime);
            UpdateFloat();
            UpdateThrusters();
            //UpdateAmmo(elapsedTime);
            // Call the parent class' Update function
            base.Update(gameTime, screen);

        }

        //===== CONTROL ====================|
        // Manually overrides control of an |
        // entity.  Used mainly for         |
        // controlling the player's avatar. |
        //==================================|
        public override void Control(string sCommand)
        {

            // Switch statement to evaluate our command
            switch (sCommand)
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
                    if (v3Position.Y > 100.0f)
                    {
                        fPitch -= 0.005f;
                    }
                    break;
                case "Speed control 1":
                    M16Dspeed = 12.0f;
                    break;
                case "Speed control 2":
                    M16Dspeed = 24.0f;
                    break;
                case "Speed control 3":
                    M16Dspeed = 36.0f;
                    break;
                case "Speed control 4":
                    M16Dspeed = 48.0f;
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
        private void UpdateRotation(GameTime gameTime)
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
           // rotationMatrix = rotationMatrix * fRotation;
                        
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
            v3Rotation.Z = (fRotation * fThrusters);
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

        //===== DRAW =======================|
        // Draws a Game Entity.             |
        //==================================|
        public override void DrawModel(ChaseCamera camera)
        {
            base.DrawModel(camera);
        }


        //====DrawText=============================
        //Draw a game entity positions details
        //=========================================
        public override void DrawText(string text)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("Thrust: {0}\n", fThrusters.ToString("#0.00"));
            builder.AppendFormat("Pitch: {0}\n", fPitch.ToString("#0.00"));
            builder.AppendFormat("Position: x:{0} y:{1} z:{2}\n",
                 v3Position.X.ToString("#0.00"),
                 v3Position.Y.ToString("#0.00"),
                 v3Position.Z.ToString("#0.00"));
            builder.AppendFormat("Rotation: x:{0} y:{1} z:{2}\n",
              v3Rotation.X.ToString("#0.00"),
              v3Rotation.Y.ToString("#0.00"),
              v3Rotation.Z.ToString("#0.00"));
            text = builder.ToString();
            base.DrawText(text);
        }


    };
}
