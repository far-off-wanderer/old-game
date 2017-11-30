using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;

namespace Conesoft
{
    using Engine;
    using Game;
    using Game.Level;

    public sealed class Data
    {
        public static readonly string Ship = "vessels/schiff V37";
        public static readonly string ShipDetails = Ship + " details";
        public static readonly string Drone = "vessels/sonde V29";
        public static readonly string DroneDetails = Drone + " details";
        public static readonly string Spaceship = "vic viper small junctioned";
        public static readonly string Fireball = "Fireball";
        public static readonly string Energyball = "Energyball";
        public static readonly string Bullet = "Bullet";
        public static readonly string Grass = "grass A bump";
        public static readonly string TutorialOverlay = "overlays/tutorial";
        public static readonly string GameWonOverlay = "overlays/gamewon";
        public static readonly string GameOverOverlay = "overlays/gameover";
        public static readonly string BlackBackground = "Black Background";
        public static readonly string Landscape = "Landscape";
        public static readonly string LandscapeGround = "Floor";
        public static readonly string ExplosionSound = "8bit explosion";
        public static readonly string GameOverSound = "8bit gameover";
        public static readonly string GoSound = "8bit go";
        public static readonly string GoodSound = "8bit good";
        public static readonly string LaserSound = "8bit laser";
        public static readonly string Font = "Font";
        public static readonly string SmallFont = "SmallFont";
        public static readonly string Username = "Username";
    }

    public partial class GamePage : PhoneApplicationPage
    {
        IGame game;
        GameTimer timer;
        SpriteBatch spriteBatch;
        GraphicsDevice graphics;
        TimeSpan? startTime;
        float fadeInTime;
        float fadeIn;
        float? fadeOut;
        TimeSpan sinceStart;
        bool gameOverTouch;
        bool dead;
        bool won;
        bool started;
        LocalPlayer localPlayer;

        DefaultLevel level;

        public GamePage()
        {
            InitializeComponent();

            game = TinyIoC.TinyIoCContainer.Current.Resolve<IGame>();

            // Create a timer for this page
            timer = new GameTimer();
            timer.UpdateInterval = TimeSpan.FromTicks(333333);
            timer.Update += OnUpdate;
            timer.Draw += OnDraw;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set the sharing mode of the graphics device to turn on XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(true);

            graphics = SharedGraphicsDeviceManager.Current.GraphicsDevice;

            game.Run();

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(SharedGraphicsDeviceManager.Current.GraphicsDevice);

            var environment = new Conesoft.Game.DefaultEnvironment()
            {
                Random = new Random(),
                ModelBoundaries = new Dictionary<string,BoundingSphere>()
            };

            game.Resources.Load((to, from) =>
            {
                to.Models.Add(from.Load<Model>(Data.Ship, Data.Drone, Data.Spaceship));
                to.Sprites.Add(from.Load<Texture2D>(Data.Fireball, Data.Energyball, Data.Bullet, Data.Grass, Data.TutorialOverlay, Data.GameWonOverlay, Data.GameOverOverlay));

                var texture = new Texture2D(graphics, 1, 1);
                texture.SetData(new Color[] { Color.Black });
                to.Sprites.Add(Data.BlackBackground, texture);

                to.Sounds.Add(from.Load<SoundEffect>(Data.ExplosionSound, Data.GameOverSound, Data.GoSound, Data.GoodSound, Data.LaserSound));

                to.Fonts.Add(from.Load<SpriteFont>(Data.Font, Data.SmallFont));

                var terrainModel = new TerrainModel()
                {
                    Position = Vector3.Down * 64 * 32,
                    Size = new Vector3(1024 * 128, 256 * 32, 1024 * 128)
                };
                terrainModel.LoadFromTexture2D(from.Load<Texture2D>(Data.LandscapeGround), environment);
                to.Terrains.Add(Data.Landscape, terrainModel);

                foreach (var model in to.Models)
                {
                    var boundingSphere = default(BoundingSphere);
                    foreach (var mesh in model.Value.Meshes)
                    {
                        boundingSphere = BoundingSphere.CreateMerged(boundingSphere, mesh.BoundingSphere);
                    }
                    environment.ModelBoundaries[model.Key] = boundingSphere;
                }
            });

            environment.Sounds = game.Resources.Sounds;

            // Start the timer
            timer.Start();

            App.Current.Appl.NavigationService.NavigateTo<LastManStandingLevel>();

            level = new DefaultLevel(environment);

            var spaceShips = level.Objects3D.OfType<Spaceship>();
            if (spaceShips.Count() > 0)
            {
                localPlayer = new LocalPlayer()
                {
                    ControlledObject = spaceShips.First()
                };
                level.Players.Add(localPlayer);
            }

            startTime = null;

            fadeInTime = App.Current.FirstTime ? 3 : 1;
            App.Current.FirstTime = false;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Stop the timer
            timer.Stop();

            game.Pause();

            // Set the sharing mode of the graphics device to turn off XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(false);

            base.OnNavigatedFrom(e);
        }

