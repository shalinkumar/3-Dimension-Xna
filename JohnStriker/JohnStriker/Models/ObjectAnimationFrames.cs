using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace JohnStriker.Models
{
    public class ObjectAnimationFrames
    {
        public string Thrusters { get; private set; }
        public string Rotation { get; private set; }
        public string Pitch { get; private set; }
        public TimeSpan Time { get; private set; }

        public Vector3 ValPosition { get; private set; }
        public Vector3 ValRotation { get; private set; }

        public ObjectAnimationFrames(string thrusters, string rotation, string pitch, TimeSpan time)
        {
            if (thrusters == "thrusters forward")
            {
                ValPosition=new Vector3(1,1,1);
            }
            if (rotation == "rotate left")
            {
                ValRotation=new Vector3(2,2,2);
            }
            if (rotation == "rotate right")
            {
                ValRotation = new Vector3(3, 3, 3);
            }
            //Thrusters = thrusters;
            //Rotation = rotation;
            Pitch = pitch;
            Time = time;
        }
    }


    public class KeyframedObjectAnimations
    {
        List<ObjectAnimationFrames> Frames = new List<ObjectAnimationFrames>();
        private bool Loop;
        private TimeSpan ElapsedTime = TimeSpan.FromSeconds(0);

        internal Vector3 Rotation { get; set; }
        internal Vector3 Position { get; set; }

        public KeyframedObjectAnimations(List<ObjectAnimationFrames> frames, bool loop)
        {
            Frames = frames;
            Loop = loop;
            Rotation = frames[0].ValPosition;
            Position = frames[0].ValRotation;
        }

        public void Update(TimeSpan elapsed)
        {
            //update the time
            ElapsedTime += elapsed;
            TimeSpan TotalTime = ElapsedTime;
            TimeSpan End = Frames[Frames.Count - 1].Time;

            //loop ariound the total time if necessary
            if (Loop)
            {
                while (TotalTime > End)
                    TotalTime -= End;
            }
            else // Otherwise, clamp to the end values
            {
                Position = Frames[Frames.Count - 1].ValPosition;
                Rotation = Frames[Frames.Count - 1].ValRotation;
                return;
            }

            int i = 0;

            //find the index of the current frame
            while (Frames[i + 1].Time < TotalTime)
            {
                i++;
            }

            // Find the time since the beginning of this frame
            TotalTime -= Frames[i].Time;

            // Find how far we are between the current and next frame (0 to 1)
            float amt = (float)((TotalTime.TotalSeconds) /
                (Frames[i + 1].Time - Frames[i].Time).TotalSeconds);

            // Interpolate position and rotation values between frames
            //Position = CatmullRom3D(
            //    Frames[Wrap(i - 1, Frames.Count - 1)].ValPosition,
            //    Frames[Wrap(i, Frames.Count - 1)].ValPosition,
            //    Frames[Wrap(i + 1, Frames.Count - 1)].ValPosition,
            //    Frames[Wrap(i + 2, Frames.Count - 1)].ValPosition,
            //    amt);
            Position = Vector3.Lerp(Frames[i].ValPosition, Frames[i + 1].ValPosition, amt);
            Rotation = Vector3.Lerp(Frames[i].ValRotation, Frames[i + 1].ValRotation, amt);
        }

        private Vector3 CatmullRom3D(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float amt)
        {
            return new Vector3(MathHelper.CatmullRom(v1.X, v2.X, v3.X, v4.X, amt), MathHelper.CatmullRom(v1.Y, v2.Y, v3.Y, v4.Y, amt), MathHelper.CatmullRom(v1.Z, v2.Z, v3.Z, v4.Z, amt));

        }

        // Wraps the "value" argument around [0, max]
        private int Wrap(int value, int max)
        {
            while (value > max)
            {
                value -= max;
            }
            while (value < 0)
            {
                value += max;
            }
            return value;
        }
    }
}
