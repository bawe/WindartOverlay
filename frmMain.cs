
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;


using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using System.Configuration;

namespace WindartOverlay
{
    public partial class frmMain : Form
    {
        private const string PIPE_NAME = "DartOverlay";
        private Point m_mouseposition;
        private delegate void SafeCallDelegate(string text);

        private bool m_hideatstart = false;

        public frmMain()
        {
            InitializeComponent();

            //this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.ControlBox = false;
            this.Text = String.Empty;
            transparentControl1.Image = imageList1.Images[1];
            this.FormBorderStyle = FormBorderStyle.None;

            Util.DoUpgrade(Properties.Settings.Default);
            
            this.Location = Properties.Settings.Default.StartPos;
            //if (m_hideatstart)
            //    this.Hide();
            
            var server = Task.Factory.StartNew(() => RunServer(this));
        }

        public frmMain(string[] args) :this()
        {
            //if(args.Contains("/hide"))
            //{
            //    m_hideatstart = true;
            //}

            //if (args.Contains("/opacity"))
            //{
            //    Opacity = .4;
            //}


            for (int i = 0; i < args.Length; i++)
            {
                if(args[i] == "/hide")
                    m_hideatstart = true;

                if (args[i] == "/opacity")
                {
                    if(i++ < args.Length)
                    {
                        double opacity = double.Parse(args[i]);
                        this.Opacity = opacity / 100;
                    }
                        
                }
            }

            

        }

        static void RunServer(frmMain form)
        {

            // echo Hello > \\.\pipe\TestPipe1

            while (true) { 
                using (var pipeServer = new NamedPipeServerStream(PIPE_NAME, PipeDirection.In))
                {
                    pipeServer.WaitForConnection();
                    using (var reader = new StreamReader(pipeServer))
                    {
                            string line = String.Empty;
                            while ((line = reader.ReadLine()) != null)
                            {
                            Console.WriteLine(line);
                            form.Event(line);
                            }
                        
                    }
                }
            }
        }



        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // turn on WS_EX_TOOLWINDOW style bit
                cp.ExStyle |= 0x80;
                return cp;
            }
        }


        public void Event(string message)
        {
            message = message.Trim();
            //MessageBox.Show(message);


            if (this.InvokeRequired)
            {
                var d = new SafeCallDelegate(Event);
                this.Invoke(d, new object[] { message });
            }
            else
            {
                if (message == "hide")
                    this.Visible = false;
                if (message == "show")
                    this.Visible = true;
            }
        }

     

        private void transparentControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(m_mouseposition.X, m_mouseposition.Y);
                Location = mousePos;
                Properties.Settings.Default.StartPos = this.Location;
                ;
            }
        }

        private void transparentControl1_MouseDown(object sender, MouseEventArgs e)
        {
            m_mouseposition = new Point(-e.X, -e.Y);
        }



        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
 
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.StartPos = this.Location;
            Properties.Settings.Default.Save();
        }

        public static class Util
        {
            // the name of the setting that flags whether we
            // should perform an upgrade or not
            private const string UpgradeFlag = "SettingsUpgrade";

            public static void DoUpgrade(ApplicationSettingsBase settings)
            {
                try
                {
                    // attempt to read the upgrade flag
                    if ((bool)settings[UpgradeFlag] == true)
                    {
                        // upgrade the settings to the latest version
                        settings.Upgrade();

                        // clear the upgrade flag
                        settings[UpgradeFlag] = false;
                        settings.Save();
                    }
                    else
                    {
                        // the settings are up to date
                    }
                }
                catch (SettingsPropertyNotFoundException ex)
                {
                    // notify the developer that the upgrade
                    // flag should be added to the settings file
                    throw ex;
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void hideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            if (m_hideatstart)
                this.Hide();

            

        }
    }


}
