﻿using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Conesoft.Game
{
    using Conesoft;
    public class Spaceship : ControllableObject3D
    {
        public ThrustFlame ThrustFlame { get; set; }

        public Quaternion ShipLeaning
        {
            get
            {
                return Quaternion.CreateFromAxisAngle(Vector3.Forward, -rotation);
            }
        }

        public Quaternion Rotation
        {
            get
            {
                return Quaternion.CreateFromAxisAngle(Vector3.Up, rotation);
            }
        }

        public float Speed { get; set; }

        private float rotation = 0;
        private float maxRotation = (float)(Math.PI / 2) / 5;
        private float rotationSpeed = 0.5f;

        private float targetRotation = 0;
        private float forwardAcceleration = 0;

        private bool ReadyToShoot { get; set; }
        public bool Shooting { get; set; }
        private float lastShot = 0;
        private float shotTrigger = 0.1f;

        private void UpdateCanon(TimeSpan ElapsedTime)
        {
            lastShot -= (float)ElapsedTime.TotalSeconds;
            if (lastShot < -shotTrigger)
            {
                lastShot = 0;
                ReadyToShoot = true;
            }
        }

        public Spaceship()
        {
            ThrustFlame = new ThrustFlame();
            Id = Data.Spaceship;
            lastShot = -shotTrigger;
        }

        public override IEnumerable<Object3D> Update(DefaultEnvironment Environment, TimeSpan ElapsedTime)
        {
            UpdateCanon(ElapsedTime);

            if (ReadyToShoot && Shooting)
            {
                var dst = (Position - Environment.ActiveCamera.Position).Length();
                Environment.Sounds[Data.LaserSound].Play(1 / (1 + dst / 5000), 0, 0);
                var bulletDirection = Vector3.Normalize(Vector3.Transform(Vector3.Forward, Orientation));
                var bullet = new Bullet(Position, bulletDirection, Speed * 50 + 1000);
                bullet.Position += (bullet.Boundary.Radius + Boundary.Radius) * 75 * bulletDirection;
                yield return bullet;

                ReadyToShoot = false;
            }
            Shooting = false;

            foreach (var thrustParticle in GenerateThrust(Environment, ElapsedTime))
            {
                yield return thrustParticle;
            }

            Speed += forwardAcceleration * (float)ElapsedTime.TotalSeconds;
            if (targetRotation > rotation)
            {
                rotation += rotationSpeed * (float)ElapsedTime.TotalSeconds;
                if (rotation >= targetRotation)
                {
                    rotation = targetRotation;
                }
            }
            else if (targetRotation < rotation)
            {
                rotation -= rotationSpeed * (float)ElapsedTime.TotalSeconds;
                if (rotation <= targetRotation)
                {
                    rotation = targetRotation;
                }
            }
            if (rotation > maxRotation)
            {
                rotation = maxRotation;
            }
            if (rotation < -maxRotation)
            {
                rotation = -maxRotation;
            }

            Orientation *= Rotation;
            var Direction = Vector3.Transform(Vector3.Forward * Speed, Orientation);
            var Up = Vector3.Transform(Vector3.Up, Orientation);
            Position += Direction * (ElapsedTime == TimeSpan.Zero ? 0 : 1);

            if (Speed > 0)
            {
                ThrustFlame.UpdateThrust(Position, Direction, Up, ElapsedTime, Environment);
            }
            else
            {
                if (ThrustFlame != null)
                {
                    ThrustFlame.DontThrust();
                }
            }
            yield break;
        }

        public override void TurnAngle(float Angle)
        {
            targetRotation = Angle;
        }

        public override void AccelerateAmount(float Amount)
        {
            forwardAcceleration = Amount;
        }

        public override void Shoot()
        {
            Shooting = true;
        }

        private IEnumerable<Explosion> GenerateThrust(DefaultEnvironment Environment, TimeSpan ElapsedTime)
        {
            if (ThrustFlame != null)
            {
                var LifeTimes = new float[]
                {
                    0.1f, 0.1f, 0.1f, 0.1f,
                    0.1f, 0.1f, 0.1f, 0.1f,
                    0.2f, 0.2f, 0.3f, 0.3f,
                    1.5f
                };
                while (ThrustFlame.Flames.Count > 0)
                {
                    var position = ThrustFlame.Flames.Dequeue();
                    var lifeTime = LifeTimes[Environment.Random.Next(LifeTimes.Length)];
                    yield return new Explosion(Data.Energyball)
                    {
                        Position = position.Position,
                        EndOfLife = lifeTime,
                        MinSize = 1.00f * ((0.1f / lifeTime) * 0.2f + 0.8f),
                        MaxSize = 0.015f,
                        StartSpin = 20 * (float)(Environment.Random.NextDouble() * 10 - 5),
                        Spin = 200
                    };
                }
            }
        }

        public override IEnumerable<Explosion> Die(DefaultEnvironment Environment, Vector3 CollisionPoint)
        {
            if (Alive == true)
            {
                var dst = (Position - Environment.ActiveCamera.Position).Length();
                Environment.Sounds[Data.ExplosionSound].Play(1 / (1 + dst / 5000), 0, 0);
            }
            Alive = false;
            yield return new Explosion(Data.Fireball)
            {
                Position = CollisionPoint,
                EndOfLife = 5,
                MinSize = 1,
                MaxSize = 10,
                Spin = (float)Environment.Random.NextDouble() * 2 - 1
            };
            for (int i = 0; i < 20; i++)
            {
                var position = new Vector3();
                do
                {
                    position.X = (float)Environment.Random.NextDouble() * 2f - 1f;
                    position.Y = (float)Environment.Random.NextDouble() * 2f - 1f;
                    position.Z = (float)Environment.Random.NextDouble() * 2f - 1f;
                } while (position.LengthSquared() > 1);
                var distance = position.Length();
                position *= Boundary.Radius * 2;
                position += Position;

                yield return new Explosion(Data.Fireball)
                {
                    EndOfLife = (1 - distance) * 10 + 2.5f,
                    MaxSize = (1 - distance) * 15 + 5,
                    MinSize = (1 - distance) * 3 + 1.25f,
                    Position = position,
                    Spin = (float)Environment.Random.NextDouble() * 2 - 1
                };
            }
        }
    }
}
