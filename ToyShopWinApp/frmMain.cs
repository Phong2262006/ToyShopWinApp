using System;
using System.Windows.Forms;

namespace ToyShopWinApp
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void loginMenu_Click(object sender, EventArgs e)
        {
            frmLogin loginForm = new frmLogin();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Login successful!", "Welcome", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void productsMenu_Click(object sender, EventArgs e)
        {
            frmProduct productForm = new frmProduct();
            productForm.ShowDialog();
        }

        private void exitMenu_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Exit application?", "Exit", MessageBoxButtons.YesNo) == DialogResult.Yes)
                Application.Exit();
        }
    }
}
