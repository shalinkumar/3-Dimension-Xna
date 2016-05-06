using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace JohnStriker.Camera
{
    public class Camera : GameComponent
    {
        //Camera matrices
        private readonly Vector3 _cameraDirection;
        private readonly Vector3 _cameraUp;
        private float currentPitch = 0;

        // Mouse stuff
        private float currentYaw = 0;
        private MouseState prevMouseState;
        private float totalPitch = MathHelper.PiOver4/2;
        private float totalYaw = MathHelper.PiOver4/2;

        public Camera(Game game, Vector3 pos, Vector3 target, Vector3 up, string screenType)
            : base(game)
        {
            // Build camera view matrix
            CameraPosition = pos;
            _cameraDirection = target - pos;
            _cameraDirection.Normalize();
            _cameraUp = up;
            CreateLookAt();

            if (screenType == "ScreenIntro")
                Projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.PiOver2,
                    Game.Window.ClientBounds.Width/
                    (float) Game.Window.ClientBounds.Height,
                    1, 3000);
            if (screenType == "ScreenGame")
                Projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.PiOver4,
                    Game.Window.ClientBounds.Width/
                    (float) Game.Window.ClientBounds.Height,
                    1, 9000);
        }

        public Matrix View { get; protected set; }
        public Matrix Projection { get; protected set; }

        // Camera vectors
        public Vector3 CameraPosition { get; protected set; }

        public Vector3 GetCameraDirection
        {
            get { return _cameraDirection; }
        }

        private void CreateLookAt()
        {
            View = Matrix.CreateLookAt(CameraPosition,
                CameraPosition + _cameraDirection, _cameraUp);
        }
    }
}