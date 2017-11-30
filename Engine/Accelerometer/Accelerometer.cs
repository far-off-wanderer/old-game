using Microsoft.Xna.Framework;

namespace Conesoft.Engine.Accelerometer
{
    interface IAccelerometer
    {
        Vector3 Acceleration { get; }
    }

    namespace Implementation
    {
        namespace Xna
        {
            class Accelerometer : IAccelerometer
            {
                private Microsoft.Devices.Sensors.Accelerometer acc;

                public Accelerometer()
                {
                    acc = new Microsoft.Devices.Sensors.Accelerometer();
                    acc.Start();
                }

                public Vector3 Acceleration
                {
                    get { return acc.CurrentValue.Acceleration; }
                }
            }
        }
    }
}
