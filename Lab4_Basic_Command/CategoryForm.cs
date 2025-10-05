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
    public partial class CategoryForm : Form
    {
        public CategoryForm()
        {
            InitializeComponent();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            string connectionString = "server=.; database = Restaurant Management; Integrated Security = true";
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            string query = "SELECT ID, Name, Type FROM Category";
            sqlCommand.CommandText = query;
            sqlConnection.Open();
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            this.DisplayCategory(sqlDataReader);
            sqlConnection.Close();

        }

        private void DisplayCategory(SqlDataReader reader)
        {
            lvCategory.Items.Clear();
            while (reader.Read())
            {
                ListViewItem item = new ListViewItem(reader["ID"].ToString());
                lvCategory.Items.Add(item);
                item.SubItems.Add(reader["Name"].ToString());
                item.SubItems.Add(reader["Type"].ToString());
                
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string connectionString = "server=.; database = Restaurant Management; Integrated Security = true";
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandText = "INSERT INTO Category(Name, [Type])" + "VALUES (N'" + txtCategoryName.Text + "', '" + txtType.Text + "')";
            sqlConnection.Open();
            int numOfRowsAffected = sqlCommand.ExecuteNonQuery();
            sqlConnection.Close();
            if (numOfRowsAffected == 1)
            {
                MessageBox.Show("Thêm nhóm món ăn thành công");
                btnLoad.PerformClick();
                txtCategoryName.Text = "";
                txtType.Text = "";
            }
            else
            {
                MessageBox.Show("Đã có lỗi xảy ra. Vui lòng thử lại");
            }
        }

        private void lvCategory_Click(object sender, EventArgs e)
        {
            ListViewItem item = lvCategory.SelectedItems[0];
            txtCategoryID.Text = item.Text;
            txtCategoryName.Text = item.SubItems[1].Text;
            txtType.Text = item.SubItems[2].Text == "0" ? "Thức uống" : "Đồ ăn"; 
            btnUpdate.Enabled = true;
            btnDelete.Enabled = true;
        }
        
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            string connectionString = "server=.; database = Restaurant Management; Integrated Security = true";
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            //sqlCommand.CommandText = "UPDATE Category SET Name = N'" + txtCategoryName.Text + "', [Type] = " + txtType.Text + " WHERE ID = " + txtCategoryID.Text;

 
            sqlCommand.CommandText =
                "UPDATE Category SET Name = @name, [Type] = @type WHERE ID = @id";
           
            int typeValue;
            if (txtType.Text.Trim().ToLower() == "thức uống")
                typeValue = 0;
            else if (txtType.Text.Trim().ToLower() == "đồ ăn")
                typeValue = 1;
            else
            {
                MessageBox.Show("Vui lòng nhập đúng 'Thức uống' hoặc 'Đồ ăn'");
                return;
            }
            // 🟢 Truyền tham số
            sqlCommand.Parameters.AddWithValue("@name", txtCategoryName.Text);
            sqlCommand.Parameters.AddWithValue("@type", typeValue);
            sqlCommand.Parameters.AddWithValue("@id", txtCategoryID.Text);

            sqlConnection.Open();
            int numOfRowsAffected = sqlCommand.ExecuteNonQuery();
            sqlConnection.Close();

            if (numOfRowsAffected == 1)
            {
                ListViewItem item = lvCategory.SelectedItems[0];
                item.SubItems[1].Text = txtCategoryName.Text;
                item.SubItems[2].Text = typeValue.ToString();
                // Xoa cac o nhap
                txtCategoryID.Text = "";
                txtCategoryName.Text = "";
                txtType.Text = "";

                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
                MessageBox.Show("Cập nhật nhóm món ăn thành công");
            }
            else
            {
                MessageBox.Show("Đã có lỗi xảy ra. Vui lòng thử lại");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string connectionString = "server=.; database = Restaurant Management; Integrated Security = true";
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            // gán câu lệnh SQL DELETE
            sqlCommand.CommandText = "DELETE FROM Category " + "WHERE ID = " + txtCategoryID.Text;
            sqlConnection.Open();
            int numOfRowsEffected = sqlCommand.ExecuteNonQuery();
            sqlConnection.Close();

            if(numOfRowsEffected == 1)
            {
                ListViewItem item = lvCategory.SelectedItems[0];
                lvCategory.Items.Remove(item);
                txtCategoryID.Text = "";
                txtCategoryName.Text = "";
                txtType.Text = "";

                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
                MessageBox.Show("Xóa nhóm món ăn thành công");
            }
            else
            {
                MessageBox.Show("Đã có lỗi xảy ra. Vui lòng thử lại");
            }
        }

        private void tsmDelete_Click(object sender, EventArgs e)
        {
            if(lvCategory.SelectedItems.Count  > 0)
                btnDelete.PerformClick();
        }

        private void tsmViewFood_Click(object sender, EventArgs e)
        {
            if (txtCategoryID.Text != "")
            {
                FoodForm foodForm = new FoodForm();
                foodForm.Show(this);
                foodForm.LoadFood(Convert.ToInt32(txtCategoryID.Text));
            }
        }
    }
}