        private void OnUpdate(object sender, GameTimerEventArgs e)
        {
            level.Environment.Acceleration = game.Accelerometer.Acceleration;
            level.Environment.ScreenSize = new Size(ActualWidth, ActualHeight);
            level.Environment.ActiveCamera = level.Camera;
            level.Environment.Flipped = this.Orientation == PageOrientation.LandscapeRight;

            if (startTime.HasValue == false)
            {
                startTime = e.TotalTime;
            }

            sinceStart = e.TotalTime - startTime.Value;

            fadeIn = Math.Min((float)sinceStart.TotalSeconds / fadeInTime, 1);
            if (fadeIn == 1)
            {
                fadeInTime = 1;
            }

            level.UpdateScene(sinceStart.TotalSeconds > fadeInTime ? e.ElapsedTime : TimeSpan.Zero);

            if (sinceStart.TotalSeconds > fadeInTime - 0.5f)
            {
                if (started == false)
                {
                    game.Resources.Sounds[Data.GoSound].Play();
                }
                started = true;
            }

            if (level.Objects3D.Contains(localPlayer.ControlledObject) == false)
            {
                if (dead != true)
                {
                    game.Resources.Sounds[Data.GameOverSound].Play();
                }
                dead = true;
                CheckForExitClick();
                UpdateFadeout(e);
            }
            else if (level.Objects3D.OfType<Spaceship>().Count() == 1 && dead == false)
            {
                if (won != true)
                {
                    game.Resources.Sounds[Data.GoodSound].Play();
                    UpdateHighscore();
                }
                won = true;
                CheckForExitClick();
                UpdateFadeout(e);
            }
        }
        
        private void UpdateHighscore()
        {
            var highscores = TinyIoC.TinyIoCContainer.Current.Resolve<Conesoft.Game.Highscore.IHighscores>();
            highscores.Add(new Conesoft.Game.Highscore.Highscore
            {
                Name = (string)IsolatedStorageSettings.ApplicationSettings[Data.Username],
                Seconds = sinceStart.TotalSeconds.ToString()
            });
        }

        private void CheckForExitClick()
        {
            if (TouchPanel.GetState().Count == 0)
            {
                gameOverTouch = true;
            }
            if (TouchPanel.GetState().Count > 0 && gameOverTouch == true)
            {
                NavigationService.GoBack();
            }
        }

        private void UpdateFadeout(GameTimerEventArgs e)
        {

            if (fadeOut.HasValue == false)
            {
                fadeOut = 0;
            }
            else
            {
                fadeOut += (float)e.ElapsedTime.TotalSeconds / 3;
                if (fadeOut > 1)
                {
                    fadeOut = 1;
                }
            }
        }

