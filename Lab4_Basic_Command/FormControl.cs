using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab4_Basic_Command
{
    public partial class FormControl : Form
    {
        public FormControl()
        {
            InitializeComponent();
        }

        private void foodCategoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new CategoryForm();
            form.ShowDialog();
        }

        private void billsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new BillsForm();
            form.ShowDialog();
        }

        private void accountManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new AccountManager();
            form.ShowDialog();
        }

        private void mainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new MainForm();
            form.ShowDialog();
        }
    }
}
