using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Lab4_Basic_Command
{
    public partial class FoodForm : Form
    {
        int currentCategoryID;
        public FoodForm()
        {
            InitializeComponent();
        }

        public void LoadFood(int categoryID)
        {
            currentCategoryID = categoryID;
            string connectionString = "server=.; database = Restaurant Management; Integrated Security = true";
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = sqlConnection.CreateCommand();

            sqlCommand.CommandText = "SELECT Name FROM Category WHERE ID = " + categoryID;
            sqlConnection.Open();
            string catName = sqlCommand.ExecuteScalar().ToString();
            this.Text = "Danh sách món ăn thuộc nhóm: " + catName;
            sqlCommand.CommandText = "SELECT * FROM Food WHERE FoodCategoryID = " + categoryID;
            SqlDataAdapter da = new SqlDataAdapter(sqlCommand);
            DataTable dt = new DataTable("Food");
            da.Fill(dt);
            dgvFood.DataSource = dt;
            sqlConnection.Close();
            sqlConnection.Dispose();
            da.Dispose();
        }

  
        private void btnSave_Click_1(object sender, EventArgs e)
        {
            string connectionString = "server=.; database = Restaurant Management; Integrated Security = true";
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = sqlConnection.CreateCommand();

            sqlConnection.Open();
            foreach (DataGridViewRow row in dgvFood.Rows)
            {
                if (row.IsNewRow) continue;

                int id = row.Cells["ID"].Value == DBNull.Value ? 0 : Convert.ToInt32(row.Cells["ID"].Value);
                string name = row.Cells["FoodName"].Value?.ToString();
                string unit = row.Cells["Unit"].Value?.ToString();
                float price = row.Cells["Price"].Value == DBNull.Value ? 0 : Convert.ToSingle(row.Cells["Price"].Value);
                string notes = row.Cells["Notes"].Value?.ToString();

                if (id == 0) // thêm mới
                {
                    sqlCommand.CommandText =
                        "INSERT INTO Food(Name, Unit, FoodCategoryID, Price, Notes) " +
                        "VALUES (@name, @unit, @foodCategoryID, @price, @notes)";
                }
                else // cập nhật
                {
                    sqlCommand.CommandText =
                        "UPDATE Food SET Name = @name, Unit = @unit, FoodCategoryID = @foodCategoryID, " +
                        "Price = @price, Notes = @notes WHERE ID = @id";
                    sqlCommand.Parameters.AddWithValue("@id", id);
                }

                sqlCommand.Parameters.AddWithValue("@name", name);
                sqlCommand.Parameters.AddWithValue("@unit", unit);
                sqlCommand.Parameters.AddWithValue("@foodCategoryID", currentCategoryID);
                sqlCommand.Parameters.AddWithValue("@price", price);
                sqlCommand.Parameters.AddWithValue("@notes", notes);

                sqlCommand.ExecuteNonQuery();
                sqlCommand.Parameters.Clear();
            }
            sqlConnection.Close();

            MessageBox.Show("Lưu thay đổi thành công!");
            LoadFood(currentCategoryID); // 🟢 refresh lại danh sách sau khi lưu
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvFood.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn món ăn cần xóa!");
                return;
            }

            DialogResult result = MessageBox.Show("Bạn có chắc muốn xóa món này không?", "Xác nhận", MessageBoxButtons.YesNo);
            if (result == DialogResult.No)
                return;

            string connectionString = "server=.; database = Restaurant Management; Integrated Security = true";
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = sqlConnection.CreateCommand();

            sqlConnection.Open();
            foreach (DataGridViewRow row in dgvFood.SelectedRows)
            {
                int id = Convert.ToInt32(row.Cells["ID"].Value);
                sqlCommand.CommandText = "DELETE FROM Food WHERE ID = @id";
                sqlCommand.Parameters.AddWithValue("@id", id);
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Parameters.Clear();
            }
            sqlConnection.Close();

            MessageBox.Show("Xóa món ăn thành công!");
            LoadFood(currentCategoryID);
        }

        
    }
}
