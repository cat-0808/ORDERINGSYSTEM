using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ORDERINGSYSTEM
{
    public partial class UserForm : Form
    {
        private List<(int ProductID, string Name, decimal Price, int Quantity)> cart = new List<(int, string, decimal, int)>();

        public UserForm()
        {
            InitializeComponent();
            InitializeNavBar();
        }

        private void UserForm_Load(object sender, EventArgs e)
        {
            // Load the products into the FlowLayoutPanel
            LoadProducts();
            UpdateTotalAmount(); // Ensure the total is 0 at the start
            lstCurrentProducts.Items.Clear();
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

        private void LoadProducts()
        {
            flpProducts.Controls.Clear();   

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                string query = "SELECT ProductID, Name, Price, Description, ImagePath FROM Products";
                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int productId = Convert.ToInt32(reader["ProductID"]);
                    string productName = reader["Name"].ToString();
                    decimal price = Convert.ToDecimal(reader["Price"]);
                    string description = reader["Description"].ToString();
                    string imagePath = reader["ImagePath"].ToString();

                    // Create a panel for the product
                    Panel productPanel = new Panel
                    {
                        Width = 200,
                        Height = 400,
                        BorderStyle = BorderStyle.FixedSingle,
                        Margin = new Padding(10)
                    };

                    // Add product image
                    PictureBox pictureBox = new PictureBox
                    {
                        Width = 180,
                        Height = 180,
                        ImageLocation = imagePath,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Dock = DockStyle.Top
                    };
                    productPanel.Controls.Add(pictureBox);

                    // Add product name
                    Label lblName = new Label
                    {
                        Text = productName,
                        AutoSize = false,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Top,
                        Font = new Font("Arial", 10, FontStyle.Bold)
                    };
                    productPanel.Controls.Add(lblName);

                    // Add product price
                    Label lblPrice = new Label
                    {
                        Text = $"Price: ₱{price}",
                        AutoSize = false,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Top,
                        Font = new Font("Arial", 10)
                    };
                    productPanel.Controls.Add(lblPrice);

                    // Add product description
                    Label lblDescription = new Label
                    {
                        Text = description,
                        AutoSize = false,
                        TextAlign = ContentAlignment.TopLeft,
                        Dock = DockStyle.Top,
                        Font = new Font("Arial", 8),
                        Padding = new Padding(5),
                        MaximumSize = new Size(180, 60),
                        AutoEllipsis = true
                    };
                    productPanel.Controls.Add(lblDescription);

                    // Add quantity input
                    Label lblQuantity = new Label
                    {
                        Text = "Quantity:",
                        Dock = DockStyle.Top,
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    NumericUpDown numQuantity = new NumericUpDown
                    {
                        Minimum = 1,
                        Maximum = 100,
                        Value = 1,
                        Tag = productId, // Store the ProductID
                        Dock = DockStyle.Top
                    };
                    productPanel.Controls.Add(lblQuantity);
                    productPanel.Controls.Add(numQuantity);

                    // Add "Add to Cart" button
                    Button btnAddToCart = new Button
                    {
                        Text = "Add to Cart",
                        Tag = new { ProductID = productId, ProductName = productName, Price = price, QuantityControl = numQuantity }, // Pass NumericUpDown for quantity
                        Dock = DockStyle.Bottom
                    };
                    btnAddToCart.Click += BtnAddToCart_Click;
                    productPanel.Controls.Add(btnAddToCart);

                    // Add the product panel to the FlowLayoutPanel
                    flpProducts.Controls.Add(productPanel);
                }
            }
        }
            

        private void BtnAddToCart_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            dynamic data = btn.Tag; // Retrieve the Tag object
            int productId = data.ProductID;
            string productName = data.ProductName;
            decimal price = data.Price;
            NumericUpDown numQuantity = data.QuantityControl;

            int quantity = (int)numQuantity.Value;

            AddToCart(productId, productName, price, quantity);
            LoadProducts(lstCurrentProducts);
            UpdateCartListBox();

        }

        private void AddToCart(int productId, string name, decimal price, int quantity)
        {
            // Add the product to the cart
            cart.Add((productId, name, price, quantity));

            // Update the total amount
            UpdateTotalAmount();
           
        }
        private void AddToCart(int productId)
        {
            // Code to add product to cart
            // For example, inserting into a Cart table in the database
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                string query = "INSERT INTO Cart (ProductID, UserID) VALUES (@ProductID, @UserID)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ProductID", productId);
                cmd.Parameters.AddWithValue("@UserID", LoginForm.currentUserId);  // Assuming currentUserId is available

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            // Clear and reload the products after adding to the cart
            lstCurrentProducts.Items.Clear();
            LoadProducts(lstCurrentProducts);
        }
        private void UpdateCartListBox()
        {
            // Clear the existing items in the ListBox
            lstCurrentProducts.Items.Clear();

            // Add each item from the cart to the ListBox
            foreach (var item in cart)
            {
                string productInfo = $"{item.Name} - ₱{item.Price} x {item.Quantity}";
                lstCurrentProducts.Items.Add(productInfo);
            }
        }



        private void LoadProducts(ListBox lstCurrentProducts)
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                string query = "SELECT Name, Price FROM Products";
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable table = new DataTable();
                adapter.Fill(table);

                // Clear existing items before adding new ones
                lstCurrentProducts.Items.Clear();

                // Add each product to the ListBox with its name and price
                foreach (DataRow row in table.Rows)
                {
                    string productInfo = $"{row["Name"]} - ₱{row["Price"]}";
                    lstCurrentProducts.Items.Add(productInfo);
                }
            }
        }
        private void UpdateTotalAmount()
        {
            decimal totalAmount = cart.Sum(item => item.Price * item.Quantity);
            lblTotal.Text = $"Total: ₱{totalAmount}";
        }

        private void btnCheckout_Click_1(object sender, EventArgs e)
        {
            if (cart.Count == 0)
            {
                MessageBox.Show("Your cart is empty!");
                return;
            }

            if (string.IsNullOrEmpty(txtAddress.Text) || string.IsNullOrEmpty(txtPhoneNumber.Text))
            {
                MessageBox.Show("Please provide an address and phone number.");
                return;
            }

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                string query = "INSERT INTO Orders (UserID, Address, PhoneNumber, TotalAmount) OUTPUT INSERTED.OrderID VALUES (@UserID, @Address, @PhoneNumber, @TotalAmount)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserID", LoginForm.currentUserID);
                cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                cmd.Parameters.AddWithValue("@PhoneNumber", txtPhoneNumber.Text);
                cmd.Parameters.AddWithValue("@TotalAmount", cart.Sum(item => item.Price * item.Quantity));

                conn.Open();
                int orderID = (int)cmd.ExecuteScalar();

                MessageBox.Show($"Order placed successfully! Order ID: {orderID}");
                cart.Clear();
                UpdateTotalAmount();
                lstCurrentProducts.Items.Clear();
            }
        }
        private void flpProducts_Paint(object sender, PaintEventArgs e)
        {

        }
    }

    


    }

