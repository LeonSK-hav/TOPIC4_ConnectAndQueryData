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
    public partial class InvoiceLogForm : Form
    {
        private string connectionString = "server=.; database=Restaurant Management; Integrated Security=true";
        public InvoiceLogForm()
        {
            InitializeComponent();
        }

        private void InvoiceLogForm_Load(object sender, EventArgs e)
        {
            LoadInvoiceLog();
        }
        private void LoadInvoiceLog()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText =
                    @"SELECT 
                b.ID,
                t.Name AS TableName,
                b.CheckoutDate,
                b.Amount,
                b.Disscount,
                b.Tax,
                (b.Amount - (b.Amount * b.Disscount / 100) + (b.Amount * b.Tax / 100)) AS FinalTotal,
                a.FullName AS Staff
            FROM Bills AS b
            JOIN [Table] AS t ON b.TableID = t.ID
            LEFT JOIN Account AS a ON b.Account = a.AccountName
            WHERE b.Status = 1
            ORDER BY b.CheckoutDate DESC";

                DataTable data = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(data);

                dgvInvoiceLog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvInvoiceLog.DataSource = data;
            }

            CalculateSummary();
        }


        private void CalculateSummary()
        {
            if (dgvInvoiceLog.Rows.Count == 0) return;

            int totalInvoices = 0;
            double totalAmount = 0;
            double totalDiscount = 0;
            double totalTax = 0;

            foreach (DataGridViewRow row in dgvInvoiceLog.Rows)
            {
                if (row.IsNewRow) continue;
                totalInvoices++;

                double amount = Convert.ToDouble(row.Cells["Amount"].Value);
                double discount = Convert.ToDouble(row.Cells["Disscount"].Value);
                double tax = Convert.ToDouble(row.Cells["Tax"].Value);

                totalDiscount += amount * discount / 100.0;
                totalTax += amount * tax / 100.0;
                totalAmount += Convert.ToDouble(row.Cells["FinalTotal"].Value);
            }

            txtTotalInvoices.Text = totalInvoices.ToString();
            txtTotalDiscount.Text = totalDiscount.ToString("N0");
            txtTotalTax.Text = totalTax.ToString("N0");
            txtTotalAmount.Text = totalAmount.ToString("N0");
        }
    }
}
    

