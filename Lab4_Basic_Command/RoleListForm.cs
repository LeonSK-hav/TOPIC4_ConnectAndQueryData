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
    public partial class RoleListForm : Form
    {
        private string connectionString;
        private string currentAccountName;
        public RoleListForm(string accountName, string connStr)
        {
            InitializeComponent();
            currentAccountName = accountName;
            this.connectionString = connStr;
        }

        private void RoleListForm_Load(object sender, EventArgs e)
        {
            LoadRoles();
        }
        private void LoadRoles()
        {
            string query = @"SELECT r.ID AS RoleID, r.RoleName, ra.Actived, ra.Notes
                         FROM RoleAccount ra
                         JOIN Role r ON ra.RoleID = r.ID
                         WHERE ra.AccountName = @AccountName";
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@AccountName", currentAccountName);
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvRolesList.DataSource = dt;
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                foreach (DataGridViewRow row in dgvRolesList.Rows)
                {
                    if (row.IsNewRow) continue;

                    int roleId = Convert.ToInt32(row.Cells["RoleID"].Value);
                    bool actived = Convert.ToBoolean(row.Cells["Actived"].Value);
                    string notes = row.Cells["Notes"].Value?.ToString() ?? "";

                    string query = "UPDATE RoleAccount SET Actived=@actived, Notes=@notes WHERE RoleID=@roleId AND AccountName=@accountName";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@actived", actived);
                        cmd.Parameters.AddWithValue("@notes", notes);
                        cmd.Parameters.AddWithValue("@roleId", roleId);
                        cmd.Parameters.AddWithValue("@accountName", currentAccountName);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
