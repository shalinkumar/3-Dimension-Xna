using JohnStriker.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JohnStriker
{
    /// <summary>
    ///     This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        private readonly GameManager gameManager;
        private readonly GraphicsDeviceManager graphics;
        private AudioEngine audioEngine;
        private FontManager fontManager;
        private GameOptions gameOptions;


        private bool renderVsync = true;
        private ScreenManager screenManager;
        private SoundBank soundBank;
        private SpriteBatch spriteBatch;
        private WaveBank waveBank;

        public GameTime GameTime { get; set; }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            audioEngine = new AudioEngine("content/Audio/sounds.xgs");
            waveBank = new WaveBank(audioEngine, "content/Audio/wave bank.xwb");
            soundBank = new SoundBank(audioEngine, "content/Audio/Sound Bank.xsb");

            gameManager = new GameManager(soundBank,Components,this);           

            graphics.PreferredBackBufferWidth = GameOptions.ScreenWidth;
            graphics.PreferredBackBufferHeight = GameOptions.ScreenHeight;

            IsFixedTimeStep = renderVsync;
            graphics.SynchronizeWithVerticalRetrace = renderVsync;          
        }

        /// <summary>
        ///     Allows the game to perform any initialization it needs to before starting to run.
        ///     This is where it can query for any required services and load any non-graphic
        ///     related content.  Calling base.Initialize will enumerate through any components
        ///     and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            // Initialize Camera

            base.Initialize();
        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            fontManager = new FontManager(graphics.GraphicsDevice);
            screenManager = new ScreenManager(this, gameManager, fontManager);

            // TODO: use this.Content to load your game content here

            screenManager = new ScreenManager(this, gameManager, fontManager);

            fontManager.LoadContent(Content);
            gameManager.LoadContent(graphics.GraphicsDevice, Content);
            screenManager.LoadContent(graphics.GraphicsDevice, Content);
            //gameManager.LoadFiles(Content, GraphicsDevice);
        }

        /// <summary>
        ///     UnloadContent will be called once per game and is the place to unload
        ///     all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        ///     This is called to switch full screen mode.
        /// </summary>
        public void ToggleFullScreen()
        {
            graphics.ToggleFullScreen();
        }

        /// <summary>
        ///     Allows the game to run logic such as updating the world,
        ///     checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            // TODO: Add your update logic here
            int ElapsedTimeFloat = gameTime.ElapsedGameTime.Seconds;

            screenManager.ProcessInput(gameTime);
            screenManager.Update(graphics.GraphicsDevice, gameTime);
         
            base.Update(gameTime);
        }

        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            screenManager.Draw(graphics.GraphicsDevice);
            //gameManager.Draw3D(graphics.GraphicsDevice);
            base.Draw(gameTime);
        }
    }
}