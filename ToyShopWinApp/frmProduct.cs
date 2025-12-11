using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ToyShopWinApp
{
    public partial class frmProduct : Form
    {
        private string connectionString;

        public frmProduct()
        {
            InitializeComponent();

            connectionString = ConfigurationManager.ConnectionStrings["ToyShopDB"].ConnectionString;

            LoadCategories();
            LoadProducts();

            dgvProducts.CellClick += dgvProducts_CellClick;

            txtProductID.ReadOnly = true;
            txtProductID.BorderStyle = BorderStyle.FixedSingle; // tránh thu nhỏ
        }

        // Load danh mục vào ComboBox
        private void LoadCategories()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT TypeID, TypeName FROM ProductType";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cbCategory.DataSource = dt;
                cbCategory.DisplayMember = "TypeName";
                cbCategory.ValueMember = "TypeID";
                cbCategory.SelectedIndex = -1;
            }
        }

        // Load sản phẩm
        private void LoadProducts()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT p.ProductID, p.ProductName, p.Price, p.Stock,
                           pt.TypeName, pt.TypeID
                    FROM Product p
                    LEFT JOIN ProductType pt ON p.TypeID = pt.TypeID
                    ORDER BY p.ProductID ASC"; // sắp xếp theo ID

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Thêm cột STT (thứ tự hiển thị liên tục)
                if (!dt.Columns.Contains("STT"))
                    dt.Columns.Add("STT", typeof(int));

                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["STT"] = i + 1;

                dgvProducts.DataSource = dt;

                // Hiển thị STT trước ProductID
                if (dgvProducts.Columns.Contains("STT"))
                    dgvProducts.Columns["STT"].DisplayIndex = 0;
            }
        }

        // Khi click vào DataGridView → hiển thị thông tin
        private void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvProducts.Rows[e.RowIndex];

            txtProductID.Text = row.Cells["ProductID"].Value.ToString();
            txtProductName.Text = row.Cells["ProductName"].Value.ToString();
            txtPrice.Text = row.Cells["Price"].Value.ToString();
            txtStock.Text = row.Cells["Stock"].Value.ToString();

            if (row.Cells["TypeID"].Value != DBNull.Value)
                cbCategory.SelectedValue = row.Cells["TypeID"].Value;
        }

        // ADD sản phẩm
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductName.Text) ||
                string.IsNullOrWhiteSpace(txtPrice.Text) ||
                string.IsNullOrWhiteSpace(txtStock.Text) ||
                cbCategory.SelectedIndex == -1)
            {
                MessageBox.Show("Please fill all fields!");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO Product (ProductName, TypeID, Price, Stock)
                                 VALUES (@name, @type, @price, @stock)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", txtProductName.Text);
                cmd.Parameters.AddWithValue("@type", cbCategory.SelectedValue);
                cmd.Parameters.AddWithValue("@price", decimal.Parse(txtPrice.Text));
                cmd.Parameters.AddWithValue("@stock", int.Parse(txtStock.Text));

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("✅ Product added successfully!");
            LoadProducts();
            ClearFields();
        }

        // UPDATE sản phẩm
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductID.Text))
            {
                MessageBox.Show("Please select a product to update!");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"UPDATE Product
                                 SET ProductName=@name, TypeID=@type, Price=@price, Stock=@stock
                                 WHERE ProductID=@id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", int.Parse(txtProductID.Text));
                cmd.Parameters.AddWithValue("@name", txtProductName.Text);
                cmd.Parameters.AddWithValue("@type", cbCategory.SelectedValue);
                cmd.Parameters.AddWithValue("@price", decimal.Parse(txtPrice.Text));
                cmd.Parameters.AddWithValue("@stock", int.Parse(txtStock.Text));

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("✅ Product updated successfully!");
            LoadProducts();
            ClearFields();
        }

        // DELETE sản phẩm
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductID.Text))
            {
                MessageBox.Show("Please select a product to delete!");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Product WHERE ProductID=@id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", int.Parse(txtProductID.Text));

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("🗑️ Product deleted successfully!");
            LoadProducts(); // STT sẽ tự động sắp xếp lại
            ClearFields();
        }

        // SEARCH sản phẩm
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT p.ProductID, p.ProductName, p.Price, p.Stock,
                           pt.TypeName, pt.TypeID
                    FROM Product p
                    LEFT JOIN ProductType pt ON p.TypeID = pt.TypeID
                    WHERE p.ProductName LIKE @keyword OR pt.TypeName LIKE @keyword
                    ORDER BY p.ProductID ASC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

                DataTable dt = new DataTable();
                da.Fill(dt);

                // Thêm STT
                if (!dt.Columns.Contains("STT"))
                    dt.Columns.Add("STT", typeof(int));
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["STT"] = i + 1;

                dgvProducts.DataSource = dt;

                if (dgvProducts.Columns.Contains("STT"))
                    dgvProducts.Columns["STT"].DisplayIndex = 0;
            }
        }

        // REFRESH
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadProducts();
            ClearFields();
        }

        // CLEAR input fields
        private void ClearFields()
        {
            txtProductID.Clear();
            txtProductName.Clear();
            txtPrice.Clear();
            txtStock.Clear();
            txtSearch.Clear();
            cbCategory.SelectedIndex = -1;
        }

        private void txtProductID_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
