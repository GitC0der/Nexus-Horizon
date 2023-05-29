using System;
using DefaultNamespace.TextureGeneration;
using Painting;
using UnityEngine;

namespace DefaultNamespace
{
    public class TextureGenerator : MonoBehaviour
    {
        private void Start()
        {
            WaveFunctionCollapse wfc = new(TextureInputs.Table, 64, 64, new Position2(30, 30), '-', 6);
            while (!wfc.IsDone())
            {
                wfc.GenerateNextSlot();
            }
            char[][] table = wfc.GetOutput();

            string outputPath = "./Assets/Textures/table.png";

            int width = table[0].Length;
            int height = table.Length;

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    char c = table[y][x];
                    Color32 color = UnityEngine.Color.blue;
                    if (c == '-')
                    {
                        color = new UnityEngine.Color(0.694f, 0.694f, 0.694f, 1f);
                        texture.SetPixel(x, y, color);
                    }
                    else if (c == 'W')
                    {
                        color = new UnityEngine.Color(0.9f, 0.9f, 0.9f, 1f);
                        texture.SetPixel(x, y, color);
                    }
                    else
                    {
                        color = new UnityEngine.Color(0.694f, 0.694f, 0.694f, 1f);
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
