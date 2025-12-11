using StuffedToyShopDB;
using System;
using System.Drawing;
using System.Windows.Forms;
using ToyShop;

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

        private void frmMain_Load(object sender, EventArgs e)
        {
     
        }

        private void customerMenu_Click(object sender, EventArgs e)
        {
            CustomerForm f = new CustomerForm();
            f.ShowDialog();
        }

        private void ordersMenu_Click(object sender, EventArgs e)
        {
            Orders f = new Orders();
            f.ShowDialog();
        }

        private void menuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
           
        }
    }
}
