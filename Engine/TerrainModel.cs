using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Conesoft.Engine
{
    public class TerrainModel
    {
        public Vector3 Position { get; set; }
        public Vector3 Size { get; set; }

        public int DataWidth { get; private set; }
        public byte[] HeightData { get; private set; }
        public VertexPositionColorTexture[] Grid { get; private set; }
        public short[] Indicees { get; private set; }

        public TerrainModel()
        {
            DataWidth = 0;
            HeightData = null;
        }

        public void LoadFromTexture2D(Texture2D TerrainTexture, Conesoft.Game.DefaultEnvironment Environment)
        {
            if (TerrainTexture.Width != TerrainTexture.Height)
            {
                throw new Exception("Terrain has to be Square in Size");
            }
            DataWidth = TerrainTexture.Width;
            HeightData = new byte[DataWidth * DataWidth];

            var colorData = new Color[DataWidth * DataWidth];
            TerrainTexture.GetData(colorData);

            foreach (var index in Enumerable.Range(0, DataWidth * DataWidth))
            {
                HeightData[index] = colorData[index].R;
            }

            Grid = new VertexPositionColorTexture[DataWidth * DataWidth];
            foreach (var y in Enumerable.Range(0, DataWidth))
            {
                foreach (var x in Enumerable.Range(0, DataWidth))
                {
                    var Height = HeightData[x + DataWidth * y];
                    var Point = new Vector3((float)x / (float)DataWidth, (float)Height / 256f, (float)y / (float)DataWidth);
                    Point = 2 * Point - new Vector3(1, 1, 1);

                    Point = Point * Size / 2 + Position;

                    var halfWidthOfSmallerStepSize = Math.Min(Size.X, Size.Z) / (float)DataWidth / 1.414f;

                    Point = Point + Environment.RandomPointInUnitSphere() * halfWidthOfSmallerStepSize;

                    var c = Height / 255f;
                    var color = new Color();
                    if (c < 0.02f)
                    {
                        color = Color.DarkBlue;
                    }
                    if (x > 0 && x < DataWidth - 1 && y > 0 && y < DataWidth - 1)
                    {
                        var dx = (HeightData[x - 1 + DataWidth * y] - HeightData[x + 1 + DataWidth * y]) / 256f;
                        var dy = (HeightData[x + DataWidth * (y - 1)] - HeightData[x + DataWidth * (y + 1)]) / 256f;

                        var dxVector = new Vector3(Size.X * 2 / DataWidth, dx * Size.Y, 0);
                        var dyVector = new Vector3(0, dy * Size.Y, Size.Z * 2 / DataWidth);
                        var normal = Vector3.Cross(dyVector, dxVector);
                        normal.Normalize();
                        var light = new Vector3(1, 1, 0);
                        light.Normalize();
                        var shade = Vector3.Dot(normal, light);
                        var snowColor = new Vector3(1, 1, 1);
                        var grassColor = new Vector3(0.1f, 0.4f, 0.01f);
                        {
                            c -= 0.7f;
                            c *= 2f;
                            if (c < 0) c = 0;
                            else c *= 2;
                            if (c > 1) c = 1;
                            color = new Color(shade * MathHelper.Lerp(grassColor.X, snowColor.X, c), shade * MathHelper.Lerp(grassColor.Y, snowColor.Y, c), shade * MathHelper.Lerp(grassColor.Z, snowColor.Z, c));
                        }
                        var h = Height / 255f;
                        if (h < 0.03f)
                        {
                            var blend = h;
                            blend = blend / 0.03f;
                            var clr1 = new Color(0, 0, 0.2f).ToVector3();
                            var clr2 = color.ToVector3();
                            var clr = (1 - blend) * clr1 + blend * clr2;
                            color = new Color(clr.X, clr.Y, clr.Z);
                        }
                    }
                    else
                    {
                        color = new Color(0.2f, 0.3f, 0.8f);
                    }
                    color *= 1.3f;
                    Grid[x + DataWidth * y] = new VertexPositionColorTexture(Point, color, new Vector2(0.5f * Point.X / Size.X + 0.5f, 0.5f * Point.Z / Size.Z + 0.5f) * 64);
                }
            }

            Indicees = new short[DataWidth * 2];
            foreach (var i in Enumerable.Range(0, Indicees.Length))
            {
                if (i % 2 == 0)
                {
                    Indicees[i] = (short)(i + DataWidth);
                }
                else
                {
                    Indicees[i] = (short)(i - 1);
                }
            }
        }

        public void Draw(BasicEffect basicEffect, Texture2D texture = null)
        {
            basicEffect.LightingEnabled = false;
            basicEffect.TextureEnabled = texture != null;
            basicEffect.VertexColorEnabled = true;
            basicEffect.Texture = texture;

            basicEffect.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;

            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                foreach (var i in Enumerable.Range(0, DataWidth - 1))
                {
                    basicEffect.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, Grid, i * DataWidth, DataWidth * 2, Indicees, 0, DataWidth - 2);
                }
            }
        }
    }
}
