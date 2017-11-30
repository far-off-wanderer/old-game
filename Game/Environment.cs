﻿using Conesoft.Engine.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Conesoft.Game
{
    public class DefaultEnvironment
    {
        public Vector3 Acceleration { get; set; }
        public Size ScreenSize { get; set; }
        public bool Flipped { get; set; }
        public System.Random Random { get; set; }
        public Dictionary<string, BoundingSphere> ModelBoundaries { get; set; }
        public IResource<SoundEffect> Sounds { get; set; }
        public Camera ActiveCamera { get; set; }

        public Vector3 RandomDirection()
        {
            return Vector3.Normalize(RandomPointInUnitSphere());
        }

        public Vector3 RandomPointInUnitSphere()
        {
            var direction = Vector3.Zero;
            do
            {
                direction.X = (float)(Random.NextDouble() * 2 - 1);
                direction.Y = (float)(Random.NextDouble() * 2 - 1);
                direction.Z = (float)(Random.NextDouble() * 2 - 1);
            } while (direction.LengthSquared() > 1);
            return direction;
        }
    }
}
