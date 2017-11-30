using Conesoft.Engine.Accelerometer;
using Conesoft.Engine.Level;
using Conesoft.Engine.NavigationService;
using Conesoft.Engine.Resources;
using Conesoft.Engine.Timer;
using System;

namespace Conesoft.Game
{
    interface IGame
    {
        INavigationService<ILevel> NavigationService { get; }
        IAccelerometer Accelerometer { get; }
        IResources Resources { get; }
        void Run();
        void Pause();
    }

    namespace Implementation
    {
        class Game : IGame
        {
            public INavigationService<ILevel> NavigationService { get; private set; }
            public IAccelerometer Accelerometer { get; private set; }
            public IResources Resources { get; private set; }

            private ITimer Timer { get; set; }

            public Game(INavigationService<ILevel> navigationService, IAccelerometer accelerometer, IResources resources, ITimer timer)
            {
                NavigationService = navigationService;
                Accelerometer = accelerometer;
                Resources = resources;
                Timer = timer;

                Timer.OnUpdate += this.Update;
                Timer.OnDraw += this.Draw;
            }

            private void Update(TimeSpan timeSpan)
            {
                var current = NavigationService.Current;
                if (current != null)
                {
                    current.Update(timeSpan);
                }
            }

            private void Draw()
            {
                var current = NavigationService.Current;
                if (current != null)
                {
                    current.Draw();
                }
            }

            public void Run()
            {
                Timer.Run();
            }

            public void Pause()
            {
                Timer.Pause();
            }
        }
    }
}
