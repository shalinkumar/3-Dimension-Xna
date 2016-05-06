using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace JohnStriker
{
    public class GameOptions
    {

        // game screen horizontal resolution
        public static int ScreenWidth = 1280;
        // game screen vertical resolution
        public static int ScreenHeight = 720;

        // color used for screen transitions
        public static Vector4 FadeColor = Vector4.Zero;
        // time for screen transition in seconds
        public static float FadeTime = 1.0f;

        // size of player collision box
        public static int CollisionBoxRadius = 60;

        // how many octree subdivisions in collision mesh
        public static uint CollisionMeshSubdivisions = 4;

        // glow buffer resolution
        public static int GlowResolution = 512;

        // max simultaneous animated sprites per frame
        public static int MaxSprites = 128;

        // max simultaneous particles per frame
        public static int MaxParticles = 8192;

        // maximum number of supported players
        public static int MaxPlayers = 1;

        //max bones per model
        public static int MaxBonesPerMode = 128;

        // blaster velocity
        public static float BlasterVelocity = 6000;

        // missile velocity
        public static float MissileVelocity = 4000;

        // time between two blasters fire
        public static float BlasterChargeTime = 0.2f;

        // time between two missiles fire
        public static float MissileChargeTime = 0.5f;

        // offset for camera in 1st person mode
        public static Vector3 CameraViewOffset = new Vector3(0, -10, 0);

        // max flight velocity
        public static float MovementVelocity = 700;

        // bobbing distance
        public static float ShipBobbingRange = 4.0f;
        // bobbing speed
        public static float ShipBobbingSpeed = 4.0f;

        // maximum bones per model
        public static int MaxBonesPerModel = 128;

        public static Vector3 MissileTrailOffset = new Vector3(0, 0, -10);

        // damping force used to stop rotation
        public static float MovementRotationForceDamping = 3.0f;

        // max rotation velocity
        public static float MovementRotationVelocity = 1.1f;

        // rotation force applied by controls to rotate ship
        public static float MovementRotationForce = 5.0f;
    }
}
