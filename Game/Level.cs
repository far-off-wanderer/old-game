﻿using System;
using System.Linq;
using System.Collections.Generic;
using Conesoft.Engine;
using Microsoft.Xna.Framework;

namespace Conesoft.Game
{
    using Conesoft;
    public class DefaultLevel
    {
        public List<Player> Players { get; set; }
        public List<Object3D> Objects3D { get; set; }
        public Terrain Terrain { get; set; }
        public Skybox Skybox { get; set; }
        public Camera Camera { get; set; }
        public DefaultEnvironment Environment { get; set; }

        public DefaultLevel(DefaultEnvironment environment)
        {
            Environment = environment;

            Players = new List<Player>();
            Objects3D = new List<Object3D>();
            var random = new System.Random();
            int factor = 3;
            Objects3D.Add(new Spaceship()
            {
                Id = Data.Ship,
                Position = Vector3.Zero,
                Orientation = Quaternion.Identity,
                Speed = 150,
                Boundary = environment.ModelBoundaries[Data.Ship],
                ThrustFlame = new ThrustFlame()
                {
                    ThrustBackshift = 150
                }
            });
            for (int y = -factor; y <= factor; y++)
            {
                for (int x = -factor; x <= factor; x++)
                {
                    if (y != 0 || x != 0)
                    {
                        var ids = new string[] { Data.Ship, Data.Drone };
                        var id = ids[environment.Random.Next(0, ids.Length)];
                        var spaceship = new Spaceship()
                        {
                            Id = id,
                            Position = 6400 * (Vector3.Forward * y + Vector3.Right * x) * (float)factor * 2 / 5,
                            Orientation = Quaternion.CreateFromAxisAngle(Vector3.Up, x + y),
                            Speed = id == ids[0] ? 150 : 0,
                            Boundary = environment.ModelBoundaries[id],
                            ThrustFlame = id == ids[1] ? null : new ThrustFlame()
                            {
                                ThrustBackshift = 150
                            }
                        };
                        Players.Add(new ComputerPlayer()
                        {
                            ControlledObject = spaceship
                        });
                        Objects3D.Add(spaceship);
                    }
                }
            }
            Terrain = new Terrain();
            Skybox = new Skybox()
            {
                Color = new Color(0.2f, 0.3f, 0.8f)
            };
            if (Objects3D.Count > 0)
            {
                Camera = new SpaceshipFollowingCamera()
                {
                    Orientation = Quaternion.Identity,
                    Up = Vector3.Up,
                    FieldOFView = (float)Math.PI / 3,
                    NearCutOff = 100,
                    FarCutOff = 80000,
                    Ship = Objects3D.OfType<Spaceship>().First()
                };
            }
            else
            {
                Camera = new FixedCamera()
                {
                    Orientation = Quaternion.Identity,
                    Up = Vector3.Up,
                    FieldOFView = (float)Math.PI / 4,
                    NearCutOff = 100,
                    FarCutOff = 80000,
                    Position = Vector3.Backward * 2,
                    Target = Vector3.Zero
                };
            }
        }

        public void UpdateScene(TimeSpan ElapsedTime)
        {
            var newObjects = new List<Object3D>();
            foreach (var objects3d in Objects3D)
            {
                newObjects.AddRange(objects3d.Update(Environment, ElapsedTime));
            }

            foreach (var player in Players)
            {
                player.UpdateThinking(ElapsedTime, Environment);
            }

            var collidableObjects = (from object3d in Objects3D
                                     where object3d.Boundary != Object3D.EmptyBoundary
                                     select object3d).ToArray();

            if (collidableObjects.Length > 1)
            {
                for (int a = 0; a < collidableObjects.Length; a++)
                {
                    var objectA = collidableObjects[a];
                    if (objectA is Explosion)
                    {
                        break;
                    }
                    var sphereA = objectA.Boundary;
                    if (sphereA != Object3D.EmptyBoundary)
                    {
                        sphereA.Center = objectA.Position; // should be += but can't, right now. should be done with proper orientation and all..

                        sphereA.Radius *= 50;
                        for (int b = a + 1; b < collidableObjects.Length; b++)
                        {
                            var objectB = collidableObjects[b];
                            var sphereB = objectB.Boundary;
                            if (sphereB != Object3D.EmptyBoundary)
                            {
                                sphereB.Center = objectB.Position; // should be += but can't, right now. should be done with proper orientation and all..
                                sphereB.Radius *= 50;

                                if (sphereA.Intersects(sphereB))
                                {
                                    var collisionPoint = (objectA.Position + objectB.Position) / 2;

                                    newObjects.AddRange(objectA.Die(Environment, collisionPoint).Cast<Object3D>());
                                    newObjects.AddRange(objectB.Die(Environment, collisionPoint).Cast<Object3D>());
                                }
                            }
                        }
                    }
                }
            }
            foreach (var newObject in newObjects)
            {
                newObject.Update(Environment, ElapsedTime);
            }
            Objects3D.AddRange(newObjects);
            foreach (var deadObject3d in (from object3d in Objects3D where object3d.Alive == false select object3d).ToArray())
            {
                Objects3D.Remove(deadObject3d);
            }
        }
    }
}
