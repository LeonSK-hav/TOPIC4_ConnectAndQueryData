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
    public partial class InvoiceListForm : Form
    {
        private int tableId;
        private string connectionString = "server=.; database=Restaurant Management; Integrated Security=true";
        public InvoiceListForm(int tableId)
        {
            InitializeComponent();
            this.tableId = tableId;

        }

        private void InvoiceListForm_Load(object sender, EventArgs e)
        {
            LoadInvoiceDates();
        }

        private void LoadInvoiceDates()
        {
            lstInvoices.Items.Clear();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT ID, CheckoutDate FROM Bills WHERE TableID = @id ORDER BY CheckoutDate DESC";
                cmd.Parameters.AddWithValue("@id", tableId);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader["CheckoutDate"] != DBNull.Value)
                    {
                        DateTime date = Convert.ToDateTime(reader["CheckoutDate"]);
                        int invoiceId = Convert.ToInt32(reader["ID"]);

                        lstInvoices.Items.Add(new
                        {
                            ID = invoiceId,
                            Display = date.ToString("dd/MM/yyyy HH:mm")
                        });
                    }
                }
            }

            lstInvoices.DisplayMember = "Display";
            lstInvoices.ValueMember = "ID";
        }

        private void lstInvoices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstInvoices.SelectedItem == null) return;
            dynamic selected = lstInvoices.SelectedItem;
            int invoiceId = selected.ID;

            LoadInvoiceDetails(invoiceId);
        }

        private void LoadInvoiceDetails(int invoiceId)
        {
            dgvDetails.Columns.Clear();
            dgvDetails.Columns.Add("Name", "Tên món");
            dgvDetails.Columns.Add("Quantity", "Số lượng");
            dgvDetails.Columns.Add("Price", "Đơn giá");
            dgvDetails.Columns.Add("Total", "Thành tiền");

            dgvDetails.Rows.Clear();

            double total = 0;
            double discount = 0;
            double tax = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Lấy chi tiết món ăn
                SqlCommand detailCmd = conn.CreateCommand();
                detailCmd.CommandText =
                    "SELECT f.Name, bd.Quantity, f.Price " +
                    "FROM BillDetails bd JOIN Food f ON bd.FoodID = f.ID " +
                    "WHERE bd.InvoiceID = @id";
                detailCmd.Parameters.AddWithValue("@id", invoiceId);

                SqlDataReader detailReader = detailCmd.ExecuteReader();
                while (detailReader.Read())
                {
                    string name = detailReader["Name"].ToString();
                    int quantity = Convert.ToInt32(detailReader["Quantity"]);
                    double price = Convert.ToDouble(detailReader["Price"]);
                    double itemTotal = price * quantity;

                    dgvDetails.Rows.Add(name, quantity, price, itemTotal);
                    total += itemTotal;
                }
                detailReader.Close();

                // Lấy thông tin thuế và giảm giá
                SqlCommand billCmd = conn.CreateCommand();
                billCmd.CommandText = "SELECT Disscount, Tax FROM Bills WHERE ID = @id";
                billCmd.Parameters.AddWithValue("@id", invoiceId);

                SqlDataReader billReader = billCmd.ExecuteReader();
                if (billReader.Read())
                {
                    if (billReader["Disscount"] != DBNull.Value)
                        discount = Convert.ToDouble(billReader["Disscount"]);

                    if (billReader["Tax"] != DBNull.Value)
                        tax = Convert.ToDouble(billReader["Tax"]);
                }
                billReader.Close();
            }

            // Tính toán
            double discountPercent = (discount <= 1) ? discount * 100 : discount;
            double taxPercent = (tax <= 1) ? tax * 100 : tax;

            double finalTotal = total - (total * discountPercent / 100.0) + (total * taxPercent / 100.0);

            txtTotal.Text = total.ToString("N0");
            txtDiscount.Text = discountPercent.ToString();
            txtTax.Text = taxPercent.ToString();
            txtFinalTotal.Text = finalTotal.ToString("N0");
        }
    }
}
