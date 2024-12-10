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
using System.IO;

namespace ORDERINGSYSTEM
{
    public partial class AdminForm : Form
    {
        public AdminForm()
        {
            InitializeComponent();
            InitializeNavBar();
        }

        private void AdminForm_Load(object sender, EventArgs e)
        {
            LoadProducts();
            LoadOrders();
        }
        private void InitializeNavBar()
        {
            // Create a Panel for the navbar
            Panel navbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,  // Set the height of the navbar
                BackColor = Color.FromArgb(255, 165, 0) // Custom light orange color
            };

            // Create the "Back to Login" button
            Button btnBackToLogin = new Button
            {
                Text = "Back to Login",
                Width = 150, // Set width for oblong shape
                Height = 40, // Set height for oblong shape
                BackColor = Color.Yellow, // Button background color
                ForeColor = Color.Black, // Text color
                FlatStyle = FlatStyle.Flat, // Flat button style for clean look
                FlatAppearance = { BorderSize = 0 }, // Remove button border
                Cursor = Cursors.Hand // Change cursor to hand when hovering over button
            };

            // Set button's corner radius (oblong look)
            btnBackToLogin.Region = new Region(new Rectangle(0, 0, btnBackToLogin.Width, btnBackToLogin.Height));
            btnBackToLogin.Click += BackToLogin_Click;

            // Set the location of the button to top-right corner
            btnBackToLogin.Location = new Point(navbarPanel.Width - btnBackToLogin.Width - 10, 5);

            // Handle hover events to change color
            btnBackToLogin.MouseEnter += (sender, e) =>
            {
                btnBackToLogin.BackColor = Color.Orange; // Change color on hover
            };
            btnBackToLogin.MouseLeave += (sender, e) =>
            {
                btnBackToLogin.BackColor = Color.Yellow; // Reset color when hover ends
            };

            // Add the button to the panel
            navbarPanel.Controls.Add(btnBackToLogin);

            // Create "Currently logged in as" label
            Label lblCurrentUser = new Label
            {
                Text = $"Logged in as: {LoginForm.currentUsername}",
                ForeColor = Color.Black,  // Text color
                Font = new Font("Arial", 12, FontStyle.Bold),  // Font settings
                AutoSize = true,  // Auto-size the label to fit text
                Location = new Point(btnBackToLogin.Left - 10 - 200, 15)  // Position left of the button
            };

            // Add the label to the panel
            navbarPanel.Controls.Add(lblCurrentUser);

            // Add the navbar panel to the form
            this.Controls.Add(navbarPanel);
        }

        private void BackToLogin_Click(object sender, EventArgs e)
        {
            // Navigate to the login form
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
            this.Close();
        }

        // Other methods (like AdminForm_Load) would go here...


        private void LoadProducts()
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                string query = "SELECT * FROM Products";
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable table = new DataTable();
                adapter.Fill(table);
                dgvProducts.DataSource = table;
            }
        }
        
        private void LoadOrders()
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                string query = "SELECT OrderID, UserID, Address, PhoneNumber, TotalAmount FROM Orders";
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable table = new DataTable();
                adapter.Fill(table);
                dgvOrders.DataSource = table; // dgvOrders is the DataGridView for orders
            }
        }



        private byte[] ConvertImageToByteArray(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }

        private string txtImagePath;

        private void btnDeleteProduct_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                int productID = Convert.ToInt32(dgvProducts.SelectedRows[0].Cells["ProductID"].Value);
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    string checkQuery = "SELECT COUNT(*) FROM Cart WHERE ProductID = @ProductID";
                    SqlCommand cmdCheck = new SqlCommand(checkQuery, conn);
                    cmdCheck.Parameters.AddWithValue("@ProductID", productID);

                    conn.Open();
                    int count = (int)cmdCheck.ExecuteScalar();

                    if (count > 0)
                    {
                        MessageBox.Show("This product is still in some carts and cannot be deleted.");
                    }
                    else
                    {
                        string deleteQuery = "DELETE FROM Products WHERE ProductID = @ProductID";
                        SqlCommand cmdDelete = new SqlCommand(deleteQuery, conn);
                        cmdDelete.Parameters.AddWithValue("@ProductID", productID);
                        cmdDelete.ExecuteNonQuery();

                        MessageBox.Show("Product deleted successfully!");
                        LoadProducts(); // Refresh product list
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a product to delete.");
            }
        }

        private void btnUploadImage_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    picProductImage.Image = Image.FromFile(openFileDialog.FileName);
                    txtImagePath = openFileDialog.FileName; // Add a hidden TextBox to store the path
                }
            }
        }

        private void btnAddProduct_Click_1(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    string query = "INSERT INTO Products (Name, Description, Price, ImagePath) VALUES (@Name, @Description, @Price, @ImagePath)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Name", txtProductName.Text);
                    cmd.Parameters.AddWithValue("@Description", txtDescription.Text);
                    cmd.Parameters.AddWithValue("@Price", decimal.Parse(txtPrice.Text));
                    cmd.Parameters.AddWithValue("@ImagePath", txtImagePath);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Product added successfully!");
                    LoadProducts(); // Refresh product list
                }
            }
            catch (Exception ) { MessageBox.Show("Missing one or more arguments!"); }

            }

        private void btnMarkAsDone_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count > 0)
            {
                int orderId = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["OrderID"].Value);

                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    string query = "DELETE FROM Orders WHERE OrderID = @OrderID";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@OrderID", orderId);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Order marked as done and removed from the list.");
                    LoadOrders(); // Refresh order list
                }
            }
            else
            {
                MessageBox.Show("Please select an order to mark as done.");
            }
        }
    }
}
