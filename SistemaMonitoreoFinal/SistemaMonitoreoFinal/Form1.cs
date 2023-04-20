using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;

namespace SistemaMonitoreoFinal
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]

        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);
        private void contra_Click(object sender, EventArgs e)
        {
            if (contraseña.Text == "abrir")
            {
                INICIO VENTANA = new INICIO();
                VENTANA.Show();
                this.Hide();
            }
            else
            {
                contraseña.Text = "";
            }
        }

        private void salir_Click(object sender, EventArgs e)
        {
            Application.Exit();

        }

        private void minimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void SpaceVoyager_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        //ENTER DE LA CONTRASEÑA
        private void contra_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void contra_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void contraseña_OnValueChanged(object sender, EventArgs e)
        {

        }

        private void contraseña_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyData==Keys.Enter)
            {
                if (contraseña.Text == "abrir")
                {
                    INICIO VENTANA = new INICIO();
                    VENTANA.Show();
                    this.Hide();
                }
                else
                {
                    contraseña.Text = "";
                }
            }
        }

        //


    }
}
