using System;
using System.Windows.Forms;

namespace Optimization.StartUp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            //string[] dirPaths;
            //StartUp.StartOptimization();
            Close();
        }

        private void select_Click(object sender, EventArgs e)
        {

        }
    }
}
