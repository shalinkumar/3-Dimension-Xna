using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace JohnStriker.Models
{
    public class ParticleSettings
    {
        public int _maxSize = 2;

    }

    public class ParticleExplosionSettings
    {
        //life of particles
        public int _minLife = 1000;
        public int _maxLife = 2000;

        //particle per round
        public int _minParticlePerRound = 100;
        public int _maxParticlePerRound = 600;

        //round time
        public int _minroundTime = 16;
        public int _maxRoundTime = 50;

        //number of particles
        public int _minParticles = 2000;
        public int _maxParticles = 3000;
    }

}
