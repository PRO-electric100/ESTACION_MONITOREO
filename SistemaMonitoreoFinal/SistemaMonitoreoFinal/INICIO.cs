//using Bunifu.UI.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml.Linq;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace SistemaMonitoreoFinal
{
    public partial class INICIO : Form
    {
        //-------------------------GL----------------------
        int eyeX = 100;
        int eyeY = 100;
        int eyeZ = 100;
        List<Int32> GList;
        List<Line> lines = new List<Line>();
        bool loaded = false;
        List<Dot> dots = new List<Dot>();
        List<Rectangle> rectangles = new List<Rectangle>();
        int startCuboAlgoritmo = -1, endCuboAlgoritmo;
        decimal[] baricentroCuboAlgoritmo = new decimal[3];
        int velocidadGrafica = 100;
        //----------------------GL----------------------------------
        string datos_puerto;
        System.IO.Ports.SerialPort puerto;
        double tiempo = 0.0;
        bool IsOpen = false;
        /// graficas
        static public double velocidad=0.0;
        static public double aceleracion=0.0;
        static public double altura=0.0;
        static public string presion="0.0";
        static public string Temperatura="0.0";
        static public string Orientacion_x= "0.0";
        static public string Orientacion_y= "0.0";
        // rotacion--------------
        private Bitmap image = SistemaMonitoreoFinal.Properties.Resources.cilindroSin;
        int Angle = 0;
        // rotacion-----------
        public INICIO()
        {
            InitializeComponent();
            //-----------------GL
            GList = new List<Int32>();
            GList.Add(0);
            //------------------GL
        }
        //------------GL---------------------
        private void gl_Paint(object sender, PaintEventArgs e)
        {
            if (!loaded)
                return;
            if (GList == null)
                return;

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 perspective = OpenTK.Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1F, 0.1F, 20000F);

            GL.LoadMatrix(ref perspective);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            Matrix4 lookat = Matrix4.LookAt(eyeX, eyeY, eyeZ, 0, 0, 0, 0, 1, 0);
            GL.LoadMatrix(ref lookat);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.PushMatrix();
            for (int i = 0; i < GList.Count; i++)
                GL.CallList(GList[i]);

            GL.PopMatrix();
            GL.Finish();
            gl.SwapBuffers();
        }
        //------------GL---------------------
        private void tmrRedraw_Tick_1(object sender, EventArgs e)
        {
            GList = new List<Int32>();
            GList.Add(0);
            foreach (Dot dot in dots)
            {
                GL.NewList(GList.Count, ListMode.Compile);
                GL.PointSize(5);
                GL.Begin(BeginMode.Points);
                GL.Color3(dot.color);

                decimal[] aux_dot = FactoryMatrix.xVxM(dot.matrix, dot.dot);
                GL.Vertex3(Decimal.ToDouble(aux_dot[0]), Decimal.ToDouble(aux_dot[1]), Decimal.ToDouble(aux_dot[2]));

                GL.End();
                GL.EndList();
                GList.Add(GList.Count);
            }

            foreach (Line line in lines)
            {
                decimal[] from = FactoryMatrix.xVxM(line.matrix, line.from);
                decimal[] to = FactoryMatrix.xVxM(line.matrix, line.to);

                GL.NewList(GList.Count, ListMode.Compile);
                GL.Begin(BeginMode.Lines);

                GL.LineWidth(line.width);
                GL.Color3(line.color);
                GL.Vertex3(Decimal.ToDouble(from[0]), Decimal.ToDouble(from[1]), Decimal.ToDouble(from[2]));
                GL.Vertex3(Decimal.ToDouble(to[0]), Decimal.ToDouble(to[1]), Decimal.ToDouble(to[2]));

                GL.End();
                GL.EndList();
                GList.Add(GList.Count);
            }

            foreach (Rectangle rectangle in rectangles)
            {
                GL.NewList(GList.Count, ListMode.Compile);
                GL.PointSize(5);
                GL.Begin(BeginMode.Quads);
                GL.Color3(rectangle.color);
                for (int i = 0; i < rectangle.points.Count; i++)
                {
                    decimal[] aux_dot = FactoryMatrix.xVxM(rectangle.matrix, rectangle.points[i]);
                    GL.Vertex3(Decimal.ToDouble(aux_dot[0]), Decimal.ToDouble(aux_dot[1]), Decimal.ToDouble(aux_dot[2]));
                }
                GL.End();
                GL.EndList();
                GList.Add(GList.Count);
            }


            gl_Paint(openGLControl1, null);
        }
        //---GL------------------------

        //metodo de rotacion de una imagen
        public static Bitmap RotateImage(Image image, PointF offset, float angle)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            //create a new empty bitmap to hold rotated image
            Bitmap rotatedBmp = new Bitmap(image.Width, image.Height);
            rotatedBmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //make a graphics object from the empty bitmap
            Graphics g = Graphics.FromImage(rotatedBmp);

            //Put the rotation point in the center of the image
            g.TranslateTransform(offset.X, offset.Y);

            //rotate the image
            g.RotateTransform(angle);

            //move the image back
            g.TranslateTransform(-offset.X, -offset.Y);

            //draw passed in image onto graphics object
            g.DrawImage(image, new PointF(0, 0));

            return rotatedBmp;
        }
        //--------------------------------
        private void INICIO_Load(object sender, EventArgs e)
        {
            //---------GL-------------------
            //this.gl = new OpenTK.GLControl();
            //--------------GL----------------
            //loaded = true;
            //GL.ClearColor(Color.Black);

            //generarFlechas();
            //actualizarValor();

            //---------------GL----
            //tiempo del reloj que muestra la hora
            TiempoReloj.Interval = 1000;
            TiempoReloj.Start();
            // datos del puerto
            try
            {
                // Puerto
                string[] ports = SerialPort.GetPortNames();
                PUERTOS.DataSource = ports;


                // Baudrate
                string[] rates = { "9600", "38400", "57600", "115200" };
                BAUDIOS.DataSource = rates;

            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        //comunicacion serial con los puertos
        public void serial()
        {

            try
            {
                this.puerto = new System.IO.Ports.SerialPort("" + PUERTOS.Text, Convert.ToInt32(BAUDIOS.Text), System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);

                this.puerto.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(recepcion);

            }
            catch (Exception error)
            {
                MessageBox.Show("falla en conexion");
                this.puerto = new System.IO.Ports.SerialPort("" + "0", 9600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);


            }
        }

        //metodod de resepcion de datos
        public void recepcion(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                // Thread.Sleep(1);
                datos_puerto = this.puerto.ReadLine();
                Console.WriteLine(datos_puerto);
                if (datos_puerto.StartsWith("$"))
                {
                    // formato de string recibido
                    NumberFormatInfo formatProvider = new NumberFormatInfo();
                    formatProvider.NumberDecimalSeparator = ".";
                    formatProvider.NumberGroupSeparator = ",";
                    // deteccion de dato
                    datos_puerto = datos_puerto.Remove(0, 1);
                    // separacion de datos
                    string[] arreglo = datos_puerto.Split(';');
                    //arreglo[0].Replace(".", ",");
                    //arreglo[1].Replace(".", ",");
                    //arreglo[2].Replace(".", ",");
                    // datos recibidos y transformados a tipo bouble
                    altura = Convert.ToDouble(arreglo[0], formatProvider);
                    aceleracion = Convert.ToDouble(arreglo[1], formatProvider);
                    velocidad = Convert.ToDouble(arreglo[2], formatProvider);
                    Orientacion_x = arreglo[5];
                    Orientacion_y = arreglo[6];
                    presion = arreglo[3];
                    Temperatura = arreglo[4];

                    Console.WriteLine(arreglo[0]);
                    Console.WriteLine(arreglo[1]);
                    Console.WriteLine(arreglo[2]);
                    Console.WriteLine(arreglo[3]);
                    Console.WriteLine(arreglo[4]);
                    Console.WriteLine(arreglo[5]);
                    Console.WriteLine(arreglo[6]);
                    Console.WriteLine("-----");
                    Console.WriteLine(altura);
                    Console.WriteLine(aceleracion);
                    Console.WriteLine(velocidad);

                }
                else
                {
                    altura = 0.0;
                    velocidad = 0.0;
                    aceleracion = 0.0;
                    presion = "0.0";
                    Temperatura = "0.0";
                    Orientacion_x = "0.0";
                    Orientacion_y = "0.0";
                }

                //this.Invoke(new EventHandler(ACTUALIZAR));

            }
            catch (Exception error)
            {
                altura = 0.0;
                velocidad = 0.0;
                aceleracion = 0.0;
                presion = "0.0";
                Temperatura = "0.0";
                Orientacion_x = "0.0";
                Orientacion_y = "0.0";
                MessageBox.Show("falla en actualizar");

            }

        }

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]

        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);
        private void CONTROL_Paint(object sender, PaintEventArgs e)
        {

        }

        private void TiempoReloj_Tick(object sender, EventArgs e)
        {
            reloj.Text = DateTime.Now.ToString("hh:mm:ss tt");
        }

        private void SpaceVoyager_Paint(object sender, PaintEventArgs e)
        {

        }

        private void max_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            max.Visible = false;
            maximisar.Visible = true;
        }

        private void minimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void maximisar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            maximisar.Visible = false;
            max.Visible = true;
        }

        private void salir_Click(object sender, EventArgs e)
        {
            clok.Stop();
            tmrRedraw.Stop();
            if (IsOpen == true)
            {
                puerto.Close();
            }

            this.Close();
            // anular todo

            Form1 abrir = new Form1();
            abrir.Show();
        }

        // para mover la ventana
        private void SpaceVoyager_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void button1_Click(object sender, EventArgs e)
        {

            string[] ports = SerialPort.GetPortNames();
            PUERTOS.DataSource = ports;
        }

        // activacion de swith para conectar y desconetar
        private void ConecDesc_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (ConecDesc.Checked)
                {
                        serial();
                        puerto.Open();
                        bunifuPictureBox1.Image = Properties.Resources.icons8_disconnected_32px;
                        IsOpen = true;
                        label3.Text = "DESCONECTAR";
                        clok.Start();
                       clok.Interval = Convert.ToInt32(TiempoIntervalo.Text);
                       tmrRedraw.Start();
                        tmrRedraw.Interval= Convert.ToInt32(TiempoIntervalo.Text);

                }

                else
                {
                 
                    clok.Stop();
                    tmrRedraw.Stop();
                    IsOpen = false;

                    Console.WriteLine("CLOSED PORT");

                    bunifuPictureBox1.Image = Properties.Resources.conectar1;
                    label3.Text = "CONECTAR";

                    htiemp.Series[0].Points.Clear();
                    Atiem.Series[0].Points.Clear();
                    Vtiem.Series[0].Points.Clear();
                    datos_puerto = "$0.0;0.0;0.0;0.0;0.0;0,0;0.0";
                    tiempo = 0.0;
                    velocidad = 0.0;
                    aceleracion = 0.0;
                    altura = 0.0;
                    presion = "0.0";
                    Temperatura = "0.0";
                    Orientacion_x = "0.0";
                    Orientacion_y ="0.0";
                    puerto.Close();
                }

            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
                ConecDesc.Checked = false;
            }

        }

        private void clok_Tick(object sender, EventArgs e)
        {
            tiempo ++;
            htiemp.Series[0].Points.AddXY(tiempo, altura);
            Atiem.Series[0].Points.AddXY(tiempo, aceleracion);
            Vtiem.Series[0].Points.AddXY(tiempo, velocidad);
            label11.Text = velocidad.ToString();
            label9.Text = aceleracion.ToString();
            label7.Text = altura.ToString();
            label14.Text = Orientacion_x;
            label20.Text = Orientacion_y;

         
            //uso de circle bar temperatura
            string[] textSplit = Temperatura.Split('.');
            bunifuCircleProgress1.ValueByTransition = Convert.ToInt32(textSplit[0]);
            bunifuCircleProgress1.SubScriptText = textSplit[1];

            //uso de circle bar presion
            string[] textSplit1 = presion.Split('.');
            bunifuCircleProgress2.ValueByTransition = Convert.ToInt32(textSplit1[0]);
            bunifuCircleProgress2.SubScriptText = textSplit1[1];

            //oerientacion x
            pictureBox3.Image = RotateImage(image, new PointF(650, 410), 180*Convert.ToSingle(Orientacion_x));

            //---------------------rotacion----------------------
            for (int i = 0; i < 100; i++)
            {
                System.Threading.Thread.Sleep(velocidadGrafica);
                Application.DoEvents();
                for (int item = startCuboAlgoritmo; item < endCuboAlgoritmo; item++)
                {
                    Rectangle r = rectangles[item];
                    decimal[,] rotationX = FactoryMatrix.getRotX(Convert.ToDecimal(Orientacion_x)/180);
                    decimal[,] rotationY = FactoryMatrix.getRotZ(Convert.ToDecimal(Orientacion_y)/180);
                    decimal[,] result = FactoryMatrix.xMxM(rotationY, rotationX);
                    r.matrix = FactoryMatrix.xMxM(result, r.matrix);
                }
            }

        }

        private void PauseReanu_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (PauseReanu.Checked)
                {
                    bunifuPictureBox2.Image = Properties.Resources.reanudar;
                    
                    label5.Text = "REANUDAR";
                    clok.Stop();
                    tmrRedraw.Stop();
                }

                else
                {

                    bunifuPictureBox2.Image = Properties.Resources.pausa;
                    label5.Text = "PAUSAR";
                    if(IsOpen==true)
                    {
                        clok.Start();
                        tmrRedraw.Start();

                    }
                }

            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            clok.Stop();
            tmrRedraw.Stop();
            if (IsOpen == true)
            {
                puerto.Close();
            }

            this.Close();
            // anular todo

            Form1 abrir = new Form1();
            abrir.Show();
        }

        private void bunifuPictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void Atiem_Click(object sender, EventArgs e)
        {

        }
        //-----------------GL------------
        private void gl_Load(object sender, EventArgs e)
        {
            int tam = 30;
            Rectangle r;
            startCuboAlgoritmo = rectangles.Count;
            /* Cara 1 */
            r = new Rectangle();
            r.points[1][1] = tam;
            r.points[2][0] = tam;
            r.points[2][1] = tam;
            r.points[3][0] = tam;
            r.color = Color.White;
            rectangles.Add(r);

            ///////* Cara 2 */
            r = new Rectangle();
            r.points[0][1] = tam;
            r.points[1][0] = tam;
            r.points[1][1] = tam;
            r.points[2][0] = tam;
            r.points[2][1] = tam;
            r.points[2][2] = tam;
            r.points[3][1] = tam;
            r.points[3][2] = tam;
            r.color = Color.Pink;
            rectangles.Add(r);

            /////* Cara 3 */
            r = new Rectangle();
            r.points[0][2] = tam;
            r.points[1][1] = tam;
            r.points[1][2] = tam;
            r.points[2][0] = tam;
            r.points[2][1] = tam;
            r.points[2][2] = tam;
            r.points[3][0] = tam;
            r.points[3][2] = tam;
            r.color = Color.Green;
            rectangles.Add(r);

            /////* Cara 4 */
            r = new Rectangle();
            r.points[1][0] = tam;
            r.points[2][0] = tam;
            r.points[2][2] = tam;
            r.points[3][2] = tam;
            r.color = Color.Blue;
            rectangles.Add(r);

            /////* Cara 5 */
            r = new Rectangle();
            r.points[0][0] = tam;
            r.points[1][0] = tam;
            r.points[1][2] = tam;
            r.points[2][0] = tam;
            r.points[2][1] = tam;
            r.points[2][2] = tam;
            r.points[3][0] = tam;
            r.points[3][1] = tam;
            r.color = Color.CadetBlue;
            rectangles.Add(r);

            /////* Cara 6 */
            r = new Rectangle();
            r.points[1][2] = tam;
            r.points[2][1] = tam;
            r.points[2][2] = tam;
            r.points[3][1] = tam;
            r.color = Color.Gold;
            rectangles.Add(r);

            endCuboAlgoritmo = rectangles.Count;

            //calcular baricentro:
            baricentroCuboAlgoritmo[0] = 0;
            baricentroCuboAlgoritmo[1] = 0;
            baricentroCuboAlgoritmo[2] = 0;
            for (int item = startCuboAlgoritmo; item < endCuboAlgoritmo; item++)
            {
                r = rectangles[item];
                baricentroCuboAlgoritmo[0] += r.points[0][0] + r.points[1][0] + r.points[2][0] + r.points[3][0];
                baricentroCuboAlgoritmo[1] += r.points[0][1] + r.points[1][1] + r.points[2][1] + r.points[3][1];
                baricentroCuboAlgoritmo[2] += r.points[0][2] + r.points[1][2] + r.points[2][2] + r.points[3][2];
            }
            baricentroCuboAlgoritmo[0] /= (4 * (endCuboAlgoritmo - startCuboAlgoritmo));
            baricentroCuboAlgoritmo[1] /= (4 * (endCuboAlgoritmo - startCuboAlgoritmo));
            baricentroCuboAlgoritmo[2] /= (4 * (endCuboAlgoritmo - startCuboAlgoritmo));

            eyeX = (int)(baricentroCuboAlgoritmo[0] * (baricentroCuboAlgoritmo[0] / 4)) + 10;
            eyeY = (int)(baricentroCuboAlgoritmo[1] * (baricentroCuboAlgoritmo[1] / 4)) + 10;
            eyeZ = (int)(baricentroCuboAlgoritmo[2] * (baricentroCuboAlgoritmo[2] / 4)) + 10;
            actualizarValor();
        }
        private void lineGenerator(float width, Color color, int x1, int y1, int z1, int x2, int y2, int z2)
        {
            Line temp = new Line();

            temp.from[0] = x1;
            temp.from[1] = y1;
            temp.from[2] = z1;
            temp.to[0] = x2;
            temp.to[1] = y2;
            temp.to[2] = z2;
            temp.color = color;
            temp.width = width;
            lines.Add(temp);
            Console.WriteLine("coordenadas");
        }
        private void generarFlechas()
        {
            lineGenerator(1, Color.Yellow, -100, 0, 0, 100, 0, 0);
            lineGenerator(1, Color.Yellow, -100, 0, 0, -90, 0, 5);
            lineGenerator(1, Color.Yellow, -100, 0, 0, -90, 0, -5);
            lineGenerator(1, Color.Yellow, 100, 0, 0, 90, 0, 5);
            lineGenerator(1, Color.Yellow, 100, 0, 0, 90, 0, -5);

            lineGenerator(1, Color.Red, 0, -100, 0, 0, 100, 0);
            lineGenerator(1, Color.Red, 0, -100, 0, 5, -90, 0);
            lineGenerator(1, Color.Red, 0, -100, 0, -5, -90, 0);
            lineGenerator(1, Color.Red, 0, 100, 0, 5, 90, 0);
            lineGenerator(1, Color.Red, 0, 100, 0, -5, 90, 0);

            lineGenerator(1, Color.Blue, 0, 0, -100, 0, 0, 100);
            lineGenerator(1, Color.Blue, 0, 0, -100, 0, 5, -90);
            lineGenerator(1, Color.Blue, 0, 0, -100, 0, -5, -90);
            lineGenerator(1, Color.Blue, 0, 0, 100, 0, 5, 90);
            lineGenerator(1, Color.Blue, 0, 0, 100, 0, -5, 90);
        }
        private void actualizarValor()
        {
            lbl_valorX.Text = eyeX.ToString();
            lbl_valorY.Text = eyeY.ToString();
            lbl_valorZ.Text = eyeZ.ToString();
        }

        private void openGLControl1_Load(object sender, EventArgs e)
        {
            loaded = true;

            generarFlechas();
            actualizarValor();
            Console.WriteLine("grafica");
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {


            //-------------prurvs---------
            //r = new Rectangle();
            //r.points[0][1] = 10;
            //r.points[1][1] = 20;
            //r.points[2][1] = tam;
            //r.points[3][0] = tam;
            //r.color = Color.White;
            //rectangles.Add(r);
            //----------------------------


            //r = new Rectangle();

            //for (int i = 0; i < 4; i++)
            //{
            //    double theta = 10* i;
            //    r.points[i][0] = (decimal)Math.Cos(theta);    // x
            //    r.points[i][0] = (decimal)Math.Sin(theta);    // y
            //    r.points[i][2] = (decimal)1.0;                  //z
            //    r.color = Color.White;
            //    rectangles.Add(r);
            //    Console.WriteLine(r.points[i][2]);
            //}

            //// Coordenadas del cuerpo
            //for (int i = 0; i < 4; i++)
            //{
            //    double theta = 2.0 * Math.PI * i / 10.0;
            //    r.points[i+10][0] = (decimal)Math.Cos(theta);    // x
            //    r.points[i+10][1] = (decimal)Math.Sin(theta);    // y
            //    r.points[i+10][2] = (decimal)0.0;
            //    r.color = Color.White;
            //    rectangles.Add(r);// z
            //}

           
        }
    }
}
