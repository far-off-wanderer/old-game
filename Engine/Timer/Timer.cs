using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conesoft.Engine.Timer
{
    delegate void UpdateHandler(TimeSpan timeSpan);
    delegate void DrawHandler();

    interface ITimer
    {
        void Run();
        void Pause();

        event UpdateHandler OnUpdate;
        event DrawHandler OnDraw;
    }

    namespace Implementation
    {
        namespace Xna
        {
            using Microsoft.Xna.Framework;

            class Timer : ITimer
            {
                GameTimer gameTimer;

                public Timer()
                {
                    gameTimer = new GameTimer();
                    gameTimer.Update += gameTimer_Update;
                    gameTimer.Draw += gameTimer_Draw;
                }

                void gameTimer_Draw(object sender, GameTimerEventArgs e)
                {
                    if (OnDraw != null)
                    {
                        OnDraw();
                    }
                }

                void gameTimer_Update(object sender, GameTimerEventArgs e)
                {
                    if (OnUpdate != null)
                    {
                        OnUpdate(e.ElapsedTime);
                    }
                }

                public event UpdateHandler OnUpdate;

                public event DrawHandler OnDraw;

                public void Run()
                {
                    gameTimer.Start();
                }

                public void Pause()
                {
                    gameTimer.Stop();
                }
            }
        }
    }
}
