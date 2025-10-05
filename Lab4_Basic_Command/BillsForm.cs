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
    public partial class BillsForm : Form
    {
        public BillsForm()
        {
            InitializeComponent();
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            string connectionString = "server=.; database=Restaurant Management; Integrated Security=true";
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = sqlConnection.CreateCommand();

            sqlCommand.CommandText =
                "SELECT ID, Name, TableID, Amount, Disscount, Tax, Status, CheckoutDate, Account " +
                "FROM Bills " +
                "WHERE CheckoutDate BETWEEN @from AND @to";

            sqlCommand.Parameters.AddWithValue("@from", dtpFromDate.Value.Date);
            sqlCommand.Parameters.AddWithValue("@to", dtpToDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59));

            SqlDataAdapter da = new SqlDataAdapter(sqlCommand);
            DataTable dt = new DataTable();

            sqlConnection.Open();
            da.Fill(dt);
            sqlConnection.Close();

            dgvBills.DataSource = dt;

            // Tính tổng tiền
            decimal total = 0, discount = 0, final = 0;
            foreach (DataRow row in dt.Rows)
            {
                decimal amount = row["Amount"] != DBNull.Value ? Convert.ToDecimal(row["Amount"]) : 0;
                decimal dis = row["Disscount"] != DBNull.Value ? Convert.ToDecimal(row["Disscount"]) : 0;
                total += amount;
                discount += amount * dis;
                final += amount - (amount * dis);
            }

            txtTotalAmount.Text = total.ToString("#,##0");
            txtDiscount.Text = discount.ToString("#,##0");
            txtFinalAmount.Text = final.ToString("#,##0");

            //// Ẩn bớt cột phụ (tùy chọn)
            //if (dgvBills.Columns.Contains("Status"))
            //    dgvBills.Columns["Status"].Visible = false;
            //if (dgvBills.Columns.Contains("Account"))
            //    dgvBills.Columns["Account"].Visible = false;

            //dgvBills.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            //dgvBills.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //dgvBills.ReadOnly = true;
        }
      

        private void dgvBills_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            int billID = Convert.ToInt32(dgvBills.Rows[e.RowIndex].Cells["ID"].Value);

            BillDetailsForm detailsForm = new BillDetailsForm(billID);
            detailsForm.ShowDialog();
        }
    }
}
