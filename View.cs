using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

namespace Vershinina_Tomogram_vizualizer
{
    internal class View
    {
        private int VBOtexture;    // Идентификатор текстуры в OpenGL

        private Bitmap textureImage; // Bitmap для хранения текстуры

        public void SetupView(int width, int height)
        {
            GL.ShadeModel(ShadingModel.Smooth); // Включает интерполяцию цветов, плавное затенение между вершинами.
            GL.MatrixMode(MatrixMode.Projection); // Работа с матрицей проекции
            GL.LoadIdentity(); // Сброс матрицы
            GL.Ortho(0, Bin.X, 0, Bin.Y, -1, 1); // Ортографическая проекция, настраивает ортографическую проекцию, где томограмма точно заполняет окно.
            GL.Viewport(0, 0, width, height); // Размер области вывода,связывает проекцию с размерами GLControl.
        }
        public int Clamp(int value, int min, int max) //метод, ограничивыющий зону действия фильтра
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        private Color TransferFunction(short value, int min, int width)
        {
            int newVal = Clamp((value - min) * 255 / width, 0, 255);
            return Color.FromArgb(255, newVal, newVal, newVal);
        }
        public void DrawQuads(int layerNumber, int min, int width)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Begin(PrimitiveType.Quads);

            for (int x = 0; x < Bin.X - 1; x++)
            {
                for (int y = 0; y < Bin.Y - 1; y++)
                {
                    // Вершины квада с применением TransferFunction
                    short val1 = Bin.array[x + y * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(val1, min, width));
                    GL.Vertex2(x, y);

                    short val2 = Bin.array[x + (y + 1) * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(val2, min, width));
                    GL.Vertex2(x, y + 1);

                    short val3 = Bin.array[x + 1 + (y + 1) * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(val3, min, width));
                    GL.Vertex2(x + 1, y + 1);

                    short val4 = Bin.array[x + 1 + y * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(val4, min, width));
                    GL.Vertex2(x + 1, y);
                }
            }
            GL.End();
        }

        public void TriangleStrip(int layerNumber, int min, int width)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Begin(PrimitiveType.TriangleStrip);

            for (int y = 0; y < Bin.Y; y++)
            {
                // Меняем направление для каждой второй строки
                bool reverse = (y % 2 == 1);
                //если y-нечетное, то идет в обратном направлении
                //если y-четное, то идет в прямом направлении
                int start;
                int end;
                int step;
                if (reverse)
                {
                    //обратное направление
                    start = Bin.X - 1;
                    end = -1;
                    step = -1;
                }
                else
                {
                    //прямое напраление
                    start = 0;
                    end = Bin.X;
                    step = 1;
                }

                    for (int x = start; x != end; x += step)
                    {
                        // Верхняя вершина
                        short valTop = Bin.array[x + y * Bin.X + layerNumber * Bin.X * Bin.Y];
                        GL.Color3(TransferFunction(valTop, min, width));
                        GL.Vertex2(x, y);

                        // Нижняя вершина (если не последняя строка)
                        if (y < Bin.Y - 1)
                        {
                            short valBottom = Bin.array[x + (y + 1) * Bin.X + layerNumber * Bin.X * Bin.Y];
                            GL.Color3(TransferFunction(valBottom, min, width));
                            GL.Vertex2(x, y + 1);
                        }
                    }

                // Возвращаемся в начало строки для зигзага
                if (y < Bin.Y - 1)
                {
                    int x;
                    if (reverse)
                    {
                        x = 1;
                    }
                    else 
                    { 
                        x = Bin.X - 1; 
                    }
                        short val = Bin.array[x + (y + 1) * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(val, min, width));
                    GL.Vertex2(x, y + 1);
                }
            }

            GL.End();
        }

        public void DrawQuadStrip(int layerNumber,int min, int width)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Begin(PrimitiveType.QuadStrip);

            for (int x_coord = 0; x_coord < Bin.X - 2; x_coord++)
            {
                short value;
                // 1 vertex
                value = Bin.array[x_coord + layerNumber * Bin.X * Bin.Y];
                GL.Color3(TransferFunction(value, min, width));
                GL.Vertex2(x_coord, 0);

                // 2 vertex
                value = Bin.array[x_coord + 1 + layerNumber * Bin.X * Bin.Y];
                GL.Color3(TransferFunction(value, min, width));
                GL.Vertex2(x_coord + 1, 0);

                for (int y_coord = 0; y_coord < Bin.Y - 1; y_coord++)
                {
                    // 1 next vertex
                    value = Bin.array[x_coord + (y_coord + 1) * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value, min, width));
                    GL.Vertex2(x_coord, y_coord + 1);

                    // 2 next vertex
                    value = Bin.array[x_coord + 1 + (y_coord + 1) * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value, min, width));
                    GL.Vertex2(x_coord + 1, y_coord + 1);
                }
                x_coord++;
                for (int y_coord = Bin.Y - 1; y_coord > 1; y_coord--)
                {
                    // 1 next vertex
                    value = Bin.array[x_coord + y_coord * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value, min, width));
                    GL.Vertex2(x_coord, y_coord);

                    // 2 next vertex
                    value = Bin.array[x_coord + 1 + y_coord * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value, min, width));
                    GL.Vertex2(x_coord + 1, y_coord);
                }
            }
            GL.End();
        }
        public void Load2DTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, VBOtexture);
            BitmapData data = textureImage.LockBits(
                new System.Drawing.Rectangle(0, 0, textureImage.Width, textureImage.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte, data.Scan0);
            textureImage.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);
            ErrorCode Er = GL.GetError();
            string str = Er.ToString();
        }
        public void GenerateTextureImage(int layerNumber, int min, int width)
        {
            textureImage = new Bitmap(Bin.X, Bin.Y);
            for (int i = 0; i < Bin.X; i++)
            {
                for (int j = 0; j < Bin.Y; j++)
                {
                    int pixelNumber = i + j * Bin.X + layerNumber * Bin.X * Bin.Y;
                    textureImage.SetPixel(i, j, TransferFunction(Bin.array[pixelNumber], min, width));
                }
            }
        }
        public void DrawTexture()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, VBOtexture);

            GL.Begin(PrimitiveType.Quads);
            GL.Color3(Color.White);
            GL.TexCoord2(0f, 0f);
            GL.Vertex2(0, 0);
            GL.TexCoord2(0f, 1f);
            GL.Vertex2(0, Bin.Y);
            GL.TexCoord2(1f, 1f);
            GL.Vertex2(Bin.X, Bin.Y);
            GL.TexCoord2(1f, 0f);
            GL.Vertex2(Bin.X, 0);
            GL.End();
            GL.Disable(EnableCap.Texture2D);
        }
    }
}
