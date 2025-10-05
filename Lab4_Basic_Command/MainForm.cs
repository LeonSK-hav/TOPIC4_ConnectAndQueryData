using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab4_Basic_Command
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        private string connectionString = "server=.; database=Restaurant Management; Integrated Security=true";
        private int selectedTableId = 0;
        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadTables();
        }
        private void LoadTables()
        {
            flpTableList.Controls.Clear();
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandText = "SELECT ID, Name, Status, Capacity FROM [Table]";

            sqlConnection.Open();
            SqlDataReader reader = sqlCommand.ExecuteReader();
            DisplayTable(reader);
            sqlConnection.Close();
        }

        private void DisplayTable(SqlDataReader reader)
        {
            while (reader.Read())
            {
                bool status = Convert.ToBoolean(reader["Status"]);
                int capacity = Convert.ToInt32(reader["Capacity"]);

                Button btn = new Button()
                {
                    Width = 100,
                    Height = 80,
                    Text = reader["Name"].ToString() + "\n(" + capacity + " chỗ)",
                    BackColor = status ? Color.LightCoral : Color.LightGreen,
                    Tag = new
                    {
                        ID = Convert.ToInt32(reader["ID"]),
                        Name = reader["Name"].ToString(),
                        Status = status,
                        Capacity = capacity
                    }
                };
                btn.ContextMenuStrip = cmsTable; // GÁN context menu
                btn.Click += TableButton_Click; // sự kiện click
                flpTableList.Controls.Add(btn);
            }

        }
        private void TableButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            dynamic table = btn.Tag;

            selectedTableId = table.ID;
            txtTableName.Text = table.Name;
            txtCapacity.Text = table.Capacity.ToString();

            LoadBill(table.ID);
        }
        private void LoadBill(int tableId)
        {
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();

            SqlCommand billCommand = sqlConnection.CreateCommand();
            billCommand.CommandText = "SELECT TOP 1 * FROM Bills WHERE TableID = @id AND Status = 0 ORDER BY ID DESC";
            billCommand.Parameters.AddWithValue("@id", tableId);

            SqlDataReader billReader = billCommand.ExecuteReader();

            if (!billReader.HasRows)
            {
                billReader.Close();
                sqlConnection.Close();

                dgvBill.Rows.Clear();
                txtTotal.Clear();
                txtDiscount.Clear();
                txtTax.Clear();
                txtFinalTotal.Clear();

                MessageBox.Show("Bàn này chưa có hóa đơn nào.");
                return;
            }

            billReader.Read();
            int invoiceID = Convert.ToInt32(billReader["ID"]);

            double discountRaw = 0;
            double taxRaw = 0;

            if (billReader["Disscount"] != DBNull.Value)
                discountRaw = Convert.ToDouble(billReader["Disscount"]);
            if (billReader["Tax"] != DBNull.Value)
                taxRaw = Convert.ToDouble(billReader["Tax"]);

            billReader.Close();

            SqlCommand detailCommand = sqlConnection.CreateCommand();
            detailCommand.CommandText =
                "SELECT f.Name, bd.Quantity, f.Unit, f.Price " +
                "FROM BillDetails bd JOIN Food f ON bd.FoodID = f.ID " +
                "WHERE bd.InvoiceID = @invoiceID";
            detailCommand.Parameters.AddWithValue("@invoiceID", invoiceID);

            SqlDataReader detailReader = detailCommand.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(detailReader);
            detailReader.Close();

            sqlConnection.Close();

            dgvBill.Columns.Clear();
            dgvBill.Columns.Add("Name", "Tên món");
            dgvBill.Columns.Add("Quantity", "Số lượng");
            dgvBill.Columns.Add("Unit", "Đơn vị");
            dgvBill.Columns.Add("Price", "Đơn giá");
            dgvBill.Columns.Add("Total", "Thành tiền");

            dgvBill.Rows.Clear();

            double total = 0;
            foreach (DataRow row in dt.Rows)
            {
                double price = Convert.ToDouble(row["Price"]);
                int quantity = Convert.ToInt32(row["Quantity"]);
                double itemTotal = price * quantity;

                dgvBill.Rows.Add(row["Name"].ToString(), quantity, row["Unit"].ToString(), price, itemTotal);
                total += itemTotal;
            }

            double discountPercent = (discountRaw <= 1) ? discountRaw * 100.0 : discountRaw;
            double taxPercent = (taxRaw <= 1) ? taxRaw * 100.0 : taxRaw;

            txtTotal.Text = total.ToString();
            txtDiscount.Text = discountPercent.ToString();
            txtTax.Text = taxPercent.ToString();

            double final = total - (total * discountPercent / 100.0) + (total * taxPercent / 100.0);
            txtFinalTotal.Text = final.ToString();
        }

        private void btnAddTable_Click(object sender, EventArgs e)
        {
            string name = txtTableName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Tên bàn không được để trống!");
                return;
            }

            if (!int.TryParse(txtCapacity.Text, out int capacity))
            {
                MessageBox.Show("Sức chứa phải là số nguyên!");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Kiểm tra bàn trùng tên
                SqlCommand checkCmd = conn.CreateCommand();
                checkCmd.CommandText = "SELECT COUNT(*) FROM [Table] WHERE Name = @name";
                checkCmd.Parameters.AddWithValue("@name", name);
                int exists = (int)checkCmd.ExecuteScalar();

                if (exists > 0)
                {
                    MessageBox.Show("Bàn này đã tồn tại! Vui lòng chọn bàn khác hoặc cập nhật.");
                    return;
                }

                // Nếu chưa có thì thêm mới
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO [Table](Name, Status, Capacity) VALUES (@name, 0, @capacity)";
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@capacity", capacity);

                int result = cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    MessageBox.Show("Thêm bàn thành công!");
                    LoadTables();
                }
                else
                {
                    MessageBox.Show("Không thể thêm bàn!");
                }
            }
        }

        private void btnDeleteTable_Click(object sender, EventArgs e)
        {
            if (selectedTableId == 0)
            {
                MessageBox.Show("Vui lòng chọn bàn cần xóa!");
                return;
            }

            var confirm = MessageBox.Show("Bạn có chắc muốn xóa bàn này không?", "Xác nhận", MessageBoxButtons.YesNo);
            if (confirm == DialogResult.No) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM [Table] WHERE ID = @id";
                cmd.Parameters.AddWithValue("@id", selectedTableId);

                int result = cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    MessageBox.Show("Đã xóa bàn!");
                    selectedTableId = 0;
                    txtTableName.Clear();
                    txtCapacity.Clear();
                    LoadTables();
                }
                else
                    MessageBox.Show("Không thể xóa bàn!");
            }
        }

        private void btnEditTable_Click(object sender, EventArgs e)
        {
            if (selectedTableId == 0)
            {
                MessageBox.Show("Vui lòng chọn bàn cần cập nhật!");
                return;
            }

            string name = txtTableName.Text.Trim();
            if (!int.TryParse(txtCapacity.Text, out int capacity))
            {
                MessageBox.Show("Sức chứa phải là số nguyên!");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE [Table] SET Name = @name, Capacity = @capacity WHERE ID = @id";
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@capacity", capacity);
                cmd.Parameters.AddWithValue("@id", selectedTableId);

                int result = cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    MessageBox.Show("Đã cập nhật thông tin bàn!");
                    LoadTables();
                }
                else
                    MessageBox.Show("Không thể cập nhật bàn!");
            }
        }

        private void tmsiDeleteTable_Click(object sender, EventArgs e)
        {
            if (cmsTable.SourceControl is Button btn && btn.Tag != null)
            {
                dynamic table = btn.Tag;
                int id = table.ID;

                var confirm = MessageBox.Show(
                    $"Bạn có chắc muốn xóa {table.Name} không?",
                    "Xác nhận xóa",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );
                if (confirm == DialogResult.No) return;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Kiểm tra bàn có hóa đơn chưa thanh toán
                    SqlCommand check = conn.CreateCommand();
                    check.CommandText = "SELECT COUNT(*) FROM Bills WHERE TableID = @id AND Status = 0";
                    check.Parameters.AddWithValue("@id", id);
                    int active = (int)check.ExecuteScalar();

                    if (active > 0)
                    {
                        MessageBox.Show("Không thể xóa bàn vì vẫn còn hóa đơn chưa thanh toán!");
                        return;
                    }

                    SqlCommand delete = conn.CreateCommand();
                    delete.CommandText = "DELETE FROM [Table] WHERE ID = @id";
                    delete.Parameters.AddWithValue("@id", id);

                    int rows = delete.ExecuteNonQuery();
                    MessageBox.Show(rows > 0 ? "Đã xóa bàn!" : "Không thể xóa bàn!");
                }

                LoadTables();
            }
        }

        private void tmsiViewBills_Click(object sender, EventArgs e)
        {
            if (cmsTable.SourceControl is Button btn && btn.Tag != null)
            {
                dynamic table = btn.Tag;
                int id = table.ID;

                InvoiceListForm f = new InvoiceListForm(id);
                f.ShowDialog();
            }
        }

        private void tmsiBillLog_Click(object sender, EventArgs e)
        {
            InvoiceLogForm logForm = new InvoiceLogForm();
            logForm.ShowDialog();
        }
    }
}
