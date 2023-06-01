using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MAST.Presentation.Controls
{
    public partial class MassiveListView : ListView
    {
        public MassiveListView()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
        }

        internal void Populate(Logic.SimulationCollection simulationCollection)
        {
            this.BeginUpdate();
            int i = 0;
            foreach (Logic.Simulation sim in simulationCollection.sims)
            {
                ListViewItem lvi = new ListViewItem(new string[] { sim.directory.Name, "Queued", "0%", sim.directory.FullName});
                lvi.Tag = sim;
                this.Items.Add(lvi);
                i++;
            }
            this.EndUpdate();
        }
    }
}
