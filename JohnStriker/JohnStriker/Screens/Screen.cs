
#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace JohnStriker.Screens
{
    public enum ScreenType
    {
        ScreenIntro,
        ScreenGameContent,        
        ScreenHelp,       
        ScreenGame,
        ScreenLevel,        
        ScreenEnd
    }
    public abstract class Screen
    {
        //This method is used for when screen gets loose focus.
        public abstract void Setfocus(ContentManager content, bool focus, GraphicsDevice graphicsDevice);
        //Called input
        public abstract void ProcessInput(GameTime gameTime, InputManager inputManager);
        //update state of game
        public abstract void Update(GameTime gameTime);
        //Called to draw 3d 
        public abstract void Draw3D(GraphicsDevice graphicsDevice);
        //Calledd to draw 2d 
        public abstract void Draw2d(GraphicsDevice graphicsDevice,FontManager font);

        public abstract void DrawText(string text);
    }
}
