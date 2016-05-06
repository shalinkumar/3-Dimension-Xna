using System;
using System.Collections.Generic;
using System.Text;
using BoxCollider;
using JohnStriker.Camera;
using JohnStriker.Graphics;
using JohnStriker.Models;
using JohnStriker.ParticleExplosion;
using JohnStriker.ParticleSystems;
using JohnStriker.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ParticleSystem = JohnStriker.ParticleExplosion.ParticleSystem;


namespace JohnStriker
{
    // game modes
    public enum ProjectileType
    {
        Blaster = 0,              // blaster projectile
        Missile                   // missile projectile
    }

    public enum RenderTechnique
    {
        PlainMapping = 0,        // plain texture mapping
        NormalMapping,           // normal mapping
        ViewMapping              // view aligned mapping (used for blaster)
    }

    public enum AnimSpriteType
    {
        Blaster = 0,              // blaster hit
        Missile,                  // missile explode
        Ship,                     // ship explode
        Spawn,                    // ship/object spawn
        Shield                    // ship shield
    }

    public enum DrawMode
    {
        Alpha = 0,
        Additive = 1,
        AlphaAndGlow = 2,
        AdditiveAndGlow = 3,
    }

    public enum ParticleSystemType
    {
        ShipExplode = 0, // ship explode
        ShipTrail, // ship trail
        MissileExplode, // missile explode
        MissileTrail, // missile trail
        BlasterExplode // blaster explode
    }

    public class GameManager
    {
        private readonly string[] ammoFiles = { "blaster", "missile" };

        private readonly string[] animSpriteFiles =
        {
            "BlasterGrid_16", "MissileGrid_16", "ShipGrid_32",
            "SpawnGrid_16", "ShieldGrid_32"
        };

        private readonly AnimSpriteManager animatedSprite; // animated sprite manager     
        //private readonly Matrix[] bones = new Matrix[GameOptions.MaxBonesPerMode];
        private readonly M16D[] m16DModel = new M16D[GameOptions.MaxPlayers];
        private Model gameShipModel;
        private readonly ParticleManager particle; // particle manager
        private readonly string[] particleFiles = { "Spark1", "Point1", "Spark2", "Point1", "Point2" };
        private readonly M16D players = new M16D();
        private readonly String[] shipFile = new String[GameOptions.MaxPlayers];
        private readonly SoundBank sound;
        private Texture2D CrossHair;
        private SkySphere _sky;
        private Model[] ammoModels;
        List<Model> listAmmoModels = new List<Model>();
        private Texture2D[] animatedSpriteTexture;
        private CollisionBox box;
        private bool collisionSound; // collision sound ready to play (use to disable 
        private bool currentKeyboardState;
        private HeightMapInfo heightMapInfo;
        private CollisionMesh levelCollision; // level collision model
        private LightList levelLights; // level lights
        private List<LightList> lightList = new List<LightList>();
        private Texture2D[] particleTexture;

        private Matrix projectionFull; // full screen projection matrix
        private Matrix projectionMatrix;
        private Matrix projectionSplit; // split screen projection matrix
        private ScreenGame screenGame;

        private TerrainModel snowTerrain; // Terrain
        List<TerrainModel> listsSnowTerrains = new List<TerrainModel>();
        private SpriteFont spriteFont;
        private Viewport viewportLeft; // left split screen viewport
        private Viewport viewportRight; // right split screen viewport

        /// <summary>
        /// Enemy Ships
        /// </summary>
        private EnemyShips _enemyShips;
        private int _enemiesCount;
        //list of level info objects
        List<LeveLInfo> _leveLInfos = new List<LeveLInfo>();
        private int _currentLevel;
        private float _nextSpawnTime;
        private float _timeSinceLastSpawn;
        Vector3 _maxSpawnLocation = new Vector3(100, 500, -3000);
        private float _maxRollAngle = MathHelper.Pi / 40;
        List<EnemyShips> _listEnemyShipse = new List<EnemyShips>();

        const float M16DCaughtDistance = 2500.0f;   //M16Dcaught distance controls the distance
        const float MissileLockDistance = 8000.0f;   //M16Dcaught distance controls the distance
        private float enemyOrientation;


        const float EnemyTurnSpeed = 0.20f;     //how fast the Enemy Turn Speed ..

        Vector3 previousM16DPosition = new Vector3();
        private bool chaseOrWander = true;
        public Camera.Camera camera { get; protected set; }
        public Random rnd { get; protected set; }

        private Model _enenmyShipsModel;
        public Game1 Game { get; set; }

        public ContentManager Content { get; set; }


        /// <summary>
        /// Explosion
        /// </summary>
        //List<ParticleExplosion> explosions = new List<ParticleExplosion>();
        //ParticleExplosionSettings particleExplosionSettings = new ParticleExplosionSettings();
        //ParticleSettings particleSettings = new ParticleSettings();
        //private Texture2D explosionTexture2D;
        //private Texture2D explosionColorTexture2D;
        //private Effect explosionEffect;

        private ParticleSystem _explosionParticles;
        private ParticleSystem _explosionSmokeParticles;
        private ParticleSystem _projectileTrailParticles;

        // The explosions effect works by firing projectiles up into the
        // air, so we need to keep track of all the active projectiles.
        readonly List<ParticleExplosion.Projectile> projectiles = new List<ParticleExplosion.Projectile>();

        //key frame animation     
        private List<KeyframedObjectAnimations> animation = new List<KeyframedObjectAnimations>();

        private int FrameStringAndEnemyCount = 1;
        private int listEnemyShipseRemoveAt = 0;
        EntityList flightEntityList = new EntityList();

        Matrix bobbing = Matrix.Identity;     // bobbing matrix
        Matrix transform;        // the player transform matrix (position/rotation)
        float bobbingTime;                    // current time for ship bobbing

        private Random random;

        // global bone array used by DrawModel method
        Matrix[] bones = new Matrix[GameOptions.MaxBonesPerModel];
        public ChaseCamera chaseCamera { get; set; }

        private Texture2D[] particleTextures;

        private Model[] projectileModels;

        private readonly String[] projectileFiles = { "blaster", "missile" };

        private readonly ProjectileManager projectile; // projectile manager

        // list of currently playing 3D sounds
        List<Cue> cueSounds = new List<Cue>();
        // 3D sounds finished and ready to delete
        List<Cue> cueSoundsDelete = new List<Cue>();
        private Vector2 fontPos;
        private SpriteBatch spriteBatch;

        Texture2D hudMissile;         // hud missile texture

        public SpriteFont SpriteFont { get; protected set; }
        private Vector3 enemyCollisionYZ { get; set; }

        private GraphicsDevice Graphics { get; set; }

        public GameManager()
        {
        }

        public GameManager(SoundBank sound, GameComponentCollection component, Game1 game1)
        {
            this.sound = sound;


            projectile = new ProjectileManager(this);
            animatedSprite = new AnimSpriteManager();
            particle = new ParticleManager();


            // Construct our particle system components.
            _explosionParticles = new ExplosionParticleSystem(game1, Content);
            _explosionSmokeParticles = new ExplosionSmokeParticleSystem(game1, Content);
            _projectileTrailParticles = new ProjectileTrailParticleSystem(game1, Content);

            // Set the draw order so the explosions and fire
            // will appear over the top of the smoke.          
            _explosionSmokeParticles.DrawOrder = 200;
            _projectileTrailParticles.DrawOrder = 300;
            _explosionParticles.DrawOrder = 400;

            // Register the particle system components.
            component.Add(_explosionParticles);
            component.Add(_explosionSmokeParticles);
            component.Add(_projectileTrailParticles);

            // random bobbing offset
            random = new Random(0);
            bobbingTime = (float)random.NextDouble();
        }



