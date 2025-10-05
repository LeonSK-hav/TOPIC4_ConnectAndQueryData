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
    public partial class BillDetailsForm : Form
    {
        private int billID;

        public BillDetailsForm(int billID)
        {
            InitializeComponent();
            this.billID = billID;
            LoadBillDetails();
        }
        private void LoadBillDetails()
        {
            string connectionString = "server=.; database=Restaurant Management; Integrated Security=true";
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = sqlConnection.CreateCommand();

            sqlCommand.CommandText =
           "SELECT bd.ID, bd.InvoiceID, bd.FoodID, bd.Quantity, " +
                    "f.Name AS N'Tên Món ăn', f.Price AS N'Giá', (bd.Quantity * f.Price) AS N'Tổng cộng' " +
                    "FROM BillDetails bd " +
                    "INNER JOIN Food f ON bd.FoodID = f.ID " +
                    "WHERE bd.InvoiceID = @billID";

            sqlCommand.Parameters.AddWithValue("@billID", billID);

            SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
            DataTable dt = new DataTable();
            sqlConnection.Open();
            adapter.Fill(dt);
           
            sqlConnection.Close();

            dgvBillDetails.DataSource = dt;
            //// Set style cho bảng
            //dgvBillDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            //dgvBillDetails.ReadOnly = true;
            //dgvBillDetails.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }
        private void BillDetailsForm_Load(object sender, EventArgs e)
        {
        }
    }
}
