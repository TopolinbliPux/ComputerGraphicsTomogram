using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;

namespace Vershinina_Tomogram_vizualizer
{
    public partial class Form1 : Form
    {
        private Bin bin = new Bin();
        private View view = new View();
        private bool loaded = false;
        private int currentLayer = 0;
        private bool needReload = true;
        //для FPS
        private int FrameCount; //счётчик кадров, увеличивается на 1 при каждой отрисовке
        private DateTime NextFPSUpdate = DateTime.Now.AddSeconds(1);//время следующего обновления показателя FPS (инициализируется на 1 секунду вперёд)

        private int min = 0;
        private int width = 255;

        private bool useTextureRendering = true;
        public Form1()
        {
            InitializeComponent();
            glControl1.Paint += glControl1_Paint;
            this.Load += Form1_Load;
            trackBar1.Maximum = 10;

            trackBar2.Minimum = 0;
            trackBar2.Maximum = 255;
            trackBar2.Value = 0;
            trackBar2.TickFrequency = 1;

            // Настройка трекбара для width (ширина окна)
            trackBar3.Minimum = 0;
            trackBar3.Maximum = 255;
            trackBar3.Value = 0;
            trackBar3.TickFrequency = 1;

            trackBar1.Scroll += trackBar1_Scroll;
            trackBar2.Scroll += trackBar2_Scroll;
            trackBar3.Scroll += trackBar3_Scroll;
        }


        //Проверяет, наступило ли время обновить показатель FPS (прошла ли 1 секунда)
        //Если да:
            //Обновляет заголовок окна с текущим FPS(количество кадров за секунду)
            //Устанавливает следующее время обновления(+1 секунда)
            //Сбрасывает счётчик кадров
        //Увеличивает счётчик кадров(вызывается при каждой отрисовке)
        private void displayFPS()
        {
            if (DateTime.Now >= NextFPSUpdate)
            {
                this.Text = String.Format("CT Visualizer (fps={0})", FrameCount);
                NextFPSUpdate = DateTime.Now.AddSeconds(1);
                FrameCount = 0;
            }
            FrameCount++;
        }

        //Функция Application_Idle проверяет, занято ли OpenGL окно работой, если нет,
        //то вызывается функция Invalidate, которая заставляет кадр рендериться заново.
        private void Application_Idle(object sender, EventArgs e)
        {
            while (glControl1.IsIdle)
            {
                displayFPS();
                glControl1.Invalidate();
            }
        }
        //подключили Application_Idle на автоматическое выполнение.
        private void Form1_Load(object sender, EventArgs e)
        {
            Application.Idle += Application_Idle;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string str = dialog.FileName;
                bin.readBin(str);
                view.SetupView(glControl1.Width, glControl1.Height);
                loaded = true;

                trackBar1.Maximum = Bin.Z - 1;
                trackBar1.Value = Bin.Z / 2;
                currentLayer = trackBar1.Value;

                glControl1.Invalidate();
            }
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (loaded)
            {
                if (useTextureRendering)
                {
                    // Всегда пересоздаём текстуру при изменении параметров
                    view.GenerateTextureImage(currentLayer, min, width);
                    view.Load2DTexture();
                    view.DrawTexture();
                }
                else if (radioButton3.Checked)
                {
                    view.DrawQuadStrip(currentLayer, min, width);
                }
                else if (radioButton2.Checked)
                {
                    view.DrawQuads(currentLayer, min, width);
                }
                else
                {
                    view.TriangleStrip(currentLayer, min, width);
                }
                    glControl1.SwapBuffers();
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            currentLayer = trackBar1.Value;
            needReload = true;
            if (loaded)
            {
                glControl1.Invalidate(); // Перерисовываем
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                useTextureRendering = true;
                needReload = true;
                if (loaded) glControl1.Invalidate();
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                useTextureRendering = false;
                if (loaded) glControl1.Invalidate();
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                useTextureRendering = false;
                if (loaded) glControl1.Invalidate();
            }
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            //if (!loaded) return;

            //int newMin = trackBar2.Value;
            //int newWidth = trackBar3.Value;

            //// Ограничение: newMin + newWidth <= 255
            //if (newMin + newWidth > 255)
            //{
            //    newWidth = 255 - newMin;
            //    trackBar3.Value = newWidth;
            //}

            //min = newMin;
            //width = newWidth;
            //needReload = true;
            //glControl1.Invalidate();
            min = trackBar2.Value; // Обновление минимального значения
            if (loaded)
            {
                glControl1.Invalidate(); // Перерисовка при изменении
            }
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            width = trackBar3.Value; // Обновление ширины
            if (loaded)
            {
                glControl1.Invalidate(); // Перерисовка при изменении
            }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
            {
                useTextureRendering = false;
                if (loaded) glControl1.Invalidate();
            }
        }
    }
}
