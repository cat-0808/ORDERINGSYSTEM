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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ORDERINGSYSTEM
{
    public partial class LoginForm : Form
    {
        public static int currentUserID;
        public LoginForm()
        {
            InitializeComponent();
            InitializeNavBar();
        }
        public static string currentUsername;
        public static int currentUserId { get; set; }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            cmbRole.Items.Add("Admin");
            cmbRole.Items.Add("User");
        }

        private void InitializeNavBar()
        {
            MenuStrip menuStrip = new MenuStrip();

            // Add "User" menu
            ToolStripMenuItem userMenu = new ToolStripMenuItem("User");
            userMenu.DropDownItems.Add("Back to Login", null, BackToLogin_Click);
            userMenu.DropDownItems.Add("Register", null, OpenRegisterForm_Click);

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

        private void OpenRegisterForm_Click(object sender, EventArgs e)
        {
            RegisterForm registerForm = new RegisterForm();
            registerForm.Show();
        }
        private void btnLogin_Click_1(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();
            string role = cmbRole.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
            {
                MessageBox.Show("Please fill out all fields and select a role.");
                return;
            }

            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    string query = "SELECT UserID, Role FROM Users WHERE Username = @Username AND Password = @Password AND Role = @Role";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password);
                    cmd.Parameters.AddWithValue("@Role", role);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        currentUserID = Convert.ToInt32(reader["UserID"]);
                        MessageBox.Show($"Welcome, {username}!");

                        // Redirect based on role
                        if (role == "Admin")
                        {
                            AdminForm adminForm = new AdminForm();
                            adminForm.Show();
                        }
                        else if (role == "User")
                        {
                            UserForm userForm = new UserForm();
                            userForm.Show();
                        }
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("Invalid login credentials. Please try again.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void btnLoginAsGuest_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                string query = @"
                INSERT INTO Users (Username, Password, Role) 
                OUTPUT INSERTED.UserID 
                VALUES (@Username, '', 'Guest')";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", $"Guest_{Guid.NewGuid().ToString().Substring(0, 8)}");

                conn.Open();
                currentUserID = (int)cmd.ExecuteScalar();
            }

            MessageBox.Show("You are now logged in as a guest.");

            // Redirect to the user form
            UserForm userForm = new UserForm();
            userForm.Show();
            this.Hide();    
        }
    }
    }
    
    


