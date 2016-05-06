using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace JohnStriker
{

    public enum FontType
    {
        ArialSmall = 0,
        ArialMedium,
        ArialLarge
    };
    public class FontManager
    {

        GraphicsDevice graphics;    // graphics device
        SpriteBatch sprite;         // sprite bacth
        List<SpriteFont> fonts;     // list of sprite fonts
        bool textMode;              // in text mode?

        public Rectangle ScreenRectangle
        {
            get
            {
                return new Rectangle(graphics.Viewport.X, graphics.Viewport.Y, graphics.Viewport.Width, graphics.Viewport.Height);
            }
        }
        /// <summary>
        /// End text mode
        /// </summary>
        public void EndText()
        {
            sprite.End();
            textMode = false;
        }

        /// <summary>
        /// Enter text mode
        /// </summary>
        public void BeginText()
        {
            sprite.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            textMode = true;
        }


        public FontManager(GraphicsDevice gd)
        {
            if (gd == null)
            {
                throw new ArgumentNullException("gd");
            }

            graphics = gd;
            sprite = new SpriteBatch(gd);
            fonts = new List<SpriteFont>();
            textMode = false;
        }

        public void LoadContent(ContentManager content)
        {
            fonts.Add(content.Load<SpriteFont>(@"fonts/ArialS"));
            fonts.Add(content.Load<SpriteFont>(@"fonts/ArialM"));
            fonts.Add(content.Load<SpriteFont>(@"fonts/ArialL"));
        }

        /// <summary>
        /// Draw a texture in screen
        /// </summary>
        public void DrawTexture(
            Texture2D texture,
            Rectangle rect,
            Color color,
            BlendState blend)
        {
            if (textMode)
                sprite.End();
            //Original
            //sprite.Begin(SpriteSortMode.Immediate, blend);
            //My change -- this is used draw the 2d on the 3d screen.
            sprite.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
           SamplerState.LinearClamp, DepthStencilState.Default,
           RasterizerState.CullCounterClockwise);
            sprite.Draw(texture, rect, color);
            sprite.End();

            if (textMode)
                sprite.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        }


          /// <summary>
        /// Drawn text using given font, position and color
        /// </summary>
        public void DrawText(FontType font, String text, Vector2 position, Color color)
        {
            if (textMode)
                sprite.DrawString(fonts[(int)font], text, position, color);
        }
    }
}
