using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Xna.Framework;

namespace Conesoft.Game
{
    public abstract class Camera : Object3D
    {
        public abstract Vector3 Target { get; set; }
        public Vector3 Up { get; set; }
        public float FieldOFView { get; set; }
        public float NearCutOff { get; set; }
        public float FarCutOff { get; set; }
    }
}
