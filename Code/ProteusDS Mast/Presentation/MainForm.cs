using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MAST.Presentation
{
    public partial class MainForm : Form
    {
        Logic.SimulationCollection simulationCollection = new Logic.SimulationCollection();
        bool pauseButtonDisplaysPause = true;
        object locker = new object();

        public MainForm()
        {
            InitializeComponent();

            this.simulationCollection = new Logic.SimulationCollection();
            this.simulationCollection.OnSimulationComplete += new Logic.SimulationComplete(simulationCollection_OnSimulationComplete);
            this.simulationCollection.OnSimulationUpdate += new Logic.SimulationUpdate(simulationCollection_OnSimulationUpdate);
            this.simulationCollection.OnUDPUpdate += new Logic.UDP_Update(simulationCollection_OnUDPUpdate);

            this.SetMaxSimultaneousSims();
        }

        void simulationCollection_OnUDPUpdate(UDP_Service.UDP_Message message, Logic.Simulation sim)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(() => this.simulationCollection_OnUDPUpdate(message, sim)));
            }
            else
            {
                this.massiveListView1.Items[sim.index].SubItems[1].Text = sim.Status.ToString();
                this.massiveListView1.Items[sim.index].SubItems[2].Text = string.Format("{0}%", sim.Progress.ToString());
            }
        }

        void simulationCollection_OnSimulationUpdate(Logic.Simulation sim)
        {

        }

        void simulationCollection_OnSimulationComplete(Logic.Simulation sim)
        {
            this.simulationCollection_OnUDPUpdate(null, sim);

            // log it
            lock (this.locker)
            {
                string filename = System.IO.Path.Combine(sim.directory.Parent.FullName.ToString(), "log.txt");
                if (!System.IO.File.Exists(filename))
                {
                    using (System.IO.TextWriter tw = System.IO.File.CreateText(filename))
                    {
                        tw.WriteLine("# foldername | exitcode | scriptRan | scriptExitCode");
                        tw.Close();
                    }
                }
                using (System.IO.TextWriter tw = System.IO.File.AppendText(filename))
                {
                    tw.WriteLine("{0} {1} {2} {3}", sim.directory.Name, sim.exitcode, sim.scriptRan, sim.scriptExitCode);
                    tw.Close();
                }
            }
        }

        protected int GetNumberOfCores()
        {
            int coreCount = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }
            return coreCount;
        }

        private void SetMaxSimultaneousSims()
        {
            int numCores = Math.Max(Environment.ProcessorCount, this.GetNumberOfCores() * 2);

            this.numericUpDownMaxNumConcurrentSims.Maximum = numCores * 2;
            this.numericUpDownMaxNumConcurrentSims.Value = numCores - 1;
        }

        private void buttonLoadSims_Click(object sender, EventArgs e)
        {
            // retrieve sims from folder
            OpenFileDialog ofd = new OpenFileDialog();

            // for now, we'll require a .MAST file to be present
            ofd.Filter = "MAST file (*.MAST)|*.MAST";
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.DirectoryInfo di = new System.IO.FileInfo(ofd.FileName).Directory;

                this.massiveListView1.Items.Clear();
                if (this.simulationCollection.RetrieveSimulations(di, true))
                {
                    this.massiveListView1.Populate(this.simulationCollection);
                }
            }
        }

        private void buttonStartAll_Click(object sender, EventArgs e)
        {
            this.simulationCollection.Start();

            this.buttonStartAll.Enabled = false;
        }

        private void buttonPauseAll_Click(object sender, EventArgs e)
        {
            if (this.pauseButtonDisplaysPause)
            {
                this.simulationCollection.PauseAll();
                this.pauseButtonDisplaysPause = false;
                this.buttonPauseAll.Text = "Resume All";
            }
            else
            {
                this.simulationCollection.UnPauseAll();
                this.pauseButtonDisplaysPause = true;
                this.buttonPauseAll.Text = "Pause All";
            }
        }

        private void buttonHaltAll_Click(object sender, EventArgs e)
        {
            this.simulationCollection.HaltAll();
            this.buttonStartAll.Enabled = true;
        }

        private void numericUpDownMaxNumConcurrentSims_ValueChanged(object sender, EventArgs e)
        {
            this.simulationCollection.maxNumSimultaneousSims = (int)this.numericUpDownMaxNumConcurrentSims.Value;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (MessageBox.Show("Close Application", "Close application now?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                this.simulationCollection.Shutdown();
                return;
            }
            else
            {
                e.Cancel = true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            this.simulationCollection.Shutdown();
            base.OnClosed(e);
        }
    }
}
