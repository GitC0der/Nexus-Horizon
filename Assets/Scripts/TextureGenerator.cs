using System;
using Painting;
using UnityEngine;

namespace DefaultNamespace
{
    public class TextureGenerator: MonoBehaviour
    {
        private void Start() {
            WaveFunctionCollapse wfc = new(WaveFunctionCollapse.Roof1, 32, 32, new Position2(0, 0), 'B');
            while (!wfc.IsDone())
            {
                wfc.GenerateNextSlot();
            }
            char[][] table = wfc.GetOutput();

            string outputPath = "./Assets/Textures/texture.png";

            int width = table[0].Length;
            int height = table.Length;

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    char c = table[y][x];
                    Color32 color = UnityEngine.Color.blue;
                    if (c == 'B')
                    {
                        color = UnityEngine.Color.yellow;
                        texture.SetPixel(x, y, color);
                    }
                    else if (c == 'S')
                    {
                        color = UnityEngine.Color.green;
                        texture.SetPixel(x, y, color);
                    } else if (c == 'C') {
                        color = UnityEngine.Color.cyan;
                        texture.SetPixel(x, y, color);
                    } else if (c == '-') {
                        color = UnityEngine.Color.magenta;
                        texture.SetPixel(x, y, color);
                    }
                    else
                    {
                        color = UnityEngine.Color.red;
                        texture.SetPixel(x, y, color);

                    }
                }
            }

            byte[] bytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(outputPath, bytes);
            Debug.Log("Texture generated successfully!");
        }
    }
}