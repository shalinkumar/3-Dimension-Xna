using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JohnStriker.Models
{
    public class ParticleExplosion
    {
        //particle array and vertex buffer
        private VertexPositionTexture[] _vertsTextures;
        private Vector3[] _vectorDirectionArray;
        private Color[] _vertexColorArray;
        private VertexBuffer _particleVertexBuffer;

        //Position
        private Vector3 _position;

        //lifeleft
        private int _lifeLeft;

        //rounds and particles count
        private int _numberParticlesPerRound;
        private int _maxParticles;
        static Random rnd = new Random();
        private int _roundTime;
        private int _timeSinceLastRound;

        //vertex and graphic info;
        private GraphicsDevice _graphicsDevice;

        //settings
        private ParticleSettings _particleSettings;

        //particle effect
        private Effect _particleEffect;

        //texture
        private Texture2D _particlecolorTexture;

        //array indices
        private int _endofLiveParticleIndex;
        private int _endofDeadParticleIndex;

        public bool IsDead
        {
            get { return _endofLiveParticleIndex == _maxParticles; }
        }

        public ParticleExplosion(GraphicsDevice graphicsDevice, Vector3 position, int lifeLeft, int roundtime, int numberParticlesPerRound, int maxParticles, Texture2D particlecolorTexture2D,
            ParticleSettings particleSettings, Effect particleEffect)
        {
            _graphicsDevice = graphicsDevice;
            _position = position;
            _lifeLeft = lifeLeft;
            _roundTime = roundtime;
            _numberParticlesPerRound = numberParticlesPerRound;
            _maxParticles = maxParticles;
            _particlecolorTexture = particlecolorTexture2D;
            _particleSettings = particleSettings;
            _particleEffect = particleEffect;

            InitializeParticleVertices();
        }

        private void InitializeParticleVertices()
        {
            //instantiate all particle arrays

            _vertsTextures=new VertexPositionTexture[_maxParticles * 4];
            _vectorDirectionArray=new Vector3[_maxParticles];
            _vertexColorArray=new Color[_maxParticles];

            //get color data from colors texture
            Color[] colors = new Color[_particlecolorTexture.Width * _particlecolorTexture.Height];
            _particlecolorTexture.GetData(colors);


            //loop until max particles
            for (int i = 0; i < _maxParticles; i++)
            {
                float size = (float)rnd.NextDouble() * _particleSettings._maxSize;
                //set position and direction and size of particles
                _vertsTextures[i * 4]=new VertexPositionTexture(_position,new Vector2(0,0));
                _vertsTextures[(i*4)+1]=new VertexPositionTexture(new Vector3(_position.X,_position.Y+size,_position.Z),new Vector2(0,1));
                _vertsTextures[(i * 4) + 2] = new VertexPositionTexture(new Vector3(_position.X + size, _position.Y, _position.Z), new Vector2(1, 0));
                _vertsTextures[(i * 4)+3]=new VertexPositionTexture(new Vector3(_position.X+size,_position.Y,_position.Z),new Vector2(1,1));

                //create a random velocity direction
                Vector3 direction=new Vector3((float)rnd.NextDouble()*2-1,(float)rnd.NextDouble()*2-1,(float)rnd.NextDouble()*2-1);
                direction.Normalize();

                //multiply by nextdouble to make sure all particles move  by same speed
                direction *= (float)rnd.NextDouble();
                //set direction of particles
                _vectorDirectionArray[i] = direction;
                // Set color of particle by getting a random color from the texture
                _vertexColorArray[i] =
                    colors[
                        (rnd.Next(0, _particlecolorTexture.Height)*_particlecolorTexture.Width) +rnd.Next(0,  _particlecolorTexture.Width)];

            }

            //instantiate vertex buffer
            _particleVertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionTexture),_vertsTextures.Length,BufferUsage.None);
        }

        public void Update(GameTime gameTime)
        {
            //decrement life untill its gone
            if (_lifeLeft > 0)
                _lifeLeft -= gameTime.ElapsedGameTime.Milliseconds;
            //time for new round
            _timeSinceLastRound += gameTime.ElapsedGameTime.Milliseconds;
            if (_timeSinceLastRound > _roundTime)
            {
                //new round add and remove particles
                _timeSinceLastRound -= _roundTime;
                // Increment end of live particles index each
                // round until end of list is reached
                if (_endofLiveParticleIndex < _maxParticles)
                {
                    _endofLiveParticleIndex += _numberParticlesPerRound;
                    if (_endofLiveParticleIndex > _maxParticles)
                        _endofLiveParticleIndex = _maxParticles;
                }
                if (_lifeLeft < 0)
                {
                    //increment end of dead particles index each
                    //round untill end of list is reached
                    if (_endofDeadParticleIndex < _maxParticles)
                    {
                        _endofDeadParticleIndex += _numberParticlesPerRound;
                        if (_endofDeadParticleIndex > _maxParticles)
                            _endofDeadParticleIndex = _maxParticles;
                    }
                }
            }

            //update position of live particles
            for (int i = _endofDeadParticleIndex; i < _endofLiveParticleIndex; ++i)
            {
                _vertsTextures[i*4].Position += _vectorDirectionArray[i];
                _vertsTextures[(i * 4)+1].Position += _vectorDirectionArray[i];
                _vertsTextures[(i * 4)+2].Position += _vectorDirectionArray[i];
                _vertsTextures[(i * 4)+3].Position += _vectorDirectionArray[i];
            }
        }

        public void Draw(Camera.Camera camera)
        {
            _graphicsDevice.SetVertexBuffer(_particleVertexBuffer);
            //only draqqw if there are live particles
            if (_endofLiveParticleIndex - _endofDeadParticleIndex > 0)
            {
                for (int i = _endofDeadParticleIndex; i < _endofLiveParticleIndex; ++i)
                {
                    _particleEffect.Parameters["WorldViewProjection"].SetValue(camera.View * camera.Projection);

                    _particleEffect.Parameters["particleColor"].SetValue(_vertexColorArray[i].ToVector4());

                    //Draw particles
                    foreach (EffectPass pass in _particleEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        _graphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip,_vertsTextures,i * 4,2);
                    }
                }
            }
        }
    }
}