        private void OnDraw(object sender, GameTimerEventArgs e)
        {
            var camera = new CameraModel(level.Camera, RenderSize);

            var basicEffect = new BasicEffect(graphics);

            graphics.Clear(level.Skybox.Color);

            graphics.BlendState = BlendState.Opaque;
            graphics.DepthStencilState = DepthStencilState.Default;

            basicEffect.EnableDefaultLighting();

            basicEffect.LightingEnabled = true;
            basicEffect.TextureEnabled = true;
            basicEffect.VertexColorEnabled = false;
            basicEffect.FogEnabled = true;
            basicEffect.FogColor = new Vector3(0.2f, 0.3f, 0.8f);
            basicEffect.FogStart = 10f;
            basicEffect.FogEnd = 75000f;

            basicEffect.DirectionalLight0.Enabled = true;
            basicEffect.DirectionalLight0.DiffuseColor = new Vector3(0, 0, 0);
            basicEffect.DirectionalLight0.Direction = new Vector3(0, 1, 0);
            basicEffect.DirectionalLight0.SpecularColor = new Vector3(0, 0, 0);
            graphics.RasterizerState = RasterizerState.CullClockwise;

            foreach (var spaceship in level.Objects3D.OfType<Spaceship>())
            {
                var model = game.Resources.Models[spaceship.Id];
                model.Draw(Matrix.CreateRotationY(MathHelper.ToRadians(180)) * Matrix.CreateFromQuaternion(spaceship.Orientation * spaceship.ShipLeaning) * Matrix.CreateTranslation(spaceship.Position), camera.View, camera.Projection);
            }

            basicEffect.World = Matrix.Identity;
            basicEffect.View = camera.View;
            basicEffect.Projection = camera.Projection;

            graphics.RasterizerState = RasterizerState.CullCounterClockwise;

            game.Resources.Terrains[level.Terrain.Id].Draw(basicEffect, game.Resources.Sprites[Data.Grass]);

            var bounds = graphics.Viewport.Bounds;

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            foreach (var explosion in level.Objects3D.OfType<Explosion>())
            {
                var transformed = graphics.Viewport.Project(explosion.Position, camera.Projection, camera.View, Matrix.Identity);
                var distance = (explosion.Position - level.Camera.Position).Length();
                if (transformed.Z > 0 && transformed.Z < 1 && distance > 0)
                {
                    var sprite = game.Resources.Sprites[explosion.Id];
                    var width = explosion.CurrentSize * bounds.Width * (float)sprite.Width / distance;
                    var rectangle = new Microsoft.Xna.Framework.Rectangle((int)(transformed.X), (int)(transformed.Y), (int)width, (int)width);
                    if (rectangle.Intersects(graphics.Viewport.Bounds))
                    {
                        spriteBatch.Draw(sprite, rectangle, null, new Color(2 - explosion.Age, 2 - explosion.Age, 1 - explosion.Age, 2 - explosion.Age), explosion.StartSpin + explosion.Spin * explosion.Age, new Vector2(sprite.Width / 2), SpriteEffects.None, 0);
                    }
                }
            }
            spriteBatch.End();
            spriteBatch.Begin();
            foreach (var bullet in level.Objects3D.OfType<Bullet>())
            {
                var transformed = graphics.Viewport.Project(bullet.Position, camera.Projection, camera.View, Matrix.Identity);
                var distance = (bullet.Position - level.Camera.Position).Length();
                if (transformed.Z > 0 && transformed.Z < 1 && distance > 0)
                {
                    var sprite = game.Resources.Sprites[bullet.Id];
                    var width = bullet.Boundary.Radius * 2 * bounds.Width * (float)sprite.Width / distance;
                    var rectangle = new Microsoft.Xna.Framework.Rectangle((int)(transformed.X), (int)(transformed.Y), (int)width, (int)width);
                    if (rectangle.Intersects(graphics.Viewport.Bounds))
                    {
                        spriteBatch.Draw(sprite, rectangle, null, Color.White, 0, new Vector2(sprite.Width / 2), SpriteEffects.None, 0);
                    }
                }
            }
            var enemyCount = level.Objects3D.OfType<Spaceship>().Count();
            var enemyCountText = (enemyCount > 0 ? enemyCount - 1 : 0).ToString();
            var enemyCountSize = game.Resources.Fonts[Data.Font].MeasureString(enemyCountText).X;

            var osdBlend = Color.White * (1f - MathHelper.Clamp((fadeOut.HasValue ? fadeOut.Value : 0) * 1.5f - 1, 0, 1));

            if (fadeIn < 1f)
            {
                var timer = (1 - fadeIn) * fadeInTime;
                var msg = timer < 0.5 ? "go" : Math.Ceiling(timer).ToString();
                var msgSize = game.Resources.Fonts[Data.Font].MeasureString(msg).X;
                spriteBatch.DrawString(game.Resources.Fonts[Data.Font], msg, new Vector2(graphics.Viewport.Width - msgSize - 20, 10), osdBlend);
                spriteBatch.Draw(game.Resources.Sprites[Data.TutorialOverlay], new Microsoft.Xna.Framework.Rectangle(0, 0, graphics.Viewport.Width, graphics.Viewport.Height), new Color(1f, 1f, 1f) * MathHelper.Clamp(4 * (1 - fadeIn), 0, 1));
            }
            else
            {
                spriteBatch.DrawString(game.Resources.Fonts[Data.Font], enemyCountText, new Vector2(graphics.Viewport.Width - enemyCountSize - 20, 10), osdBlend);
            }

            if (fadeOut.HasValue)
            {
                spriteBatch.Draw(game.Resources.Sprites[Data.BlackBackground], new Microsoft.Xna.Framework.Rectangle(0, 0, graphics.Viewport.Width, graphics.Viewport.Height), new Color(0, 0, 0, fadeOut.Value / 2));
                if (dead == true)
                {
                    spriteBatch.Draw(game.Resources.Sprites[Data.GameOverOverlay], new Microsoft.Xna.Framework.Rectangle(0, 0, graphics.Viewport.Width, graphics.Viewport.Height), new Color(1f, 1f, 1f) * MathHelper.Clamp(fadeOut.Value * 2.5f - 1, 0, 1));
                }
                else if (won == true)
                {
                    spriteBatch.Draw(game.Resources.Sprites[Data.GameWonOverlay], new Microsoft.Xna.Framework.Rectangle(0, 0, graphics.Viewport.Width, graphics.Viewport.Height), new Color(1f, 1f, 1f) * MathHelper.Clamp(fadeOut.Value * 2.5f - 1, 0, 1));
                }
            }
            spriteBatch.End();

        }
    }
}