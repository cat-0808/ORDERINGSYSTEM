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

namespace ORDERINGSYSTEM
{
    public partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();
            InitializeNavBar();
        }
        private void InitializeNavBar()
        {
            MenuStrip menuStrip = new MenuStrip();

            // Add "User" menu
            ToolStripMenuItem userMenu = new ToolStripMenuItem("User");
            userMenu.DropDownItems.Add("Back to Login", null, BackToLogin_Click);
            

            // Add current user label
            ToolStripMenuItem currentUser = new ToolStripMenuItem($"Signed in as: {LoginForm.currentUsername}");
            currentUser.Enabled = false;

            menuStrip.Items.Add(userMenu);
            menuStrip.Items.Add(currentUser);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
            menuStrip.Dock = DockStyle.Top;
        }

        private void BackToLogin_Click(object sender, EventArgs e)
        {
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
            this.Close();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            TextBox txtUsername = (TextBox)this.Controls["txtUsername"];
            TextBox txtPassword = (TextBox)this.Controls["txtPassword"];
            TextBox txtConfirmPassword = (TextBox)this.Controls["txtConfirmPassword"];

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("All fields are required.");
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                string query = "INSERT INTO Users (Username, Password, Role) VALUES (@Username, @Password, 'User')";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Registration successful!");
                    this.Close();
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Unique constraint violation
                    {
                        MessageBox.Show("Username already exists. Please choose a different username.");
                    }
                    else
                    {
                        MessageBox.Show($"An error occurred: {ex.Message}");
                    }
                }
            }
        }

        private void RegisterForm_Load(object sender, EventArgs e)
        {

        }
    }
}