        public void LoadContent(GraphicsDevice gd,
            ContentManager content)
        {
            Graphics = gd;
            spriteBatch = new SpriteBatch(gd);
            // set up projection matrix for full and slpit screen
            float aspect = gd.Viewport.Width / (float)gd.Viewport.Height;
            projectionFull = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(60), aspect, 1.0f, 10000.0f);
            projectionSplit = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(60), aspect * 0.5f, 1.0f, 10000.0f);

            // viewport for split screen
            viewportLeft = gd.Viewport;
            viewportLeft.Width = gd.Viewport.Width / 2 - 1;
            viewportRight = viewportLeft;
            viewportRight.X = gd.Viewport.Width / 2 + 1;


            List<ObjectAnimationFrames> FrameString = new List<ObjectAnimationFrames>();
            for (int i = 0; i < FrameStringAndEnemyCount; i++)
            {
                FrameString.Add(new ObjectAnimationFrames("thrusters forward", "", "", TimeSpan.FromSeconds(0)));
                FrameString.Add(new ObjectAnimationFrames("thrusters forward", "rotate left", "", TimeSpan.FromSeconds(15)));
                FrameString.Add(new ObjectAnimationFrames("", "rotate left", "", TimeSpan.FromSeconds(20)));
                FrameString.Add(new ObjectAnimationFrames("thrusters forward", "", "", TimeSpan.FromSeconds(23)));
                FrameString.Add(new ObjectAnimationFrames("thrusters forward", "rotate right", "", TimeSpan.FromSeconds(25)));
                FrameString.Add(new ObjectAnimationFrames("", "rotate right", "", TimeSpan.FromSeconds(28)));
                FrameString.Add(new ObjectAnimationFrames("thrusters forward", "", "", TimeSpan.FromSeconds(30)));

                animation.Add(new KeyframedObjectAnimations(FrameString, true));

            }
            FrameString.RemoveRange(7, FrameString.Count - 7);

            //animation = new KeyframedObjectAnimations(FrameString, true);


            particle.LoadContent(gd, content);

