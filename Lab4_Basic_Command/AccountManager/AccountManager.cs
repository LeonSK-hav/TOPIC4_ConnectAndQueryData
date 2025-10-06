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
    public partial class AccountManager : Form
    {
        bool isChangingPassword = false;

        public AccountManager()
        {
            InitializeComponent();
        }
        private string connectionString = "server=.; database = Restaurant Management; Integrated Security = true";


        private void AccountManager_Load(object sender, EventArgs e)
        {
            cboRoleFilter.SelectedIndex = 0; // All
            LoadAllRoles();
            LoadAccounts();

           
        }
        #region HÀM LỌC VÀ LOAD DỮ LIỆU HIỂN THỊ LÊN DATAGRIDVIEW
        // Hàm load và lọc dữ liệu tài khoản và hiển thị lên DataGridView dgvAccountManager
        private void LoadAccounts(string roleFilter = "All", bool activeOnly = false)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = conn.CreateCommand();

                cmd.CommandText = @"
                    SELECT 
                        A.AccountName, 
                        A.FullName, 
                        A.Email, 
                        A.Tell, 
                        A.DateCreated, 
                        ISNULL(STRING_AGG(R.RoleName, ', '), '') AS Roles,
                        MAX(CAST(RA.Actived AS INT)) AS Actived
                    FROM Account A
                    LEFT JOIN RoleAccount RA ON A.AccountName = RA.AccountName
                    LEFT JOIN Role R ON R.ID = RA.RoleID
                    WHERE (RA.Actived = 1 OR @activeOnly = 0)
                    GROUP BY A.AccountName, A.FullName, A.Email, A.Tell, A.DateCreated
                    HAVING (@roleFilter = 'All' OR CHARINDEX(@roleFilter, ISNULL(STRING_AGG(R.RoleName, ', '), '')) > 0)
                    ORDER BY A.DateCreated DESC";

                cmd.Parameters.AddWithValue("@activeOnly", activeOnly ? 1 : 0);
                cmd.Parameters.AddWithValue("@roleFilter", roleFilter);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                dgvAccountManager.DataSource = dt;
            }
        }

        private void cbTrangThai_CheckedChanged(object sender, EventArgs e)
        {
            string roleFilter = cboRoleFilter.SelectedItem.ToString();
            bool activeOnly = cbTrangThai.Checked;
            LoadAccounts(roleFilter, activeOnly);
        }

        private void cboRoleFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            string roleFilter = cboRoleFilter.SelectedItem.ToString();
            bool activeOnly = cbTrangThai.Checked;
            LoadAccounts(roleFilter, activeOnly);
        }


        #endregion

        #region SỰ KIỆN KHI CHỌN HÀNG TRONG DATAGRIDVIEW VÀ HIỂN THỊ LÊN CÁC Ô NHẬP
        private void dgvAccountManager_Click(object sender, EventArgs e)
        {
            if (dgvAccountManager.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvAccountManager.SelectedRows[0];

                txtAccountName.Text = row.Cells["AccountName"].Value?.ToString();
                txtFullName.Text = row.Cells["FullName"].Value?.ToString();
                txtEmail.Text = row.Cells["Email"].Value?.ToString();
                txtTell.Text = row.Cells["Tell"].Value?.ToString();
                chkbActived.Checked = Convert.ToBoolean(row.Cells["Actived"].Value);

                if (row.Cells["DateCreated"].Value != DBNull.Value)
                    dtDateCreated.Value = Convert.ToDateTime(row.Cells["DateCreated"].Value);

                // ⚠️ Không hiển thị mật khẩu thật
                txtPassword.Text = "********";
                txtPassword.ReadOnly = true;

                // Reset trạng thái đổi mật khẩu
                isChangingPassword = false;
                btnResetPassword.Text = "Đổi mật khẩu";

                // Load quyền (nếu bạn có CheckListBox Role)
                LoadRolesForUser(txtAccountName.Text.Trim());
            }
           
        }
        #endregion

        #region HÀM HỖ TRỢ LOAD ROLE VÀ CHECK VÀO CHECKLISTBOX
        private void LoadRolesForUser(string accountName)
        {
            // Xóa các check cũ
            for (int i = 0; i < clbRoles.Items.Count; i++)
                clbRoles.SetItemChecked(i, false);

            if (string.IsNullOrEmpty(accountName))
                return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
            SELECT R.RoleName
            FROM RoleAccount RA
            JOIN Role R ON RA.RoleID = R.ID
            WHERE RA.AccountName = @AccountName AND RA.Actived = 1";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@AccountName", accountName);
                SqlDataReader reader = cmd.ExecuteReader();

                // Lặp qua từng quyền của user và check vào list
                while (reader.Read())
                {
                    string roleName = reader.GetString(0);
                    for (int i = 0; i < clbRoles.Items.Count; i++)
                    {
                        if (clbRoles.Items[i].ToString() == roleName)
                            clbRoles.SetItemChecked(i, true);
                    }
                }
            }
        }

        private void LoadAllRoles()
        {
            clbRoles.Items.Clear();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT RoleName FROM Role", conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    clbRoles.Items.Add(reader.GetString(0));
                }
            }
        }
        #endregion

        #region XỬ LÝ NÚT THÊM (LOGIC VÀ KIỂM TRA NGƯỜI DÙNG CÓ NHẬP THIẾU THÔNG TIN CẦN THIẾT?)
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAccountName.Text) || string.IsNullOrWhiteSpace(txtPassword.Text) || string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tên tài khoản, Mật khẩu và Họ tên!",
                        "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (clbRoles.CheckedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một vai trò (Role) cho tài khoản!",
                                "Thiếu quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Account WHERE AccountName = @acc", conn);
                checkCmd.Parameters.AddWithValue("@acc", txtAccountName.Text.Trim());
                int exists = (int)checkCmd.ExecuteScalar();
                if (exists > 0)
                {
                    MessageBox.Show("Tên tài khoản này đã tồn tại, vui lòng chọn tên khác!",
                                    "Tài khoản trùng", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Thêm vào bảng Account
                string insertAccount = @"
            INSERT INTO Account(AccountName, Password, FullName, Email, Tell, DateCreated)
            VALUES(@acc, @pass, @full, @email, @tell, GETDATE())";

                SqlCommand cmd = new SqlCommand(insertAccount, conn);
                cmd.Parameters.AddWithValue("@acc", txtAccountName.Text.Trim());
                cmd.Parameters.AddWithValue("@pass", txtPassword.Text.Trim());
                cmd.Parameters.AddWithValue("@full", txtFullName.Text.Trim());
                cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@tell", txtTell.Text.Trim());
                cmd.ExecuteNonQuery();

                // 2️⃣ Gán quyền cho tài khoản (RoleAccount)
                foreach (var item in clbRoles.CheckedItems)
                {
                    SqlCommand cmdRole = new SqlCommand(@"
                INSERT INTO RoleAccount(RoleID, AccountName, Actived)
                SELECT ID, @acc, 1 FROM Role WHERE RoleName = @role", conn);

                    cmdRole.Parameters.AddWithValue("@acc", txtAccountName.Text.Trim());
                    cmdRole.Parameters.AddWithValue("@role", item.ToString());
                    cmdRole.ExecuteNonQuery();
                }

                MessageBox.Show("Thêm tài khoản thành công!");
                LoadAccounts();
                btnMacDinh_Click(null, null); // reset form về mặc định
            }
        }
        #endregion

        #region XỬ LÝ NÚT CẬP NHẬT (LOGIC VÀ KIỂM TRA NGƯỜI DÙNG CÓ CHỌN TÀI KHOẢN NÀO CHƯA?)
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAccountName.Text))
            {
                MessageBox.Show("Vui lòng chọn tài khoản để cập nhật!");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // 1️⃣ Update bảng Account
                string updateAccount = @"
            UPDATE Account 
            SET FullName = @full, Email = @email, Tell = @tell
            WHERE AccountName = @acc";

                SqlCommand cmd = new SqlCommand(updateAccount, conn);
                cmd.Parameters.AddWithValue("@acc", txtAccountName.Text.Trim());
                cmd.Parameters.AddWithValue("@full", txtFullName.Text.Trim());
                cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@tell", txtTell.Text.Trim());
                cmd.ExecuteNonQuery();

                // 2️⃣ Xóa role cũ, thêm role mới
                SqlCommand cmdDel = new SqlCommand("DELETE FROM RoleAccount WHERE AccountName = @acc", conn);
                cmdDel.Parameters.AddWithValue("@acc", txtAccountName.Text.Trim());
                cmdDel.ExecuteNonQuery();

                foreach (var item in clbRoles.CheckedItems)
                {
                    SqlCommand cmdRole = new SqlCommand(@"
                INSERT INTO RoleAccount(RoleID, AccountName, Actived)
                SELECT ID, @acc, 1 FROM Role WHERE RoleName = @role", conn);
                    cmdRole.Parameters.AddWithValue("@acc", txtAccountName.Text.Trim());
                    cmdRole.Parameters.AddWithValue("@role", item.ToString());
                    cmdRole.ExecuteNonQuery();
                }

                MessageBox.Show("Cập nhật tài khoản thành công!");
                LoadAccounts();
            }
        }
        #endregion

        #region XỬ LÝ NÚT CẬP NHẬT MẬT KHẨU (CHO PHÉP ĐỔI MẬT KHẨU NẾU NGƯỜI DÙNG GHI ĐÚNG MẬT KHẨU CŨ, CÁC SỰ KIỆN UI LIÊN QUAN)
        private void btnResetPassword_Click(object sender, EventArgs e)
        {
            // Nếu chưa bật chế độ đổi mật khẩu
            if (!isChangingPassword)
            {
                // Hiện 2 ô mật khẩu cũ & mới
                lblOldPassword.Visible = txtOldPassword.Visible = true;
                lblNewPassword.Visible = txtNewPassword.Visible = true;
                btnCancel.Visible = true;

                txtOldPassword.Clear();
                txtNewPassword.Clear();

                txtPassword.ReadOnly = true; // vẫn không cho sửa trực tiếp mật khẩu chính
                btnResetPassword.Text = "Xác nhận đổi";
                isChangingPassword = true;
                return;
            }
          
            // Đã ở chế độ đổi mật khẩu → xử lý
            string username = txtAccountName.Text.Trim();
            string oldPass = txtOldPassword.Text.Trim();
            string newPass = txtNewPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(oldPass) || string.IsNullOrWhiteSpace(newPass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ mật khẩu cũ và mật khẩu mới!");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Lấy mật khẩu hiện tại
                SqlCommand cmdGet = new SqlCommand("SELECT Password FROM Account WHERE AccountName = @acc", conn);
                cmdGet.Parameters.AddWithValue("@acc", username);
                string currentPass = cmdGet.ExecuteScalar()?.ToString();

                // Kiểm tra mật khẩu cũ
                if (currentPass != oldPass)
                {
                    MessageBox.Show("Mật khẩu cũ không đúng!");
                    return;
                }

                // Cập nhật mật khẩu mới
                SqlCommand cmdUpdate = new SqlCommand("UPDATE Account SET Password = @newPass WHERE AccountName = @acc", conn);
                cmdUpdate.Parameters.AddWithValue("@newPass", newPass);
                cmdUpdate.Parameters.AddWithValue("@acc", username);
                cmdUpdate.ExecuteNonQuery();

                MessageBox.Show("Đổi mật khẩu thành công!");
            }

            ResetPasswordUI();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
           ResetPasswordUI();
        }

        private void ResetPasswordUI()
        {
            // Ẩn 2 ô mật khẩu và nút huỷ
            lblOldPassword.Visible = txtOldPassword.Visible = false;
            lblNewPassword.Visible = txtNewPassword.Visible = false;
            btnCancel.Visible = false;

            // Reset textbox
            txtOldPassword.Clear();
            txtNewPassword.Clear();

            // Reset trạng thái form
            txtPassword.Text = "********";
            txtPassword.ReadOnly = true;
            btnResetPassword.Text = "Đổi mật khẩu";
            isChangingPassword = false;
        }

        #endregion

        #region XỬ LÝ NÚT MẶC ĐỊNH (RESET FORM VỀ TRẠNG THÁI BAN ĐẦU)
        private void btnMacDinh_Click(object sender, EventArgs e)
        {
            // Xóa nội dung các ô nhập
            txtAccountName.Clear();
            txtFullName.Clear();
            txtEmail.Clear();
            txtTell.Clear();
            txtPassword.Clear();

            // Reset checkbox
            chkbActived.Checked = true;

            // Reset ngày tạo về hiện tại
            dtDateCreated.Value = DateTime.Now;

            // Bỏ chọn tất cả role
            for (int i = 0; i < clbRoles.Items.Count; i++)
                clbRoles.SetItemChecked(i, false);

            // Bật nhập lại mật khẩu
            txtPassword.ReadOnly = false;

            // Reset trạng thái đổi mật khẩu
            isChangingPassword = false;
            btnResetPassword.Text = "Đổi mật khẩu";

            // Focus lại vào ô tài khoản để tiện nhập
            txtAccountName.Focus();

            // Bỏ chọn DataGridView nếu đang chọn hàng nào
            if (dgvAccountManager.SelectedRows.Count > 0)
                dgvAccountManager.ClearSelection();
        }
        #endregion

        #region XỬ LÝ TOOL STRIP MENU ITEM ̣(XÓA VÀ XEM DANH SÁCH VAI TRÒ CỦA TÀI KHOẢN ĐƯỢC CHỌN)
        private void xóaTàiKhoảnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgvAccountManager.CurrentRow != null)
            {
                string accountName = dgvAccountManager.CurrentRow.Cells["AccountName"].Value.ToString();
                if (MessageBox.Show($"Bạn có chắc muốn xóa tài khoản '{accountName}' không?",
                                    "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    using (SqlConnection conn = new SqlConnection("server=.; database=Restaurant Management; Integrated Security=true"))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(@"
                    DELETE FROM RoleAccount WHERE AccountName = @AccountName;
                    DELETE FROM Account WHERE AccountName = @AccountName;", conn);
                        cmd.Parameters.AddWithValue("@AccountName", accountName);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Đã xóa tài khoản thành công!", "Thông báo");
                    LoadAccounts(); // gọi lại hàm load danh sách tài khoản
                }
            }
        }

        private void xemDanhSáchVaiTròToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgvAccountManager.CurrentRow != null)
            {
                string accountName = dgvAccountManager.CurrentRow.Cells["AccountName"].Value.ToString();
                RoleListForm roleListForm = new RoleListForm(accountName, connectionString);
                roleListForm.ShowDialog();
            }
        }
        #endregion

    }
}
