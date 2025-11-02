using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;

namespace ToyShopWinApp
{
    public partial class frmProduct : Form
    {
        private string connectionString;

        public frmProduct()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
            LoadCategories();
            LoadProducts();
        }

        // Load Products với khung màu đẹp
        private void LoadProducts()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT p.ProductID, p.ProductName, pt.TypeName AS Category,
                               p.Price, p.Stock
                        FROM Product p
                        LEFT JOIN ProductType pt ON p.TypeID = pt.TypeID";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvProducts.AutoGenerateColumns = false;
                    dgvProducts.Columns.Clear();

                    // Thêm các cột
                    dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
                    {
                        Name = "ProductID",
                        HeaderText = "ID",
                        DataPropertyName = "ProductID",
                        ReadOnly = true
                    });
                    dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
                    {
                        Name = "ProductName",
                        HeaderText = "Product Name",
                        DataPropertyName = "ProductName"
                    });
                    dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
                    {
                        Name = "Category",
                        HeaderText = "Category",
                        DataPropertyName = "Category"
                    });
                    dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
                    {
                        Name = "Price",
                        HeaderText = "Price",
                        DataPropertyName = "Price"
                    });
                    dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
                    {
                        Name = "Stock",
                        HeaderText = "Stock",
                        DataPropertyName = "Stock"
                    });

                    dgvProducts.DataSource = dt;

                    // Cấu hình hiển thị
                    dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dgvProducts.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                    dgvProducts.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                    dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dgvProducts.ReadOnly = true;

                    // Khung & màu đẹp
                    dgvProducts.EnableHeadersVisualStyles = false;
                    dgvProducts.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);
                    dgvProducts.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
                    dgvProducts.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold);
                    dgvProducts.GridColor = System.Drawing.Color.LightGray;
                    dgvProducts.CellBorderStyle = DataGridViewCellBorderStyle.Single;
                    dgvProducts.RowTemplate.Height = 30;

                    // Màu xen kẽ
                    dgvProducts.RowsDefaultCellStyle.BackColor = System.Drawing.Color.White;
                    dgvProducts.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(242, 242, 242);

                    // ScrollBars
                    dgvProducts.ScrollBars = ScrollBars.Both;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading products: " + ex.Message);
            }
        }

        // Load Category vào ComboBox
        private void LoadCategories()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT TypeName FROM ProductType";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    cbCategory.DataSource = dt;
                    cbCategory.DisplayMember = "TypeName";
                    cbCategory.ValueMember = "TypeName";
                    cbCategory.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading categories: " + ex.Message);
            }
        }

        // Click DataGridView → điền TextBox + ComboBox
        private void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = dgvProducts.Rows[e.RowIndex];

            txtProductID.Text = row.Cells["ProductID"].Value?.ToString() ?? "";
            txtProductName.Text = row.Cells["ProductName"].Value?.ToString() ?? "";
            txtPrice.Text = row.Cells["Price"].Value?.ToString() ?? "";
            txtStock.Text = row.Cells["Stock"].Value?.ToString() ?? "";
            cbCategory.SelectedValue = row.Cells["Category"].Value?.ToString() ?? null;
        }

        // Thêm sản phẩm
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductName.Text) ||
                string.IsNullOrWhiteSpace(txtPrice.Text) ||
                string.IsNullOrWhiteSpace(txtStock.Text) ||
                cbCategory.SelectedIndex == -1)
            {
                MessageBox.Show("Please fill all fields and select a category!");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        INSERT INTO Product (ProductName, TypeID, Price, Stock)
                        VALUES (@name, (SELECT TypeID FROM ProductType WHERE TypeName=@type), @price, @stock)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", txtProductName.Text.Trim());
                        cmd.Parameters.AddWithValue("@type", cbCategory.SelectedValue.ToString());
                        cmd.Parameters.AddWithValue("@price", decimal.Parse(txtPrice.Text.Trim()));
                        cmd.Parameters.AddWithValue("@stock", int.Parse(txtStock.Text.Trim()));
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("✅ Product added successfully!");
                LoadProducts();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when adding product: " + ex.Message);
            }
        }

        // Cập nhật sản phẩm
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductID.Text))
            {
                MessageBox.Show("Please select a product to update!");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtProductName.Text) ||
                string.IsNullOrWhiteSpace(txtPrice.Text) ||
                string.IsNullOrWhiteSpace(txtStock.Text) ||
                cbCategory.SelectedIndex == -1)
            {
                MessageBox.Show("Please fill all fields before updating!");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        UPDATE Product
                        SET ProductName = @name,
                            TypeID = (SELECT TypeID FROM ProductType WHERE TypeName=@type),
                            Price = @price,
                            Stock = @stock
                        WHERE ProductID = @id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", int.Parse(txtProductID.Text));
                        cmd.Parameters.AddWithValue("@name", txtProductName.Text.Trim());
                        cmd.Parameters.AddWithValue("@type", cbCategory.SelectedValue.ToString());
                        cmd.Parameters.AddWithValue("@price", decimal.Parse(txtPrice.Text.Trim()));
                        cmd.Parameters.AddWithValue("@stock", int.Parse(txtStock.Text.Trim()));
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("✅ Product updated successfully!");
                LoadProducts();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when updating product: " + ex.Message);
            }
        }

        // Xóa sản phẩm
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductID.Text))
            {
                MessageBox.Show("Please select a product to delete!", "Warning",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult dr = MessageBox.Show(
                "Are you sure you want to delete this product?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dr == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "DELETE FROM Product WHERE ProductID = @id";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", int.Parse(txtProductID.Text));
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("🗑️ Product deleted successfully!", "Deleted",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadProducts();
                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error when deleting product: " + ex.Message, "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Search sản phẩm
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query;
                    if (string.IsNullOrWhiteSpace(keyword))
                    {
                        query = @"
                        SELECT p.ProductID, p.ProductName, pt.TypeName AS Category,
                               p.Price, p.Stock
                        FROM Product p
                        LEFT JOIN ProductType pt ON p.TypeID = pt.TypeID";
                    }
                    else
                    {
                        query = @"
                        SELECT p.ProductID, p.ProductName, pt.TypeName AS Category,
                               p.Price, p.Stock
                        FROM Product p
                        LEFT JOIN ProductType pt ON p.TypeID = pt.TypeID
                        WHERE CAST(p.ProductID AS NVARCHAR) LIKE @keyword
                           OR p.ProductName LIKE @keyword
                           OR CAST(p.Price AS NVARCHAR) LIKE @keyword
                           OR CAST(p.Stock AS NVARCHAR) LIKE @keyword
                           OR pt.TypeName LIKE @keyword";
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    if (!string.IsNullOrWhiteSpace(keyword))
                        adapter.SelectCommand.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvProducts.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when searching: " + ex.Message);
            }
        }

        // Refresh + Clear
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadProducts();
            ClearFields();
        }

        // Clear TextBox + ComboBox
        private void ClearFields()
        {
            txtProductID.Clear();
            txtProductName.Clear();
            txtPrice.Clear();
            txtStock.Clear();
            txtSearch.Clear();
            cbCategory.SelectedIndex = -1;
        }
    }
}
