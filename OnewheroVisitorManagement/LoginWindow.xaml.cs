using System;
using System.Windows;
using System.Windows.Input;
using OnewheroVisitorManagement.Services;

namespace OnewheroVisitorManagement
{
    public partial class LoginWindow : Window
    {
        private MongoDBService _dbService;

        public LoginWindow()
        {
            InitializeComponent();
            _dbService = new MongoDBService();

            CreateDefaultAdminUser();
        }

        private async void CreateDefaultAdminUser()
        {
            try
            {
                var users = await _dbService.GetAllUsersAsync();
                if (users.Count == 0)
                {
                    var adminUser = new Models.User
                    {
                        Username = "admin",
                        Password = "admin123",
                        Email = "admin@onewhero.com",
                        FullName = "System Administrator",
                        Role = "Admin"
                    };
                    await _dbService.AddUserAsync(adminUser);
                }
            }
            catch (Exception ex)
            {
                txtLoginStatus.Text = $"Error initializing: {ex.Message}";
            }
        }

        // ==================== LOGIN ====================

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            await PerformLogin();
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformLogin();
            }
        }

        private async System.Threading.Tasks.Task PerformLogin()
        {
            try
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Password;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    txtLoginStatus.Text = "Please enter username and password";
                    return;
                }

                txtLoginStatus.Text = "Logging in...";
                txtLoginStatus.Foreground = System.Windows.Media.Brushes.Gray;
                btnLogin.IsEnabled = false;

                var user = await _dbService.AuthenticateUserAsync(username, password);

                if (user != null)
                {
                    SessionManager.CurrentUser = user;

                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    txtLoginStatus.Foreground = System.Windows.Media.Brushes.Red;
                    txtLoginStatus.Text = "Invalid username or password";
                    txtPassword.Password = "";
                }
            }
            catch (Exception ex)
            {
                txtLoginStatus.Foreground = System.Windows.Media.Brushes.Red;
                txtLoginStatus.Text = $"Login error: {ex.Message}";
            }
            finally
            {
                btnLogin.IsEnabled = true;
            }
        }

        // ==================== REGISTER ====================

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fullName = txtRegFullName.Text.Trim();
                string username = txtRegUsername.Text.Trim();
                string email = txtRegEmail.Text.Trim();
                string password = txtRegPassword.Password;
                string confirmPassword = txtRegConfirmPassword.Password;

                // Validation
                if (string.IsNullOrWhiteSpace(fullName) ||
                    string.IsNullOrWhiteSpace(username) ||
                    string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(password))
                {
                    txtRegisterStatus.Foreground = System.Windows.Media.Brushes.Red;
                    txtRegisterStatus.Text = "Please fill in all fields";
                    return;
                }

                if (password != confirmPassword)
                {
                    txtRegisterStatus.Foreground = System.Windows.Media.Brushes.Red;
                    txtRegisterStatus.Text = "Passwords do not match";
                    return;
                }

                if (password.Length < 6)
                {
                    txtRegisterStatus.Foreground = System.Windows.Media.Brushes.Red;
                    txtRegisterStatus.Text = "Password must be at least 6 characters";
                    return;
                }

                // Check if username already exists
                bool exists = await _dbService.UsernameExistsAsync(username);
                if (exists)
                {
                    txtRegisterStatus.Foreground = System.Windows.Media.Brushes.Red;
                    txtRegisterStatus.Text = "Username already exists";
                    return;
                }

                txtRegisterStatus.Text = "Creating account...";
                txtRegisterStatus.Foreground = System.Windows.Media.Brushes.Gray;
                btnRegister.IsEnabled = false;

                // Create new user (Staff role by default)
                var newUser = new Models.User
                {
                    Username = username,
                    Password = password,
                    Email = email,
                    FullName = fullName,
                    Role = "Staff" // Regular users get Staff role
                };

                string userId = await _dbService.AddUserAsync(newUser);

                if (!string.IsNullOrEmpty(userId))
                {
                    txtRegisterStatus.Foreground = System.Windows.Media.Brushes.Green;
                    txtRegisterStatus.Text = "Account created! You can now login.";

                    // Clear form
                    txtRegFullName.Clear();
                    txtRegUsername.Clear();
                    txtRegEmail.Clear();
                    txtRegPassword.Clear();
                    txtRegConfirmPassword.Clear();

                    // Focus on login username
                    txtUsername.Focus();
                }
                else
                {
                    txtRegisterStatus.Foreground = System.Windows.Media.Brushes.Red;
                    txtRegisterStatus.Text = "Failed to create account. Please try again.";
                }
            }
            catch (Exception ex)
            {
                txtRegisterStatus.Foreground = System.Windows.Media.Brushes.Red;
                txtRegisterStatus.Text = $"Error: {ex.Message}";
            }
            finally
            {
                btnRegister.IsEnabled = true;
            }
        }
    }
}