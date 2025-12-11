using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ToyShopWinApp
{
    public partial class Orders : Form
    {
        private string connectionString;
        private DataTable dtOrderDetails;  // Chi tiết đơn hàng
        private DataTable dtProducts;      // Danh sách sản phẩm

        public Orders()
        {
            InitializeComponent();

            connectionString = ConfigurationManager.ConnectionStrings["ToyShopDB"].ConnectionString;

            InitializeOrderDetailsGrid();
            LoadCustomers();
            LoadProducts();

            dgvOrderDetails.CellValueChanged += DgvOrderDetails_CellValueChanged;
        }

        // Khởi tạo DataTable cho chi tiết đơn hàng
        private void InitializeOrderDetailsGrid()
        {
            dtOrderDetails = new DataTable();
            dtOrderDetails.Columns.Add("ProductID", typeof(int));
            dtOrderDetails.Columns.Add("ProductName", typeof(string));
            dtOrderDetails.Columns.Add("Quantity", typeof(int));
            dtOrderDetails.Columns.Add("UnitPrice", typeof(decimal));
            dtOrderDetails.Columns.Add("Total", typeof(decimal));

            dgvOrderDetails.DataSource = dtOrderDetails;
            dgvOrderDetails.Columns["ProductID"].Visible = false;

            dgvOrderDetails.Columns["ProductName"].ReadOnly = true;
            dgvOrderDetails.Columns["UnitPrice"].ReadOnly = true;
            dgvOrderDetails.Columns["Total"].ReadOnly = true;

            dgvOrderDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvOrderDetails.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        // Load khách hàng vào ComboBox cbCustomer
        private void LoadCustomers()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string sql = "SELECT CustomerID, FullName FROM Customer";
                    SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cbCustomer.DataSource = dt;
                    cbCustomer.DisplayMember = "FullName";
                    cbCustomer.ValueMember = "CustomerID";
                    cbCustomer.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading customers: " + ex.Message);
            }
        }

        // Load sản phẩm vào ComboBox cbProduct
        private void LoadProducts()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string sql = "SELECT ProductID, ProductName, Price FROM Product";
                    SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                    dtProducts = new DataTable();
                    da.Fill(dtProducts);

                    cbProduct.DataSource = dtProducts;
                    cbProduct.DisplayMember = "ProductName";
                    cbProduct.ValueMember = "ProductID";
                    cbProduct.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading products: " + ex.Message);
            }
        }

        // Thêm sản phẩm đã chọn vào chi tiết đơn hàng
        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            if (cbProduct.SelectedValue == null)
            {
                MessageBox.Show("Please select a product!");
                return;
            }

            int selectedProductID = Convert.ToInt32(cbProduct.SelectedValue);

            // Kiểm tra sản phẩm đã có trong đơn chưa (nếu có, tăng số lượng)
            foreach (DataRow row in dtOrderDetails.Rows)
            {
                if (Convert.ToInt32(row["ProductID"]) == selectedProductID)
                {
                    // Tăng Quantity lên 1
                    row["Quantity"] = Convert.ToInt32(row["Quantity"]) + 1;
                    row["Total"] = Convert.ToInt32(row["Quantity"]) * Convert.ToDecimal(row["UnitPrice"]);
                    UpdateTotalAmount();
                    return;
                }
            }

            // Nếu chưa có thì thêm mới
            DataRow[] selected = dtProducts.Select($"ProductID = {selectedProductID}");
            if (selected.Length == 0)
            {
                MessageBox.Show("Selected product not found!");
                return;
            }
            DataRow prod = selected[0];

            DataRow newRow = dtOrderDetails.NewRow();
            newRow["ProductID"] = prod["ProductID"];
            newRow["ProductName"] = prod["ProductName"];
            newRow["Quantity"] = 1;
            newRow["UnitPrice"] = Convert.ToDecimal(prod["Price"]);
            newRow["Total"] = Convert.ToDecimal(prod["Price"]);
            dtOrderDetails.Rows.Add(newRow);

            UpdateTotalAmount();
        }

        // Xóa sản phẩm khỏi đơn hàng
        private void btnRemoveProduct_Click(object sender, EventArgs e)
        {
            if (dgvOrderDetails.CurrentRow != null)
            {
                dtOrderDetails.Rows.RemoveAt(dgvOrderDetails.CurrentRow.Index);
                UpdateTotalAmount();
            }
        }

        // Khi số lượng thay đổi trong DataGridView thì cập nhật lại Total và tổng tiền
        private void DgvOrderDetails_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvOrderDetails.Columns[e.ColumnIndex].Name == "Quantity")
            {
                var row = dtOrderDetails.Rows[e.RowIndex];
                int qty = 1;
                if (int.TryParse(row["Quantity"].ToString(), out qty))
                {
                    if (qty < 1)
                    {
                        MessageBox.Show("Quantity must be at least 1");
                        row["Quantity"] = 1;
                        qty = 1;
                    }
                }
                else
                {
                    row["Quantity"] = 1;
                    qty = 1;
                }

                decimal price = Convert.ToDecimal(row["UnitPrice"]);
                row["Total"] = qty * price;

                UpdateTotalAmount();
            }
        }

        // Cập nhật tổng tiền hiển thị trên txtTotalAmount
        private void UpdateTotalAmount()
        {
            decimal total = 0m;
            foreach (DataRow row in dtOrderDetails.Rows)
            {
                total += Convert.ToDecimal(row["Total"]);
            }

            txtTotalAmount.Text = total.ToString("0.00");
        }

        // Xóa toàn bộ đơn hàng trên form
        private void btnClear_Click(object sender, EventArgs e)
        {
            dtOrderDetails.Rows.Clear();
            txtTotalAmount.Text = "";
            cbCustomer.SelectedIndex = -1;
            cbProduct.SelectedIndex = -1;
            dtOrderDate.Value = DateTime.Today;
        }

        // Lưu đơn hàng vào DB (Order và OrderDetail)
        private void btnSaveOrder_Click(object sender, EventArgs e)
        {
            if (cbCustomer.SelectedValue == null || dtOrderDetails.Rows.Count == 0)
            {
                MessageBox.Show("Please select a customer and add products before printing the invoice!");
                return;
            }

            string customerName = cbCustomer.Text;
            string orderDate = dtOrderDate.Value.ToString("dd/MM/yyyy");
            decimal totalAmount = Convert.ToDecimal(txtTotalAmount.Text);

            // Invoice header
            string invoice = "===== INVOICE =====\n";
            invoice += $"Customer: {customerName}\n";
            invoice += $"Order Date: {orderDate}\n";
            invoice += "-------------------------------\n";

            // Column titles, align: 20 chars for name, 5 for quantity, 10 for unit price, 12 for total
            invoice += string.Format("{0,-20} {1,5} {2,10} {3,12}\n", "Product", "Qty", "Unit Price", "Total");
            invoice += "------------------------------------------------------\n";

            // Product rows
            foreach (DataRow row in dtOrderDetails.Rows)
            {
                string name = row["ProductName"].ToString();
                int qty = Convert.ToInt32(row["Quantity"]);
                decimal price = Convert.ToDecimal(row["UnitPrice"]);
                decimal total = Convert.ToDecimal(row["Total"]);

                invoice += string.Format("{0,-20} {1,5} {2,10:0.00} {3,12:0.00}\n", name, qty, price, total);
            }

            invoice += "------------------------------------------------------\n";
            invoice += string.Format("{0,-20} {1,5} {2,10} {3,12:0.00}\n", "", "", "Total:", totalAmount);

            MessageBox.Show(invoice, "Invoice", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }
}
