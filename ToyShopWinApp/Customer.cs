using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ToyShop
{
    public partial class CustomerForm : Form
    {
        private string connectionString =ConfigurationManager.ConnectionStrings["ToyShopDB"].ConnectionString;


        public CustomerForm()
        {
            InitializeComponent();
        }

        private void CustomerForm_Load(object sender, EventArgs e)
        {
            LoadCustomerData();
            dgvCustomer.CellClick += dgvCustomer_CellClick;
        }

        // Load all customers from SQL
        private void LoadCustomerData()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Customer";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvCustomer.DataSource = dt;
            }
        }

        // Add new customer
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Please enter full name and phone number.");
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO Customer (FullName, Address, Phone)
                                 VALUES (@FullName, @Address, @Phone)";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@FullName", txtFullName.Text);
                cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                cmd.Parameters.AddWithValue("@Phone", txtPhone.Text);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            LoadCustomerData();
            ClearInput();
            MessageBox.Show("Customer added successfully!");
        }

        // Update selected customer
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCustomerID.Text))
            {
                MessageBox.Show("Please select a customer to update.");
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"UPDATE Customer
                                 SET FullName=@FullName, Address=@Address, Phone=@Phone
                                 WHERE CustomerID=@CustomerID";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CustomerID", txtCustomerID.Text);
                cmd.Parameters.AddWithValue("@FullName", txtFullName.Text);
                cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                cmd.Parameters.AddWithValue("@Phone", txtPhone.Text);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            LoadCustomerData();
            ClearInput();
            MessageBox.Show("Customer updated successfully!");
        }

        // Delete selected customer
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCustomerID.Text))
            {
                MessageBox.Show("Please select a customer to delete.");
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Customer WHERE CustomerID=@CustomerID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CustomerID", txtCustomerID.Text);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            LoadCustomerData();
            ClearInput();
            MessageBox.Show("Customer deleted successfully!");
        }

        // Search customers by name or phone
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT * FROM Customer
                                 WHERE FullName LIKE @keyword OR Phone LIKE @keyword";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.SelectCommand.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                    dgvCustomer.DataSource = dt;
                else
                    MessageBox.Show("No matching customers found.");
            }
        }

        // Refresh / clear search
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadCustomerData();
            txtSearch.Clear();
        }

        // Click on row → show info in textboxes
        private void dgvCustomer_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                txtCustomerID.Text = dgvCustomer.Rows[e.RowIndex].Cells["CustomerID"].Value.ToString();
                txtFullName.Text = dgvCustomer.Rows[e.RowIndex].Cells["FullName"].Value.ToString();
                txtAddress.Text = dgvCustomer.Rows[e.RowIndex].Cells["Address"].Value.ToString();
                txtPhone.Text = dgvCustomer.Rows[e.RowIndex].Cells["Phone"].Value.ToString();
            }
        }

        // Clear all input fields
        private void ClearInput()
        {
            txtCustomerID.Clear();
            txtFullName.Clear();
            txtAddress.Clear();
            txtPhone.Clear();
        }
    }
}
