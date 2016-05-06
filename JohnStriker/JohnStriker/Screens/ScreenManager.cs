using System;
using System.Collections.Generic;
using JohnStriker.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JohnStriker.Screens
{
    //2) The class that has been written is never such that in future no changes will be made and if such changes are made and new resources
    /// <summary>
    ///     1) As the per the coding standards followed by so many developers it is always good to implement IDisposable on the
    ///     classes which use some resources
    ///     and once the scope of that object is over the Dispose method in that class will make sure all the resources have
    ///     been released.
    ///     are added then the developer knows that he has to release those resources in Dispose function.
    /// </summary>
    public class ScreenManager : IDisposable
    {
        private readonly FontManager _fontManager;
        private readonly GameManager _gameManager;
        private readonly InputManager _inputManager;
        private readonly Game1 johnStriker;
        private readonly List<Screen> screenList;
        private float backgroundTime; // time for background animation used on menus

        private BlurManager blurManager;

        private RenderTarget2D colorRT; // render target for main color buffer
        private ContentManager contentManager;
        private Screen current; // currently active screen       
        private float fade;
        private Vector4 fadeColor;
        private float fadeTime;
        private int frameRate; // current game frame rate (in frames per sec)
        private int frameRateCount; // current frame count since last frame rate update
        private float frameRateTime; // elapsed time since last frame rate update
        private Game1 game1;
        private RenderTarget2D glowRT1; // render target for glow horizontal blur
        private RenderTarget2D glowRT2; // render target for glow vertical blur
        private Screen next; //next screen on a transition;
        private Texture2D textureBackground; // the background texture used on menus       

        public ScreenManager(Game1 game1, GameManager gameManager, FontManager fontManager)
        {
            johnStriker = game1;
            _gameManager = gameManager;
            _fontManager = fontManager;
            screenList = new List<Screen>();

            _inputManager = new InputManager();


            //add all screens
            screenList.Add(new ScreenIntro(this, gameManager));
            screenList.Add(new ScreenGameContent(this, gameManager));
            screenList.Add(new ScreenHelp(this, gameManager));
            screenList.Add(new ScreenGame(this, gameManager));
            // fade in to intro screen
            SetNextScreen(ScreenType.ScreenIntro,
                GameOptions.FadeColor, GameOptions.FadeTime);
            fade = fadeTime*0.5f;

            camera = new Camera.Camera(game1, new Vector3(0, 0, 50),
                Vector3.Zero, Vector3.Up, "ScreenIntro");

            //CrossHaircamera = new Camera.Camera(game1, new Vector3(910, 0, 300),
            //   new Vector3(0, 0, 10), Vector3.Up);
        }

        public Camera.Camera camera { get; protected set; }

        public Camera.Camera CrossHaircamera { get; protected set; }

        public void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }

            contentManager = content;
            textureBackground = content.Load<Texture2D>("screens/Space");
            // create blur manager
            blurManager = new BlurManager(graphicsDevice,
                content.Load<Effect>("Shaders/Blur"),
                GameOptions.GlowResolution, GameOptions.GlowResolution);

            int width = graphicsDevice.Viewport.Width;
            int height = graphicsDevice.Viewport.Height;


            // create render targets
            colorRT = new RenderTarget2D(graphicsDevice, width, height,
                true, SurfaceFormat.Color, DepthFormat.Depth24);
            glowRT1 = new RenderTarget2D(graphicsDevice, GameOptions.GlowResolution, GameOptions.GlowResolution,
                true, SurfaceFormat.Color, DepthFormat.Depth24);
            glowRT2 = new RenderTarget2D(graphicsDevice, GameOptions.GlowResolution, GameOptions.GlowResolution,
                true, SurfaceFormat.Color, DepthFormat.Depth24);
        }

        public bool SetNextScreen(ScreenType screenType, Vector4 fadeColor, float fadeTime)
        {
            if (next == null)
            {
                next = screenList[(int) screenType];
                this.fadeColor = fadeColor;
                this.fadeTime = fadeTime;
                fade = fadeTime;
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Unload all content
        /// </summary>

        //start a transition to new screen, using a one sec fade to black
        public bool SetNextScreen(ScreenType screenType)
        {
            return SetNextScreen(screenType, Vector4.Zero, 1.0f);
        }

        // exit game
        public void Exit()
        {
            johnStriker.Exit();
        }

        // process input
        public void ProcessInput(GameTime gameTime)
        {
            _inputManager.BeginInputProcessing(true);

            // process input for currently active screen
            if (current != null && next == null)
            {
                current.ProcessInput(gameTime, _inputManager);
            }


            // toggle full screen with F5 key
            if (_inputManager.IsKeyPressed(0, Keys.F5) ||
                _inputManager.IsKeyPressed(1, Keys.F5))
            {
                johnStriker.ToggleFullScreen();
            }


            _inputManager.EndInputProcessing();
        }

        public GameTime UpdateGameTime(GameTime gameTime)
        {
            return gameTime;
        }

        // update for given elapsed time
        public void Update(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            // if in a transition
            if (fade > 0)
            {
                // update transition time
                fade -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                // if time to switch to new screen (fade out finished)
                if (next != null && fade < 0.5f*fadeTime)
                {
                    // tell new screen it is getting in focus
                    next.Setfocus(contentManager, true, graphicsDevice);

                    // tell the old screen it lost its focus
                    if (current != null)
                        current.Setfocus(contentManager, false, graphicsDevice);

                    // set new screen as current
                    current = next;
                    next = null;
                }
            }

            // if current screen available, update it
            if (current != null)
                current.Update(gameTime);

            // calulate frame rate
            frameRateTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (frameRateTime > 0.5f)
            {
                frameRate = (int) (frameRateCount/frameRateTime);
                frameRateCount = 0;
                frameRateTime = 0;
            }

            //accumulate elapsed time for background animation
            backgroundTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        //draw background animation
        public void DrawBackGround(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }
            //if (current.ToString() == "JohnStriker.Screens.ScreenGame")
            //{              

            //}
            //else
            //{
            const float animationtime = 3.0f;
            const float animationLength = 0.4f;
            const int numberLayers = 2;
            const float layerDistance = 1.0f/numberLayers;

            //normalized time
            float normalizedTime = (backgroundTime/animationtime)%1.0f;

            //set render state

            //The depth buffer stores floating-point depth or z data for each pixel while the stencil buffer stores integer data for each pixel. 
            //The depth-stencil state class, DepthStencilState, contains state that controls how depth and stencil data impacts rendering.
            DepthStencilState ds = graphicsDevice.DepthStencilState;
            //Transparent PNGs are drawn as expected, with anything behind them preserved
            //We use Color.White to draw a PNG as-is
            //We will change the alpha channel of the color to change the "opacity" of the texture
            BlendState bs = graphicsDevice.BlendState;
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            graphicsDevice.BlendState = BlendState.AlphaBlend;

            float scale;
            Vector4 color;

            //render all background layers
            for (int i = 0; i < numberLayers; i++)
            {
                if (normalizedTime > 0.5f)
                {
                    scale = 2 - normalizedTime*2;
                }
                else
                {
                    scale = normalizedTime*2;
                }
                color = new Vector4(scale, scale, scale, 0);

                scale = 1 + normalizedTime*animationLength;
                //if (current.ToString() != "JohnStriker.Screens.ScreenGame")
                //{
                blurManager.RenderScreenQuad(graphicsDevice, BlurManager.BlurTechnique.ColorTexture, textureBackground,
                    color, scale);
                //}


                normalizedTime = (normalizedTime + layerDistance)%1.0f;
            }

            // restore render states
            graphicsDevice.DepthStencilState = ds;
            graphicsDevice.BlendState = bs;

            //}
        }

        // draw render target as fullscreen texture with given intensity and blend mode
        private void DrawRenderTargetTexture(
            GraphicsDevice gd,
            RenderTarget2D renderTarget,
            float intensity,
            bool additiveBlend)
        {
            if (gd == null)
            {
                throw new ArgumentNullException("gd");
            }

            // set up render state and blend mode
            //BlendState bs = gd.BlendState;
            //gd.DepthStencilState = DepthStencilState.Default;
            //if (additiveBlend)
            //{
            //    gd.BlendState = BlendState.Additive;
            //}

            gd.DepthStencilState = DepthStencilState.None;
            if (additiveBlend)
            {
                gd.BlendState = BlendState.Additive;
            }


            // draw render tareget as fullscreen texture
            blurManager.RenderScreenQuad(gd, BlurManager.BlurTechnique.ColorTexture,
                renderTarget, new Vector4(intensity));

            // restore render state and blend mode
            //gd.BlendState = bs;
            //gd.DepthStencilState = DepthStencilState.Default;
            //gd.BlendState = BlendState.Opaque;
            gd.DepthStencilState = DepthStencilState.Default;
        }

        // draw a texture with destination rectangle, color and blend mode
        public void DrawTexture(
            Texture2D texture,
            Rectangle rect,
            Color color,
            BlendState blend)
        {
            _fontManager.DrawTexture(texture, rect, color, blend);
        }

        // draws the currently active screen
        public void Draw(GraphicsDevice gd)
        {
            if (gd == null)
            {
                throw new ArgumentNullException("gd");
            }

            frameRateCount++;

            // if a valid current screen is set
            if (current != null)
            {
                if (current.ToString() == "JohnStriker.Screens.ScreenGame")
                {
                    gd.BlendState = BlendState.Opaque;
                    gd.DepthStencilState = DepthStencilState.Default;
                    gd.SamplerStates[0] = SamplerState.LinearWrap;

                    current.Draw3D(gd);

                    //draw the text 
                    string text = string.Empty;
                    current.DrawText(text);

                    //draw the 2D scene 
                    current.Draw2d(gd, _fontManager);
                }
                else
                {
                    // set the color render target
                    gd.SetRenderTarget(colorRT);

                    // draw the screen 3D scene
                    current.Draw3D(gd);

                    // resolve the color render target
                    gd.SetRenderTarget(null);

                    // blur the glow render target


                    // draw the 3D scene texture
                    DrawRenderTargetTexture(gd, colorRT, 1.0f, false);

                    // draw the glow texture with additive blending
                    DrawRenderTargetTexture(gd, glowRT2, 2.0f, true);

                    // begin text mode
                    _fontManager.BeginText();

                    // draw the 2D scene 
                    current.Draw2d(gd, _fontManager);

                    // draw fps
                    //fontManager.DrawText(
                    //    FontType.ArialSmall,
                    //    "FPS: " + frameRate,
                    //    new Vector2(gd.Viewport.Width - 80, 0), Color.White);

                    // end text mode
                    _fontManager.EndText();
                }
            }

            // if in a transition
            if (fade > 0)
            {
                // compute transtition fade intensity
                float size = fadeTime*0.5f;
                fadeColor.W = 1.25f*(1.0f - Math.Abs(fade - size)/size);

                // set alpha blend and no depth test or write
                gd.DepthStencilState = DepthStencilState.None;
                gd.BlendState = BlendState.AlphaBlend;

                if (current.ToString() != "JohnStriker.Screens.ScreenGame")
                {
                    // draw transition fade color
                    blurManager.RenderScreenQuad(gd, BlurManager.BlurTechnique.Color, null, fadeColor);
                }


                // restore render states
                gd.DepthStencilState = DepthStencilState.Default;
                gd.BlendState = BlendState.Opaque;
            }
        }

        public void UnloadContent()
        {
            textureBackground = null;
            if (blurManager != null)
            {
                blurManager.Dispose();
                blurManager = null;
            }

            if (colorRT != null)
            {
                colorRT.Dispose();
                colorRT = null;
            }
            if (glowRT1 != null)
            {
                glowRT1.Dispose();
                glowRT1 = null;
            }
            if (glowRT2 != null)
            {
                glowRT2.Dispose();
                glowRT2 = null;
            }
        }

        #region IDisposable memberas

        public bool isDisposed = false;

        public bool IsDisposed
        {
            get { return isDisposed; }
        }

        public void Dispose()
        {
            Dispose(true);
            //controls the system garbage collector, a service that reclaims unused memory.
            //request the system not call the finalizer for the specific object;
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                UnloadContent();
            }
        }

        #endregion
    }
}