            // load content for animated sprite manager
            animatedSprite.LoadContent(gd, content);
        }


        public void LoadFiles(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {

            //vector3 x , y, z is  x==left and right depends on + and - , y==top and down ,z==back and front           
            chaseCamera = new ChaseCamera(new Vector3(0, 500, 1000), new Vector3(0, 200, 0), new Vector3(0, 0, 0),
           graphicsDevice);

            spriteFont = contentManager.Load<SpriteFont>(@"Fonts\ArialM");
            //spriteFont.Spacing = -1.0f;
            gameShipModel = contentManager.Load<Model>("Models/skyfighter fbx");
            m16DModel[0] = new M16D(gameShipModel, spriteFont,
                new Vector3(-36000.0f, 100.0f, 19000f), Vector3.Zero, new Vector3(40f), graphicsDevice, this);

            snowTerrain = new TerrainModel(contentManager.Load<Texture2D>("Models/terrain_Weightmap"), 160, 8000,
                //contentManager.Load<Texture2D>("Texture/snow_grass"), 6, new Vector3(1, -1, 0),
                contentManager.Load<Texture2D>("Texture/snow_sand"), 6, new Vector3(0, 0, 2),
                contentManager, graphicsDevice);

            snowTerrain.WeightMap = contentManager.Load<Texture2D>("Texture/weightMap");
            snowTerrain.RTexture = contentManager.Load<Texture2D>("Texture/snow_sand");
            snowTerrain.GTexture = contentManager.Load<Texture2D>("Texture/snow_rock");
            snowTerrain.BTexture = contentManager.Load<Texture2D>("Texture/snow");
            snowTerrain.DetailTexture = contentManager.Load<Texture2D>("Texture/noise_texture");


            //listsSnowTerrains.Add(new TerrainModel(contentManager.Load<Texture2D>("Models/terrain_Weightmap"), 160, 8000,
            //    //contentManager.Load<Texture2D>("Texture/snow_grass"), 6, new Vector3(1, -1, 0),
            //    contentManager.Load<Texture2D>("Texture/snow_sand"), 6, new Vector3(1, -1, 0),
            //    contentManager, graphicsDevice));

            //snowTerrain = contentManager.Load<Model>("Models/terrain_Weightmap");

            // The terrain processor attached a HeightMapInfo to the terrain model's
            // Tag. We'll save that to a member variable now, and use it to
            // calculate the terrain's heights later.
            //heightMapInfo = snowTerrain.Tag as HeightMapInfo;
            //if (heightMapInfo == null)
            //{
            //    string message = "The terrain model did not have a HeightMapInfo " +
            //        "object attached. Are you sure you are using the " +
            //        "TerrainProcessor?";
            //    throw new InvalidOperationException(message);
            //}

            //projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
            //    MathHelper.ToRadians(45.0f), graphicsDevice.Viewport.AspectRatio, 1f, 10000);

            //SkySphere
            _sky = new SkySphere(contentManager, graphicsDevice, contentManager.Load<TextureCube>("DDS/ThickCloudsWater"));
            //CrossHair
            CrossHair = contentManager.Load<Texture2D>("Screens/hud_sp_crosshair");

            // Initialize game levels
            _leveLInfos.Add(new LeveLInfo(5, 10, FrameStringAndEnemyCount, 2, 6, 10));
            //_leveLInfos.Add(new LeveLInfo(900, 2800, FrameStringAndEnemyCount, 2, 6, 9));
            //_leveLInfos.Add(new LeveLInfo(800, 2600, FrameStringAndEnemyCount24, 2, 6, 8));
            //_leveLInfos.Add(new LeveLInfo(700, 2400, FrameStringAndEnemyCount, 3, 7, 7));
            //_leveLInfos.Add(new LeveLInfo(600, 2200, FrameStringAndEnemyCount, 3, 7, 6));
            //_leveLInfos.Add(new LeveLInfo(500, 2000, FrameStringAndEnemyCount, 3, 7, 5));
            //_leveLInfos.Add(new LeveLInfo(400, 1800, FrameStringAndEnemyCount, 4, 7, 4));
            //_leveLInfos.Add(new LeveLInfo(300, 1600, FrameStringAndEnemyCount, 4, 8, 3));
            //_leveLInfos.Add(new LeveLInfo(200, 1400, FrameStringAndEnemyCount, 5, 8, 2));
            //_leveLInfos.Add(new LeveLInfo(100, 1200, FrameStringAndEnemyCount, 5, 9, 1));
            //_leveLInfos.Add(new LeveLInfo(50, 1000, FrameStringAndEnemyCount, 6, 9, 0));
            //_leveLInfos.Add(new LeveLInfo(50, 800, FrameStringAndEnemyCount, 6, 9, 0));
            //_leveLInfos.Add(new LeveLInfo(50, 600, FrameStringAndEnemyCount, 8, 10, 0));
            //_leveLInfos.Add(new LeveLInfo(25, 400, FrameStringAndEnemyCount, 8, 10, 0));
            //_leveLInfos.Add(new LeveLInfo(0, 200, FrameStringAndEnemyCount, 18, 20, 0));

            Game = new Game1();
            camera = new Camera.Camera(Game, new Vector3(0, 0, 100),
             Vector3.Zero, Vector3.Up, "ScreenGame");

            _enenmyShipsModel = contentManager.Load<Model>("Models/EuroFighter");

            // now that the GraphicsDevice has been created, we can create the projection matrix.
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f), graphicsDevice.Viewport.AspectRatio, 1f, 10000);

            rnd = new Random();

            SetNextSpawnTime();

            flightEntityList = EntityList.Load("Ship.xml");

            float radius = GameOptions.CollisionBoxRadius;
            box = new CollisionBox(-radius, radius);

            //Ammo
            if (animatedSpriteTexture == null)
            {
                int i, j = animSpriteFiles.GetLength(0);
                animatedSpriteTexture = new Texture2D[j];
                for (i = 0; i < j; i++)
                {
                    animatedSpriteTexture[i] = contentManager.Load<Texture2D>("Explosion/" + animSpriteFiles[i]);
                }
            }

            //particle
            if (particleTexture == null)
            {
                int i, j = particleFiles.GetLength(0);
                particleTexture = new Texture2D[j];
                for (i = 0; i < j; i++)
                {
                    particleTexture[i] = contentManager.Load<Texture2D>("Particle/" + particleFiles[i]);
                }
            }

            // cerate players
            for (int i = 0; i < 1; i++)
            {
                shipFile[0] = "Ship";
                if (shipFile[0] != null)
                {
                    //EntityList ShipEnities = EntityList.Load("Ship.xml");

                    //screenGame = new ScreenGame(ShipEnities);
                }
            }



            // load projectile models

            if (projectileModels == null)
            {
                int i, j = projectileFiles.GetLength(0);
                projectileModels = new Model[j];
                for (i = 0; i < j; i++)
                    projectileModels[i] = contentManager.Load<Model>(@"Ammo/" + projectileFiles[i]);
            }

            // load particle textures
            if (particleTextures == null)
            {
                int i, j = particleFiles.GetLength(0);
                particleTextures = new Texture2D[j];
                for (i = 0; i < j; i++)
                    particleTextures[i] = contentManager.Load<Texture2D>(
                        "particle/" + particleFiles[i]);
            }

            hudMissile = contentManager.Load<Texture2D>(
                                 "screens/hud_sp_missile");

            SpriteFont = spriteFont;
        }

        public void UnloadFiles()
        {
        }

        public void PlaySound(string soundName)
        {
            sound.PlayCue(soundName);
        }

        public void ProcessInput(GameTime gameTime, InputManager input)
        {
            UserControls();

            players.ProcessInput(gameTime, input, this);
        }

        public int GetPlayerPosition(Vector3 position)
        {
            for (int i = 0; i < GameOptions.MaxPlayers; i++)
            {
                if (players != null && players.IsAlive)
                {
                    //need to work here
                }
            }
            return -1;
        }






        public AnimSprite AddAnimSprite(AnimSpriteType type, Vector3 position, float radius, float viewOffSet,
            float frameRate, DrawMode mode, int player)
        {
            var a = new AnimSprite(type, position, radius, viewOffSet, animatedSpriteTexture[(int)type], 256, 256,
                frameRate, mode, player);

            animatedSprite.Add(a);

            return a;
        }

        /// <summary>
        ///     Add damage to all players inside a splash sphere with distance attenuation
        /// </summary>
        public void AddDamageSplash(int attacker,
            float damage, Vector3 position, float radius)
        {
            // check all players
            for (int i = 0; i < GameOptions.MaxPlayers; i++)
                // if player is alive
                if (players != null && players.IsAlive)
                {
                    // get squared distance from player to splash center
                    Vector3 vec = players.v3Position - position;
                    float len = vec.LengthSquared();
                    // if player inside sphere
                    if (len < radius * radius)
                    {
                        // get actual length
                        len = (float)Math.Sqrt(len);

                        // compute damage intensity (squared not linear inside sphere)
                        float intensity = len / radius;
                        intensity = 1.0f - intensity * intensity;

                        // normalize vector used for pushing direction
                        vec *= 1.0f / len;

                        // apply damage and push player
                        AddDamage(attacker, i, intensity * damage, vec);
                    }
                }
        }

        public void AddDamage(int attacker, int defender, float damage, Vector3 pushDirection)
        {
        }

        //used to push a player explosion and player to player collision
        public void AddImpulse(Vector3 force)
        {
        }

        public void Update(GameTime gameTime)
        {
            // Get the keyboard state
            KeyboardState keys = Keyboard.GetState();
            chaseOrWander = true;
            // Update the camera
            UpdateCameras();
            //// Update the flight
            m16DModel[0].Update(gameTime, "");

            //check to see if its time to spawn enemy
            Chectospawnenemy(gameTime);

            if (IsEven(gameTime.TotalGameTime.Seconds))
            {
                int evenTime = gameTime.TotalGameTime.Seconds;

                previousM16DPosition = m16DModel[0].v3Position;
            }

            //for (int i = 0; i < _listEnemyShipse.Count -listEnemyShipseRemoveAt; i++)
            for (int i = 0; i < _listEnemyShipse.Count; i++)
            {
                enemyCollisionYZ = _listEnemyShipse[i].v3Position;
                float distanceFromEnemy = Vector3.Distance(_listEnemyShipse[i].v3Position, m16DModel[0].v3Position);

                //1478.09875 2009.12549
                //if (distanceFromEnemy < M16DCaughtDistance)
                //{
                //    //animation.RemoveAt(i);
                //    chaseOrWander = false;
                //    listEnemyShipseRemoveAt++;
                //    animation.RemoveAt(i);

                //    enemyOrientation = TurnToFace(_listEnemyShipse[i].v3Position, m16DModel[0].v3Position,
                //    enemyOrientation, EnemyTurnSpeed);

                //    var heading = new Vector3((float)Math.Cos(enemyOrientation), (float)Math.Sin(enemyOrientation),
                //   (float)Math.Cos(enemyOrientation));
                //    //_listEnemyShipse[i].v3Position.Z = m16DModel[0].v3Position.Z;


                //    // if (previousM16DPosition.X > m16DModel[0].v3Position.X)
                //    //{
                //    //    _listEnemyShipse[i].Update();
                //    //    _listEnemyShipse[i].Control(new Vector3(2, 2, 2));    
                //    //}
                //    // if (previousM16DPosition.X < m16DModel[0].v3Position.X)
                //    //{
                //    //    _listEnemyShipse[i].Update();
                //    //    _listEnemyShipse[i].Control(new Vector3(3, 3, 3));    
                //    //}
                //    //else if (previousM16DPosition.Z <= m16DModel[0].v3Position.Z)
                //    //{
                //    //    _listEnemyShipse[i].Update();
                //    //    _listEnemyShipse[i].Control(new Vector3(1, 1, 1));        
                //    //}
                //}
                //else
                //{
                //animation[i].Update(gameTime.ElapsedGameTime);
                //_listEnemyShipse[i].Update();
                //_listEnemyShipse[i].Control(animation[i].Position);
                //_listEnemyShipse[i].Control(animation[i].Rotation);
                //}

                MissileLock(_listEnemyShipse[i].v3Position);

                UpdateProjectiles(gameTime, _listEnemyShipse[i].Model, _listEnemyShipse[i].v3Position, gameShipModel, m16DModel[0].v3Position);

            }
            if (chaseOrWander == true)
            {
                if (previousM16DPosition.Z >= m16DModel[0].v3Position.Z)
                {
                    for (int i = 0; i < listEnemyShipseRemoveAt; i++)
                    {
                        _listEnemyShipse[i].Update();
                        _listEnemyShipse[i].Control(new Vector3(1, 1, 1));


                        //// Check to see if the LEFT key is down
                        //if (keys.IsKeyDown(Keys.Left))
                        //{
                        //    _listEnemyShipse[i].Control(new Vector3(2, 2, 2));
                        //}

                        //// Check to see if the RIGHT key is down
                        //if (keys.IsKeyDown(Keys.Right))
                        //{
                        //    _listEnemyShipse[i].Control(new Vector3(3, 3, 3));
                        //}

                        //// Check to see if the q key is down
                        //if (keys.IsKeyDown(Keys.Q))
                        //{
                        //    _listEnemyShipse[i].Control(new Vector3(4, 4, 4));

                        //}
                    }

                }
            }

            // update animated projectiles
            projectile.Update(gameTime);

            // update particle systems
            particle.Update(gameTime);

            animatedSprite.Update(gameTime);

            cueSoundsDelete.Clear();
            foreach (Cue cue in cueSounds)
            {
                if (cue.IsStopped)
                {
                    cueSoundsDelete.Add(cue);
                }

            }

            foreach (Cue cue in cueSoundsDelete)
            {
                cueSounds.Remove(cue);
                cue.Dispose();
            }
        }

        private float positionPer1;
        //private float positionPer2 = 0.0f;
        //private float positionPer3 = 0.0f;


        public void MissileLock(Vector3 enemyPosition)
        {
            bool Locked = false;

            float distanceBetween = Vector3.Distance(m16DModel[0].v3Position, enemyPosition);
            positionPer1 = 0.0f;

            if (Vector3.Dot(enemyPosition, m16DModel[0].v3Position) > 0)
            {
                
            }

            if (Vector3.Dot(enemyPosition, m16DModel[0].v3Position) < 0)
            {

            }

            if (Vector3.Dot(enemyPosition, m16DModel[0].v3Position) == 0)
            {

            }

            if (Vector3.Dot(enemyPosition, m16DModel[0].v3Position) == 1)
            {

            }

            if (Vector3.Dot(enemyPosition, m16DModel[0].v3Position) == -1)
            {

            }

            float theta = Vector3.Dot(enemyPosition, m16DModel[0].v3Position);

            if (theta < 0)
            {
              
            }
            if (theta > 0)
            {
               
            }
            if (theta == 0)
            {
              
            }


            if(Math.Acos(Vector3.Dot(Vector3.Normalize(enemyPosition), Vector3.Normalize(m16DModel[0].v3Position))) >0)
            {
                
            }// the angle between the two vectors is less than 90 degrees.



            if (Math.Acos(Vector3.Dot(Vector3.Normalize(enemyPosition), Vector3.Normalize(m16DModel[0].v3Position))) < 0) //the angle between the two vectors is more than 90 degrees.
            {

            }
            if (Math.Acos(Vector3.Dot(Vector3.Normalize(enemyPosition), Vector3.Normalize(m16DModel[0].v3Position))) == 0) //the angle between the two vectors is 90 degrees; that is, the vectors are orthogonal.
            {

            }
            if (Math.Acos(Vector3.Dot(Vector3.Normalize(enemyPosition), Vector3.Normalize(m16DModel[0].v3Position))) == 1) //the angle between the two vectors is 0 degrees; that is, the vectors point in the same direction and are parallel.
            {

            }
            if (Math.Acos(Vector3.Dot(Vector3.Normalize(enemyPosition), Vector3.Normalize(m16DModel[0].v3Position))) == -1)//the angle between the two vectors is 180 degrees; that is, the vectors point in opposite directions and are parallel.}
            {

            }
            

            if (m16DModel[0].v3Position.Y > enemyPosition.Y && distanceBetween <= MissileLockDistance && chaseCamera.FollowTargetPosition.Y > enemyPosition.Y)
            //if (m16DModel[0].v3Position.Y > enemyPosition.Y && distanceBetween <= MissileLockDistance)
            {
                positionPer1 = Vector3.Distance(new Vector3(0, m16DModel[0].v3Position.Y, 0), new Vector3(0, enemyPosition.Y, 0));
                positionPer1 = (100 - positionPer1);
                //float positionPercentage = enemyPosition.Y / m16DModel[0].v3Position.Y;
                //float percentage = positionPercentage * 100;

            }

            //else if (m16DModel[0].v3Position.Y < enemyPosition.Y && distanceBetween >= M16DCaughtDistance ||
            //             distanceBetween <= M16DCaughtDistance)
            if (m16DModel[0].v3Position.Y < enemyPosition.Y && distanceBetween <= MissileLockDistance && chaseCamera.FollowTargetPosition.Y < enemyPosition.Y)
            //if (m16DModel[0].v3Position.Y < enemyPosition.Y && distanceBetween <= MissileLockDistance)
            {
                positionPer1 = Vector3.Distance(new Vector3(0, enemyPosition.Y, 0), new Vector3(0, m16DModel[0].v3Position.Y, 0));
                positionPer1 = (100 - positionPer1);
                //float positionPercentage = m16DModel[0].v3Position.Y / enemyPosition.Y;
                //float percentage = positionPercentage * 100;
            }

            //if (m16DModel[0].v3Position.Y == enemyPosition.Y && distanceBetween >= M16DCaughtDistance ||
            //     distanceBetween <= M16DCaughtDistance)
            //{
            //    positionPer3 = Vector3.Distance(new Vector3(0, m16DModel[0].v3Position.Y, 0), new Vector3(0, enemyPosition.Y, 0));
            //    float positionPercentage = enemyPosition.Y / m16DModel[0].v3Position.Y;
            //    float percentage = positionPercentage * 100;

            //}

            if (positionPer1.ToString("#0") == "100")
            {
                // fire left missile                    
                Matrix m = new Matrix();
                m = Matrix.CreateFromYawPitchRoll(m16DModel[0].v3Rotation.Y, m16DModel[0].v3Rotation.X, m16DModel[0].v3Rotation.Z);
                m.Translation = m16DModel[0].v3Position + Vector3.Transform(new Vector3(-240, 0, 0), Matrix.CreateFromYawPitchRoll(m16DModel[0].v3Rotation.Y, m16DModel[0].v3Rotation.X, m16DModel[0].v3Rotation.Z));

                Graphics.Projectile p = AddProjectile(ProjectileType.Missile,
                      1, m, GameOptions.MissileVelocity, 0.1f,
                      RenderTechnique.ViewMapping);

                p.SetExplosion(AnimSpriteType.Missile,
                    90, 30, DrawMode.AdditiveAndGlow,
                    0.5f, 500, "missile_explode");

                // set missile trail
                Graphics.ParticleSystem Trail = AddParticleSystem(
                    ParticleSystemType.MissileTrail, Matrix.Identity);
                p.SetTrail(Trail,
                    Matrix.CreateTranslation(GameOptions.MissileTrailOffset));
            }
                
        }

        

        public static bool IsEven(int value)
        {
            return value % 2 == 0;
        }

        /// <summary>
        /// Helper for updating the list of active projectiles.
        /// </summary>
        private void UpdateProjectiles(GameTime gameTime, Model enemyModel, Vector3 enemyWorld, Model gameShipModelsModel, Vector3 gameShipWorldTwo)
        {
            int i = 0;

            if (CollidesWith(enemyModel, enemyWorld, gameShipModelsModel, gameShipWorldTwo))
            {
                //collision add an explosion             
                var distVector33 = Vector3.Distance(enemyWorld, gameShipWorldTwo);
                //53.42622
                //60.5967636
                projectiles.Clear();
                projectiles.Add(new ParticleExplosion.Projectile(_explosionParticles,
                                         _explosionSmokeParticles,
                                         _projectileTrailParticles));

            }

            for (int j = 0; j < projectiles.Count; j++)
            {
                if (!projectiles[j].Update(gameTime))
                {
                    _listEnemyShipse.RemoveAt(j);
                    animation.RemoveAt(j);
                    // Remove projectiles at the end of their life.
                    projectiles.RemoveAt(j);
                    // play collision sound
                    PlaySound("ship_explode");
                    projectiles.Clear();
                }
            }
        }

        public bool CollidesWith(Model enemyModel, Vector3 enemyWorld, Model gameShipModelsModel, Vector3 gameShipWorldTwo)
        {
            // Loop through each ModelMesh in both objects and compare
            // all bounding spheres for collisions

            //foreach (ModelMesh hisModelMeshes in enemyModel.Meshes)
            //{
            //    foreach (ModelMesh myModelMeshes in gameShipModelsModel.Meshes)
            //    {
            //        if (myModelMeshes.BoundingSphere.Transform(
            //            gameShipWorldTwo).Intersects(
            //                hisModelMeshes.BoundingSphere.Transform(enemyWorld)))
            //        {
            //            return true;
            //        }

            //    }
            //}

            for (int i = 0; i < gameShipModelsModel.Meshes.Count; i++)
            {
                // Check whether the bounding boxes of the two cubes intersect.
                BoundingSphere c1BoundingSphere = gameShipModelsModel.Meshes[i].BoundingSphere;
                c1BoundingSphere.Center += m16DModel[0].v3Position;

                for (int j = 0; j < enemyModel.Meshes.Count; j++)
                {
                    BoundingSphere c2BoundingSphere = enemyModel.Meshes[j].BoundingSphere;
                    c2BoundingSphere.Center += new Vector3(enemyWorld.X, enemyWorld.Y, enemyWorld.Z);

                    if (c1BoundingSphere.Intersects(c2BoundingSphere))
                    {

                        return false;
                    }
                }
            }

            return false;
        }

        //Collision for world
        public void UpdateCollision(float elapsedTime, CollisionMesh collision)
        {
            if (collision == null)
            {
                throw new ArgumentNullException("collision");
            }

            // hold position before movement
            Vector3 lastPostion = m16DModel[0].Position;
            // test for collision with level
            Vector3 collisionPosition;
            if (collision.BoxMove(box, lastPostion, m16DModel[0].Position,
                1.0f, 0.0f, 3, out collisionPosition))
            {
                // update to valid position after collision
                m16DModel[0].Position = collisionPosition;

                // compute new velocity after collision
                Vector3 newVelocity =
                    (collisionPosition - lastPostion) * (1.0f / elapsedTime);

                // if collision sound enabled
                if (collisionSound)
                {
                    // test collision angle to play collision sound 
                    //Vector3 WorldVel = m16DModel[0].WorldVelocity;
                    //float dot = Vector3.Dot(
                    //    Vector3.Normalize(WorldVel), Vector3.Normalize(newVelocity));
                    //if (dot < 0.7071f)
                    //{
                    //    // play collision sound
                    //    PlaySound("ship_collide");

                    //    // set rumble intensity
                    //    dot = 1 - 0.5f * (dot + 1);
                    //    //gameManager.SetVibration(playerIndex, dot * 0.5f);

                    //    // disable collision sounds until ship stops colliding
                    //    collisionSound = false;
                    //}
                }

                // set new velocity after collision
                //m16DModel[0].WorldVelocity = newVelocity;
            }
        }

        private void UpdateCameras()
        {
            //move the camera to the new models position and rotation.
            chaseCamera.Move(m16DModel[0].Vector3Position(), m16DModel[0].Vector3Rotation());
            chaseCamera.Update();
        }


        private void UserControls()
        {
            // Get the keyboard state
            KeyboardState keys = Keyboard.GetState();


            // Check to see if the LEFT key is down
            if (keys.IsKeyDown(Keys.Left))
            {
                m16DModel[0].Control("rotate left");
            }

            // Check to see if the RIGHT key is down
            if (keys.IsKeyDown(Keys.Right))
            {
                m16DModel[0].Control("rotate right");
            }

            // Check to see if the UP key is down
            if (keys.IsKeyDown(Keys.Up))
            {
                m16DModel[0].Control("thrusters forward");
                currentKeyboardState = true;
            }
            if (currentKeyboardState == false)
            {
                m16DModel[0].Control("thrusters default forward");
            }
            // Check to see if the DOWN key is down
            if (keys.IsKeyDown(Keys.Down))
            {
                m16DModel[0].Control("thrusters backward");
            }

            // Check to see if the Q key is down
            if (keys.IsKeyDown(Keys.Q))
            {
                m16DModel[0].Control("ascend");
            }

            // Check to see if the E key is down
            if (keys.IsKeyDown(Keys.E))
            {
                m16DModel[0].Control("descend");
            }

            //Flight speed
            //Speed control 1
            if (keys.IsKeyDown(Keys.D1))
            {
                m16DModel[0].Control("Speed control 1");
            }
            //Speed control 2
            if (keys.IsKeyDown(Keys.D2))
            {
                m16DModel[0].Control("Speed control 2");
            }
            //Speed control 3
            if (keys.IsKeyDown(Keys.D3))
            {
                m16DModel[0].Control("Speed control 3");
            }
            //Speed control 4
            if (keys.IsKeyDown(Keys.D4))
            {
                m16DModel[0].Control("Speed control 4");
            }
        }

        private void SetNextSpawnTime()
        {
            // Reset the variables to indicate the next enemy spawn time
            _nextSpawnTime = rnd.Next(_leveLInfos[_currentLevel].MinSpawntime, _leveLInfos[_currentLevel].MaxSpawnTime);
            _timeSinceLastSpawn = 0.0f;
        }

        private void Chectospawnenemy(GameTime gameTime)
        {
            if (_enemiesCount < _leveLInfos[_currentLevel].NumberEnemies)
            {
                _timeSinceLastSpawn += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_timeSinceLastSpawn > _nextSpawnTime)
                {
                    SpawnEnemy();
                }
            }
        }

        private void SpawnEnemy()
        {
            //Vector3 _position = new Vector3(rnd.Next(-(int)_maxSpawnLocation.X, -(int)_maxSpawnLocation.X),
            //    rnd.Next(-(int)_maxSpawnLocation.Y, (int)_maxSpawnLocation.Y),
            //    _maxSpawnLocation.Z);

            Vector3 _position = new Vector3(-36000.0f, 120.0f, 20000f);

            //direction will be always (0,0,z)
            //where z is random values between minspeed and maxspeed

            Vector3 _direction = new Vector3(0, 0, rnd.Next(_leveLInfos[_currentLevel].MinSpeed, _leveLInfos[_currentLevel].MaxSpeed));

            //get a random roll rotation between -maxRollAngle and maxRollAngle
            //float _rollRotation = (float)rnd.NextDouble() * _maxRollAngle - (_maxRollAngle / 2);
            float _rollRotation = (float)0;

            //add model to this list
            _listEnemyShipse.Add(new EnemyShips(_enenmyShipsModel, _position, _direction, 0, 0, _rollRotation, new Vector3(10f), Graphics));

            //increase of enemies this level and set next spawn time
            ++_enemiesCount;
            SetNextSpawnTime();
        }

        public void Draw3D(GraphicsDevice graphicsDevice)
        {
            _sky.Draw(chaseCamera);

            snowTerrain.Draw(chaseCamera);
            //DrawModel(snowTerrain);
            m16DModel[0].DrawModel(chaseCamera);
            foreach (EnemyShips enemyShipse in _listEnemyShipse)
            {

                enemyShipse.Draw(chaseCamera);
            }

            //foreach (TerrainModel terrainModel in listsSnowTerrains)
            //{
            //    terrainModel.Draw(chaseCamera);
            //}


            GraphicsDevice device = graphicsDevice;
            // Compute camera matrices.
            float aspectRatio = (float)device.Viewport.Width /
                                (float)device.Viewport.Height;



            // Pass camera matrices through to the particle system components.
            _explosionParticles.SetCamera(camera.View, camera.Projection);
            _explosionSmokeParticles.SetCamera(camera.View, camera.Projection);
            _projectileTrailParticles.SetCamera(camera.View, camera.Projection);

            DrawScene(graphicsDevice, RenderTechnique.NormalMapping);
        }

        public void Draw2D(FontManager font, GraphicsDevice graphicsDevice)
        {
            Rectangle rect = font.ScreenRectangle;
            DrawHud(font, rect, Vector3.Zero, 70, 120, false);
            if (positionPer1.ToString("#0") == "100")
            {
                font.BeginText();
                font.DrawText(FontType.ArialMedium, "LOCKED", new Vector2(rect.Right - 154, rect.Bottom - 160), Color.LightCyan);
                font.EndText();
            }
            else if (Convert.ToInt32(positionPer1.ToString("#0")) < Convert.ToInt32(0))
            {
                font.BeginText();
                font.DrawText(FontType.ArialMedium, "TOO FAR", new Vector2(rect.Right - 154, rect.Bottom - 160), Color.LightCyan);
                font.EndText();
            }
            else if (Convert.ToInt32(positionPer1.ToString("#0")) <= Convert.ToInt32(100) && Convert.ToInt32(positionPer1.ToString("#0")) >= Convert.ToInt32(0))
            {
                font.BeginText();
                font.DrawText(FontType.ArialMedium, "LOCKING..", new Vector2(rect.Right - 157, rect.Bottom - 160), Color.LightCyan);
                font.EndText();
            }                               
            font.BeginText();
            font.DrawText(FontType.ArialMedium, positionPer1.ToString("#0"), new Vector2(rect.Right - 138, rect.Bottom - 120), Color.LightCyan);
            font.EndText();
            //font.BeginText();
            //font.DrawText(FontType.ArialMedium, positionPer2.ToString("#0"), new Vector2(rect.Right - 138, rect.Bottom - 100), Color.LightCyan);
            //font.EndText();
            //font.BeginText();
            //font.DrawText(FontType.ArialMedium, positionPer3.ToString("#0.00"), new Vector2(rect.Right - 138, rect.Bottom - 80), Color.LightCyan);
            //font.EndText();

        }

        private void DrawHud(FontManager font, Rectangle rectangle, Vector3 bars, int barsLeft, int barsRight,
            bool crossHair)
        {
            var r = new Rectangle(0, 0, 0, 0);

            if (crossHair)
            {
                r.X = rectangle.X + (rectangle.Width - CrossHair.Width) / 2;
                r.Y = rectangle.Y + (rectangle.Height - CrossHair.Height) / 2;
                r.Width = CrossHair.Width;
                r.Height = CrossHair.Height;
                font.DrawTexture(CrossHair, r, Color.White, BlendState.AlphaBlend);
            }


            r.X = rectangle.X + rectangle.Width - hudMissile.Width;
            r.Y = rectangle.Y + rectangle.Height - hudMissile.Height;
            r.Width = hudMissile.Width;
            r.Height = hudMissile.Height;
            font.DrawTexture(hudMissile, r, Color.Black, BlendState.AlphaBlend);



        }

        public void DrawText(string text)
        {
            m16DModel[0].DrawText(text);
        }




        public int GetPlayerAtPosition(Vector3 position)
        {
            //if (enemyCollisionYZ != new Vector3(0, 0, 0))
            //{

            //    float distanceBetween = Vector3.Distance(m16DModel[0].v3Position, enemyCollisionYZ);

            //    if (m16DModel[0].v3Position.Y > enemyCollisionYZ.Y && distanceBetween >= M16DCaughtDistance || distanceBetween <= M16DCaughtDistance)
            //    {

            //    }
            //    else if (m16DModel[0].v3Position.Y < enemyCollisionYZ.Y && distanceBetween >= M16DCaughtDistance || distanceBetween <= M16DCaughtDistance)
            //    {

            //    }
            //}

            //you need to lock the enemy flight here..lock means two flight position Y should be same and position z should be nearest distance.

            //Matrix m = new Matrix();
            //m = Matrix.CreateFromYawPitchRoll(m16DModel[0].v3Rotation.Y, m16DModel[0].v3Rotation.X, m16DModel[0].v3Rotation.Z);
            //m.Translation = position + Vector3.Transform(new Vector3(0, 0, 0), Matrix.CreateFromYawPitchRoll(m16DModel[0].v3Rotation.Y, m16DModel[0].v3Rotation.X, m16DModel[0].v3Rotation.Z));

            //if (CollidesWith(projectileModels[1], Matrix.CreateTranslation(position),
            //   _enenmyShipsModel, Matrix.CreateTranslation(_listEnemyShipse[0].v3Position)))
            //{

            //}

            return -1;
        }

        public void PlaySound3D(String soundName, Vector3 position)
        {
            // get distance from sound to closest player
            float minimumDistance = 1e10f;
            for (int i = 0; i < GameOptions.MaxPlayers; i++)
            {
                float dist = (position - m16DModel[0].v3Position).LengthSquared();
                if (dist < minimumDistance)
                {
                    minimumDistance = dist;
                }

            }

            // create a new sound instance
            Cue cue = sound.GetCue(soundName);
            cueSounds.Add(cue);

            // set volume based on distance from closest player
            //cue.SetVariable("Distance", (float)Math.Sqrt(minimumDistance));

            // play sound 
            cue.Play();
        }

        /// <summary>
        ///     Create a new particle system and add it to the particle system manager
        /// </summary>
        public Graphics.ParticleSystem AddParticleSystem(
            ParticleSystemType type,
            Matrix transform)
        {
            Graphics.ParticleSystem ps = null;

            switch (type)
            {
                case ParticleSystemType.ShipExplode:
                    ps = new Graphics.ParticleSystem(
                        ParticleSystemType.ShipExplode,
                        200, // num particles
                        0.0f, // emission angle (0 for omni)
                        0.8f, 0.8f, // particle and total time
                        20.0f, 50.0f, // min and max size
                        600.0f, 1000.0f, // min and max vel
                        new Vector4(1.0f, 1.0f, 1.0f, 1.6f), // start color
                        new Vector4(1.0f, 1.0f, 1.0f, 0.0f), // end color
                        particleTextures[(int)type], // texture
                        DrawMode.Additive, // draw mode
                        transform); // transform
                    break;
                case ParticleSystemType.ShipTrail:
                    ps = new Graphics.ParticleSystem(
                        ParticleSystemType.ShipTrail,
                        100, // num particles
                        5.0f, // emission angle (0 for omni)
                        0.5f, 2.0f, // particle time and total time
                        50.0f, 100.0f, // min and max size
                        1000.0f, 1500.0f, // min and max vel
                        new Vector4(0.5f, 0.2f, 0.0f, 1.0f), // start color
                        new Vector4(1.0f, 0.0f, 0.0f, 0.0f), // end color
                        particleTextures[(int)type], // texture
                        DrawMode.AdditiveAndGlow, // draw mode
                        transform); // transform
                    break;
                case ParticleSystemType.MissileExplode:
                    ps = new Graphics.ParticleSystem(
                        ParticleSystemType.MissileExplode,
                        200, // num particles
                        0.0f, // emission angle (0 for omni)
                        0.5f, 0.5f, // particle and total time
                        20.0f, 60.0f, // min and max size
                        800.0f, 1200.0f, // min and max vel
                        new Vector4(1.0f, 1.0f, 1.0f, 1.5f), // start color
                        new Vector4(1.0f, 1.0f, 1.0f, -0.5f), // end color
                        particleTextures[(int)type], // texture
                        DrawMode.AdditiveAndGlow, // draw mode
                        transform); // transform
                    break;
                case ParticleSystemType.MissileTrail:
                    ps = new Graphics.ParticleSystem(
                        ParticleSystemType.MissileTrail,
                        100, // num particles
                        10.0f, // emission angle (0 for omni)
                        0.5f, 1.0f, // particle time and total time
                        15.0f, 30.0f, // min and max size
                        1000.0f, 1500.0f, // min and max vel
                        new Vector4(0.5f, 0.2f, 0.0f, 1.0f), // start color
                        new Vector4(1.0f, 0.0f, 0.0f, 0.0f), // end color
                        particleTextures[(int)type], // texture
                        DrawMode.AdditiveAndGlow, // draw mode
                        transform); // transform
                    break;
                case ParticleSystemType.BlasterExplode:
                    ps = new Graphics.ParticleSystem(
                        ParticleSystemType.BlasterExplode,
                        40, // num particles
                        2, // emission angle (0 for omni)
                        0.25f, 0.25f, // particle time and total time
                        30.0f, 40.0f, // min and max size
                        200.0f, 800.0f, // min and max vel
                        new Vector4(1.0f, 1.0f, 1.0f, 1.5f), // start color
                        new Vector4(1.0f, 1.0f, 1.0f, -0.2f), // end color
                        particleTextures[(int)type], // texture
                        DrawMode.AdditiveAndGlow, // draw mode
                        transform); // transform
                    break;
            }

            if (ps != null)
                particle.Add(ps);

            return ps;
        }

        public void FireProjectile(ProjectileType projectile, float velocity)
        {
            switch (projectile)
            {
                case ProjectileType.Blaster:
                    {
                        //// fire left blaster          
                        Matrix m = new Matrix();
                        m = Matrix.CreateFromYawPitchRoll(m16DModel[0].v3Rotation.Y, m16DModel[0].v3Rotation.X, m16DModel[0].v3Rotation.Z);
                        m.Translation = m16DModel[0].v3Position + Vector3.Transform(new Vector3(-240, 0, 0), Matrix.CreateFromYawPitchRoll(m16DModel[0].v3Rotation.Y, m16DModel[0].v3Rotation.X, m16DModel[0].v3Rotation.Z));

                        //Matrix m = Matrix.CreateTranslation(m16DModel[0].v3Position + new Vector3(-240, 0, 0));                      
                        Graphics.Projectile p = AddProjectile(projectile,
                            1, m, velocity, 0.1f,
                            RenderTechnique.ViewMapping);

                        p.SetExplosion(AnimSpriteType.Blaster,
                            30, 30, DrawMode.AdditiveAndGlow, 0, 0, null);

                        // fire right blaster                      
                        Matrix m1 = new Matrix();
                        m1 = Matrix.CreateFromYawPitchRoll(m16DModel[0].v3Rotation.Y, m16DModel[0].v3Rotation.X, m16DModel[0].v3Rotation.Z);
                        m1.Translation = m16DModel[0].v3Position + Vector3.Transform(new Vector3(240, 0, 0), Matrix.CreateFromYawPitchRoll(m16DModel[0].v3Rotation.Y, m16DModel[0].v3Rotation.X, m16DModel[0].v3Rotation.Z));

                        Graphics.Projectile p1 = AddProjectile(projectile,
                           1, m1, velocity, 0.1f,
                           RenderTechnique.ViewMapping);

                        p1.SetExplosion(AnimSpriteType.Blaster,
                            30, 30, DrawMode.AdditiveAndGlow, 0, 0, null);

                        // play blaster fire sound
                        PlaySound("fire_primary");
                    }
                    break;
                case ProjectileType.Missile:
                    {
                        // fire left missile                    
                        Matrix m = new Matrix();
                        m = Matrix.CreateFromYawPitchRoll(m16DModel[0].v3Rotation.Y, m16DModel[0].v3Rotation.X, m16DModel[0].v3Rotation.Z);
                        m.Translation = m16DModel[0].v3Position + Vector3.Transform(new Vector3(-240, 0, 0), Matrix.CreateFromYawPitchRoll(m16DModel[0].v3Rotation.Y, m16DModel[0].v3Rotation.X, m16DModel[0].v3Rotation.Z));

                        Graphics.Projectile p = AddProjectile(projectile,
                              1, m, velocity, 0.1f,
                              RenderTechnique.ViewMapping);

                        p.SetExplosion(AnimSpriteType.Missile,
                            90, 30, DrawMode.AdditiveAndGlow,
                            0.5f, 500, "missile_explode");

                        // set missile trail
                        Graphics.ParticleSystem Trail = AddParticleSystem(
                            ParticleSystemType.MissileTrail, Matrix.Identity);
                        p.SetTrail(Trail,
                            Matrix.CreateTranslation(GameOptions.MissileTrailOffset));

                        // fire right missile       
                        Matrix m1 = new Matrix();
                        m1 = Matrix.CreateFromYawPitchRoll(m16DModel[0].v3Rotation.Y, m16DModel[0].v3Rotation.X, m16DModel[0].v3Rotation.Z);
                        m1.Translation = m16DModel[0].v3Position + Vector3.Transform(new Vector3(240, 0, 0), Matrix.CreateFromYawPitchRoll(m16DModel[0].v3Rotation.Y, m16DModel[0].v3Rotation.X, m16DModel[0].v3Rotation.Z));

                        Graphics.Projectile p1 = AddProjectile(projectile,
                            1, m1, velocity, 0.1f,
                            RenderTechnique.ViewMapping);

                        p1.SetExplosion(AnimSpriteType.Missile,
                            90, 30, DrawMode.AdditiveAndGlow,
                            0.5f, 500, "missile_explode");

                        // set missile trail
                        Graphics.ParticleSystem Trail1 = AddParticleSystem(
                            ParticleSystemType.MissileTrail, Matrix.Identity);
                        p1.SetTrail(Trail1,
                            Matrix.CreateTranslation(GameOptions.MissileTrailOffset));

                        // play missile fire sound
                        PlaySound("fire_secondary");
                    }
                    break;
            }
        }

        /// <summary>
        ///     Create a new projectile and add it to the projectile manager
        /// </summary>
        public Graphics.Projectile AddProjectile(
            ProjectileType type,
            int player,
            Matrix transform,
            float velocity,
            float damage,
            RenderTechnique technique)
        {
            // get source and destination positions for projectile
            Vector3 source = transform.Translation;
            var destination = new Vector3();

            if (type == ProjectileType.Missile)
            {
                destination = source + transform.Forward * 10000;
            }
            else if (type == ProjectileType.Blaster)
            {
                destination = source + transform.Forward * 10000;
            }


            // ray intersect level to find out where projetile is going to hit
            float hitDist;
            Vector3 hitPos, hitNormal;
            //            if (levelCollision.PointIntersect(source, destination,
            //                                        out hitDist, out hitPos, out hitNormal))
            //                destination = hitPos;
            //            else
            //                hitNormal = transform.Backward;

            // create projectile
            var p = new Graphics.Projectile(type, projectileModels[(int)type],
                player, velocity, damage, transform, destination, technique);

            // add it to the projectile manager
            projectile.Add(p);

            return p;
        }

        public void DrawScene(GraphicsDevice gd, RenderTechnique technique)
        {
            if (gd == null)
            {
                throw new ArgumentNullException("gd");
            }

            // camera position and view projection matrix             
            Vector3 cameraPosition = chaseCamera.Position;
            Matrix viewProjection = chaseCamera.View * projectionFull;

            // draw the level geomery


            // if in 3rd person mode draw player ship
            bool camera3rdPerson = false;
            //if (camera3rdPerson)
            //    players[0].Draw(gd, technique,
            //        cameraPosition, viewProjection, levelLights);

            // draw projectiles
            projectile.Draw(gd, technique, cameraPosition, viewProjection,
                levelLights);

            // draw powerups


            // draw animated sprites
            animatedSprite.Draw(gd, cameraPosition, m16DModel[0].ViewUp,
                viewProjection, 0, camera3rdPerson);

            // draw particle systems
            particle.Draw(gd, viewProjection);

        }


        public void DrawModel(GraphicsDevice gd, Model model, RenderTechnique technique, Vector3 cameraPosition,
            Matrix world, Matrix viewProjection, LightList light)
        {
            if (gd == null)
            {
                throw new ArgumentNullException("GraphicsDevice");
            }
            //model bones
            model.CopyAbsoluteBoneTransformsTo(bones);

            BlendState bs = gd.BlendState;
            DepthStencilState ds = gd.DepthStencilState;

            gd.BlendState = BlendState.Additive;
            gd.DepthStencilState = DepthStencilState.DepthRead;

            //for each mehs in model
            foreach (ModelMesh mesh in model.Meshes)
            {
                Matrix worldBone = bones[mesh.ParentBone.Index] * world;
                //matrix invert
                Matrix worldBoneInverse = Matrix.Invert(worldBone);

                //compute camera position in object space
                Vector3 cameraObjectSpaces = cameraPosition - worldBone.Translation;
                cameraObjectSpaces = Vector3.Transform(cameraObjectSpaces, worldBoneInverse);
                //samplerstate determine how to sapmle textrure data
                gd.SamplerStates[0] = SamplerState.LinearWrap;

                //for each mesh part
                foreach (ModelMeshPart meshpart in mesh.MeshParts)
                {
                    //setup vertices and indices
                    if (meshpart.PrimitiveCount > 0)
                    {
                        //setup vertices and indices
                        gd.SetVertexBuffer(meshpart.VertexBuffer);
                        gd.Indices = meshpart.IndexBuffer;

                        //setup effect
                        Effect effect = meshpart.Effect;
                        effect.Parameters["WorldViewProj"].SetValue(worldBone * viewProjection);
                        effect.Parameters["CameraPosition"].SetValue(cameraObjectSpaces);

                        //setup technique
                        effect.CurrentTechnique = meshpart.Effect.Techniques[(int)technique];

                        //if not lights specified
                        if (light == null)
                        {
                            effect.CurrentTechnique.Passes[0].Apply();
                            //draw with plain mapping
                            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                meshpart.VertexOffset, 0, meshpart.NumVertices,
                                meshpart.StartIndex, meshpart.PrimitiveCount);
                            gd.SetVertexBuffer(null);
                            gd.Indices = null;
                        }
                        else
                        {
                            gd.DepthStencilState = DepthStencilState.Default;
                            gd.BlendState = BlendState.Opaque;

                            // get light effect parameters
                            EffectParameter effectLightPosition =
                                effect.Parameters[1];
                            EffectParameter effectLightColor =
                                effect.Parameters[2];
                            EffectParameter effectLightAmbient =
                                effect.Parameters[3];

                            // ambient light
                            Vector3 ambient = light.ambient;

                            // for each light
                            foreach (Light lights in light.lights)
                            {
                                // setup light in effect
                                effectLightAmbient.SetValue(ambient);
                                lights.SetEffect(effectLightPosition,
                                    effectLightColor, worldBoneInverse);

                                // begin effect
                                effect.CurrentTechnique.Passes[0].Apply();
                                // draw primitives
                                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                    meshpart.VertexOffset, 0, meshpart.NumVertices,
                                    meshpart.StartIndex, meshpart.PrimitiveCount);

                                // setup additive blending with no depth write
                                gd.DepthStencilState = DepthStencilState.DepthRead;
                                gd.BlendState = BlendState.Additive;

                                // clear ambinet light (applied in first pass only)
                                ambient = Vector3.Zero;
                            }

                            // clear vertices and indices
                            gd.SetVertexBuffer(null);
                            gd.Indices = null;
                        }
                    }
                }
            }
            gd.DepthStencilState = ds;
            gd.BlendState = bs;
        }


        public static float TurnToFace(Vector3 position, Vector3 faceThis, float currentAngle, float turnSpeed)
        {
            float x = faceThis.X - position.X;
            float y = faceThis.Y - position.Y;
            float z = faceThis.Z - position.Z;

            var desiredAngle = (float)Math.Atan2(y, x);

            float difference = WrapAngle(desiredAngle - currentAngle);

            difference = MathHelper.Clamp(difference, -turnSpeed, turnSpeed);

            return WrapAngle(currentAngle + difference);
        }

        private static float WrapAngle(float radians)
        {
            while (radians < -MathHelper.Pi)
            {
                radians += MathHelper.TwoPi;
            }
            while (radians > MathHelper.Pi)
            {
                radians -= MathHelper.TwoPi;
            }
            return radians;
        }


    }
}