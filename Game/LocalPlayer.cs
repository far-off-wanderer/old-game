using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework;

namespace Conesoft.Game
{
    public class LocalPlayer : Player
    {
        public static float deadZone = (float)Math.Sin(MathHelper.ToRadians(1));

        public override void UpdateThinking(TimeSpan timeSpan, DefaultEnvironment environment)
        {
            float turnAngle = 0;
            float acceleration = 0;
            bool shoot = false;
            foreach (var touchPoint in TouchPanel.GetState())
            {
                if (touchPoint.Position.X < environment.ScreenSize.Width / 3)
                {
                    turnAngle += (float)Math.PI / 2;
                }
                else if (touchPoint.Position.X > 2 * environment.ScreenSize.Width / 3)
                {
                    turnAngle -= (float)Math.PI / 2;
                }
                else
                {
                    shoot = true;
                    //if (touchPoint.Position.Y < playerEnvironment.ScreenSize.Height / 2)
                    //{
                    //    acceleration += 1;
                    //}
                    //else
                    //{
                    //    acceleration -= 1;
                    //}
                }
            }
            var spaceShip = ControlledObject as Spaceship;
            if (shoot)
            {
                spaceShip.Shoot();
            }
            spaceShip.TurnAngle(turnAngle + (environment.Flipped ? -1 : 1) * Math.Sign(environment.Acceleration.Y) *MathHelper.Clamp((Math.Abs(environment.Acceleration.Y) - deadZone) / (1 - deadZone), 0, 1));
            spaceShip.AccelerateAmount(acceleration/* - playerEnvironment.Acceleration.Z*/);
        }
    }
}
