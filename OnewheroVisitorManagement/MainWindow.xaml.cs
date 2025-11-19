using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OnewheroVisitorManagement.Models;
using OnewheroVisitorManagement.Services;

namespace OnewheroVisitorManagement
{
    public partial class MainWindow : Window
    {
        private MongoDBService _dbService;
        private Event _editingEvent = null;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                _dbService = new MongoDBService();

                // Initialize placeholders for all TextBoxes
                InitializePlaceholders();

                // Load user profile info
                LoadUserProfile();

                LoadDashboardData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to database. Please check your connection string.\n\nError: {ex.Message}",
                    "Database Connection Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadUserProfile()
        {
            if (SessionManager.IsLoggedIn)
            {
                var user = SessionManager.CurrentUser;
                txtUserName.Text = user.FullName;
                txtUserRole.Text = user.Role;

                // Show/Hide admin button based on role
                btnAdmin.Visibility = SessionManager.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void InitializePlaceholders()
        {
            // Visitor form placeholders
            SetPlaceholder(txtFirstName, "First Name");
            SetPlaceholder(txtLastName, "Last Name");
            SetPlaceholder(txtEmail, "Email");
            SetPlaceholder(txtPhone, "Phone");
            SetPlaceholder(txtAddress, "Address");

            // Event form placeholders
            SetPlaceholder(txtEventName, "Event Name");
            SetPlaceholder(txtEventDescription, "Description");
            SetPlaceholder(txtCapacity, "Capacity");
            SetPlaceholder(txtTicketPrice, "Ticket Price ($)");

            // Booking form placeholders
            SetPlaceholder(txtNumTickets, "Number of Tickets");
        }

        private void SetPlaceholder(TextBox textBox, string placeholder)
        {
            if (textBox == null) return;

            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = placeholder;
                textBox.Foreground = Brushes.Gray;
            }

            textBox.GotFocus += (sender, e) =>
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = "";
                    textBox.Foreground = Brushes.Black;
                }
            };

            textBox.LostFocus += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.Foreground = Brushes.Gray;
                }
            };
        }

        private string GetTextBoxValue(TextBox textBox, string placeholder)
        {
            if (textBox.Text == placeholder || string.IsNullOrWhiteSpace(textBox.Text))
                return "";
            return textBox.Text.Trim();
        }

        // ==================== NAVIGATION ====================

        private void ShowView(Grid viewToShow)
        {
            DashboardView.Visibility = Visibility.Collapsed;
            VisitorsView.Visibility = Visibility.Collapsed;
            EventsView.Visibility = Visibility.Collapsed;
            BookingsView.Visibility = Visibility.Collapsed;
            AttractionsView.Visibility = Visibility.Collapsed;
            AnalyticsView.Visibility = Visibility.Collapsed;
            ProfileView.Visibility = Visibility.Collapsed;
            AdminView.Visibility = Visibility.Collapsed;

            viewToShow.Visibility = Visibility.Visible;
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            ShowView(DashboardView);
            LoadDashboardData();
        }

        private void btnVisitors_Click(object sender, RoutedEventArgs e)
        {
            ShowView(VisitorsView);
            LoadVisitors();
        }

        private void btnEvents_Click(object sender, RoutedEventArgs e)
        {
            ShowView(EventsView);
            LoadEvents();
        }

        private void btnBookings_Click(object sender, RoutedEventArgs e)
        {
            ShowView(BookingsView);
            LoadBookings();
            LoadBookingDropdowns();
        }

        private void btnAttractions_Click(object sender, RoutedEventArgs e)
        {
            ShowView(AttractionsView);
        }

        private void btnAnalytics_Click(object sender, RoutedEventArgs e)
        {
            ShowView(AnalyticsView);
            LoadAnalytics();
        }

        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            ShowView(ProfileView);
            LoadProfileView();
        }

        private void btnAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionManager.IsAdmin)
            {
                MessageBox.Show("Access Denied. Admin privileges required.", "Access Denied",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ShowView(AdminView);
            LoadAdminEvents();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SessionManager.Logout();
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to exit?", "Confirm Exit",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        // ==================== PROFILE VIEW ====================

        private void LoadProfileView()
        {
            if (SessionManager.IsLoggedIn)
            {
                var user = SessionManager.CurrentUser;
                txtProfileFullName.Text = user.FullName;
                txtProfileUsername.Text = user.Username;
                txtProfileEmail.Text = user.Email;
                txtProfileRole.Text = user.Role;
                txtProfileCreatedDate.Text = user.CreatedDate.ToString("dd MMMM yyyy");
            }
        }

        // ==================== ADMIN PANEL ====================

        private async void LoadAdminEvents()
        {
            try
            {
                var events = await _dbService.GetAllEventsAsync();
                dgAdminEvents.ItemsSource = events;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading events: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnRefreshAdminEvents_Click(object sender, RoutedEventArgs e)
        {
            LoadAdminEvents();
        }

        private async void btnEditEvent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                string eventId = button.Tag.ToString();

                var evt = await _dbService.GetEventByIdAsync(eventId);
                if (evt == null)
                {
                    MessageBox.Show("Event not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Create edit dialog
                Window editWindow = new Window
                {
                    Title = "Edit Event",
                    Width = 500,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Background = new SolidColorBrush(Color.FromRgb(245, 245, 245))
                };

                var grid = new Grid { Margin = new Thickness(20) };
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Title
                var title = new TextBlock
                {
                    Text = "Edit Event Details",
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 20)
                };
                Grid.SetRow(title, 0);
                grid.Children.Add(title);

                // Form
                var formStack = new StackPanel();
                Grid.SetRow(formStack, 1);

                var txtEditName = CreateLabeledTextBox("Event Name:", evt.EventName);
                var txtEditDescription = CreateLabeledTextBox("Description:", evt.Description);
                var dpEditDate = CreateLabeledDatePicker("Event Date:", evt.EventDate);
                var txtEditCapacity = CreateLabeledTextBox("Capacity:", evt.Capacity.ToString());
                var txtEditPrice = CreateLabeledTextBox("Ticket Price:", evt.TicketPrice.ToString());
                var chkEditActive = CreateLabeledCheckBox("Active:", evt.IsActive);

                formStack.Children.Add(txtEditName);
                formStack.Children.Add(txtEditDescription);
                formStack.Children.Add(dpEditDate);
                formStack.Children.Add(txtEditCapacity);
                formStack.Children.Add(txtEditPrice);
                formStack.Children.Add(chkEditActive);

                grid.Children.Add(formStack);

                // Buttons
                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                Grid.SetRow(buttonPanel, 2);

                var btnSave = new Button
                {
                    Content = "Save Changes",
                    Width = 120,
                    Height = 35,
                    Margin = new Thickness(0, 0, 10, 0),
                    Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                var btnCancel = new Button
                {
                    Content = "Cancel",
                    Width = 100,
                    Height = 35,
                    Background = new SolidColorBrush(Color.FromRgb(149, 165, 166)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                btnSave.Click += async (s, args) =>
                {
                    try
                    {
                        evt.EventName = ((TextBox)txtEditName.Children[1]).Text;
                        evt.Description = ((TextBox)txtEditDescription.Children[1]).Text;
                        evt.EventDate = ((DatePicker)dpEditDate.Children[1]).SelectedDate.Value;
                        evt.Capacity = int.Parse(((TextBox)txtEditCapacity.Children[1]).Text);
                        evt.TicketPrice = decimal.Parse(((TextBox)txtEditPrice.Children[1]).Text);
                        evt.IsActive = ((CheckBox)chkEditActive.Children[1]).IsChecked.Value;

                        bool success = await _dbService.UpdateEventAsync(eventId, evt);

                        if (success)
                        {
                            MessageBox.Show("Event updated successfully!", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            editWindow.Close();
                            LoadAdminEvents();
                            LoadDashboardData();
                        }
                        else
                        {
                            MessageBox.Show("Failed to update event.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error updating event: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };

                btnCancel.Click += (s, args) => editWindow.Close();

                buttonPanel.Children.Add(btnSave);
                buttonPanel.Children.Add(btnCancel);
                grid.Children.Add(buttonPanel);

                editWindow.Content = grid;
                editWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing event: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnDeleteEvent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                string eventId = button.Tag.ToString();

                var result = MessageBox.Show("Are you sure you want to delete this event?\n\nThis action cannot be undone!",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    bool success = await _dbService.DeleteEventAsync(eventId);

                    if (success)
                    {
                        MessageBox.Show("Event deleted successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadAdminEvents();
                        LoadDashboardData();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete event.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting event: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Helper methods for edit dialog
        private StackPanel CreateLabeledTextBox(string label, string value)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };
            panel.Children.Add(new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 5)
            });
            panel.Children.Add(new TextBox
            {
                Text = value,
                Height = 35,
                
                FontSize = 14
            });
            return panel;
        }

        private StackPanel CreateLabeledDatePicker(string label, DateTime value)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };
            panel.Children.Add(new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 5)
            });
            panel.Children.Add(new DatePicker
            {
                SelectedDate = value,
                Height = 35,
                FontSize = 14
            });
            return panel;
        }

        private StackPanel CreateLabeledCheckBox(string label, bool isChecked)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };
            panel.Children.Add(new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 5)
            });
            panel.Children.Add(new CheckBox
            {
                IsChecked = isChecked,
                Content = "Event is active"
            });
            return panel;
        }

        // ==================== DASHBOARD ====================

        private async void LoadDashboardData()
        {
            try
            {
                int totalVisitors = await _dbService.GetTotalVisitorCountAsync();
                var activeEvents = await _dbService.GetActiveEventsAsync();
                int totalBookings = await _dbService.GetTotalBookingsCountAsync();

                txtTotalVisitors.Text = totalVisitors.ToString();
                txtActiveEvents.Text = activeEvents.Count.ToString();
                txtTotalBookings.Text = totalBookings.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==================== VISITORS ====================

        private async void LoadVisitors()
        {
            try
            {
                var visitors = await _dbService.GetAllVisitorsAsync();
                dgVisitors.ItemsSource = visitors;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading visitors: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnAddVisitor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string firstName = GetTextBoxValue(txtFirstName, "First Name");
                string lastName = GetTextBoxValue(txtLastName, "Last Name");
                string email = GetTextBoxValue(txtEmail, "Email");
                string phone = GetTextBoxValue(txtPhone, "Phone");
                string address = GetTextBoxValue(txtAddress, "Address");

                if (string.IsNullOrWhiteSpace(firstName) ||
                    string.IsNullOrWhiteSpace(lastName) ||
                    string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show("Please fill in all required fields: First Name, Last Name, and Email",
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var visitor = new Visitor
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Phone = phone,
                    Address = address,
                    Interests = new List<string>()
                };

                if (chkMuseum.IsChecked == true) visitor.Interests.Add("Museum");
                if (chkKiwiHouse.IsChecked == true) visitor.Interests.Add("Kiwi House");
                if (chkBirds.IsChecked == true) visitor.Interests.Add("Native Birds");
                if (chkReptiles.IsChecked == true) visitor.Interests.Add("Reptiles");
                if (chkMarae.IsChecked == true) visitor.Interests.Add("Marae");

                string visitorId = await _dbService.AddVisitorAsync(visitor);

                MessageBox.Show($"Visitor '{visitor.FirstName} {visitor.LastName}' registered successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                ClearVisitorForm();
                LoadVisitors();
                LoadDashboardData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding visitor: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearVisitorForm()
        {
            SetPlaceholder(txtFirstName, "First Name");
            SetPlaceholder(txtLastName, "Last Name");
            SetPlaceholder(txtEmail, "Email");
            SetPlaceholder(txtPhone, "Phone");
            SetPlaceholder(txtAddress, "Address");

            txtFirstName.Text = "First Name";
            txtFirstName.Foreground = Brushes.Gray;
            txtLastName.Text = "Last Name";
            txtLastName.Foreground = Brushes.Gray;
            txtEmail.Text = "Email";
            txtEmail.Foreground = Brushes.Gray;
            txtPhone.Text = "Phone";
            txtPhone.Foreground = Brushes.Gray;
            txtAddress.Text = "Address";
            txtAddress.Foreground = Brushes.Gray;

            chkMuseum.IsChecked = false;
            chkKiwiHouse.IsChecked = false;
            chkBirds.IsChecked = false;
            chkReptiles.IsChecked = false;
            chkMarae.IsChecked = false;
        }

        // ==================== EVENTS ====================

        private async void LoadEvents()
        {
            try
            {
                var events = await _dbService.GetAllEventsAsync();
                dgEvents.ItemsSource = events;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading events: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnAddEvent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string eventName = GetTextBoxValue(txtEventName, "Event Name");
                string description = GetTextBoxValue(txtEventDescription, "Description");
                string capacityStr = GetTextBoxValue(txtCapacity, "Capacity");
                string ticketPriceStr = GetTextBoxValue(txtTicketPrice, "Ticket Price ($)");

                if (string.IsNullOrWhiteSpace(eventName) ||
                    dpEventDate.SelectedDate == null ||
                    cmbEventType.SelectedItem == null)
                {
                    MessageBox.Show("Please fill in Event Name, Date, and Type",
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int capacity = 0;
                decimal ticketPrice = 0;

                if (string.IsNullOrWhiteSpace(capacityStr) || !int.TryParse(capacityStr, out capacity) || capacity <= 0)
                {
                    MessageBox.Show("Please enter a valid capacity (positive number)",
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(ticketPriceStr) || !decimal.TryParse(ticketPriceStr, out ticketPrice) || ticketPrice < 0)
                {
                    MessageBox.Show("Please enter a valid ticket price (0 or greater)",
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var evt = new Event
                {
                    EventName = eventName,
                    Description = description,
                    EventDate = dpEventDate.SelectedDate.Value,
                    EventType = (cmbEventType.SelectedItem as ComboBoxItem).Content.ToString(),
                    Capacity = capacity,
                    AvailableSeats = capacity,
                    TicketPrice = ticketPrice,
                    IsActive = true
                };

                string eventId = await _dbService.AddEventAsync(evt);

                MessageBox.Show($"Event '{evt.EventName}' created successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                ClearEventForm();
                LoadEvents();
                LoadDashboardData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding event: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearEventForm()
        {
            txtEventName.Text = "Event Name";
            txtEventName.Foreground = Brushes.Gray;
            txtEventDescription.Text = "Description";
            txtEventDescription.Foreground = Brushes.Gray;
            txtCapacity.Text = "Capacity";
            txtCapacity.Foreground = Brushes.Gray;
            txtTicketPrice.Text = "Ticket Price ($)";
            txtTicketPrice.Foreground = Brushes.Gray;

            dpEventDate.SelectedDate = null;
            cmbEventType.SelectedIndex = -1;
        }

        // ==================== BOOKINGS ====================

        private async void LoadBookings()
        {
            try
            {
                var bookings = await _dbService.GetAllBookingsAsync();
                dgBookings.ItemsSource = bookings;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bookings: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadBookingDropdowns()
        {
            try
            {
                var visitors = await _dbService.GetAllVisitorsAsync();
                var events = await _dbService.GetAllEventsAsync();

                cmbVisitor.ItemsSource = visitors;
                cmbEvent.ItemsSource = events;

                if (visitors.Count == 0)
                {
                    MessageBox.Show("No visitors found! Please register visitors first in the Visitors section.",
                        "No Visitors", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                if (events.Count == 0)
                {
                    MessageBox.Show("No events found! Please create events first in the Events section.",
                        "No Events", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dropdown data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbEvent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTotalPrice();
        }

        private void txtNumTickets_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTotalPrice();
        }

        private void UpdateTotalPrice()
        {
            string numTicketsStr = GetTextBoxValue(txtNumTickets, "Number of Tickets");

            if (cmbEvent.SelectedItem != null && !string.IsNullOrWhiteSpace(numTicketsStr))
            {
                int numTickets = 0;
                if (int.TryParse(numTicketsStr, out numTickets) && numTickets > 0)
                {
                    var selectedEvent = cmbEvent.SelectedItem as Event;
                    decimal total = selectedEvent.TicketPrice * numTickets;
                    txtTotalPrice.Text = $"Total: ${total:F2}";
                }
                else
                {
                    txtTotalPrice.Text = "Total: $0.00";
                }
            }
            else
            {
                txtTotalPrice.Text = "Total: $0.00";
            }
        }

        private async void btnCreateBooking_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbVisitor.SelectedItem == null || cmbEvent.SelectedItem == null)
                {
                    MessageBox.Show("Please select both a visitor and an event",
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string numTicketsStr = GetTextBoxValue(txtNumTickets, "Number of Tickets");
                int numTickets = 0;

                if (string.IsNullOrWhiteSpace(numTicketsStr) || !int.TryParse(numTicketsStr, out numTickets) || numTickets <= 0)
                {
                    MessageBox.Show("Please enter a valid number of tickets (positive number)",
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var visitor = cmbVisitor.SelectedItem as Visitor;
                var evt = cmbEvent.SelectedItem as Event;

                if (evt.AvailableSeats < numTickets)
                {
                    MessageBox.Show($"Not enough seats available!\nOnly {evt.AvailableSeats} seat(s) remaining.",
                        "Insufficient Seats", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var booking = new Booking
                {
                    VisitorId = visitor.Id,
                    EventId = evt.Id,
                    NumberOfTickets = numTickets,
                    TotalAmount = evt.TicketPrice * numTickets
                };

                string bookingId = await _dbService.CreateBookingAsync(booking);

                if (bookingId != null)
                {
                    MessageBox.Show($"Booking created successfully!\n\nVisitor: {visitor.FirstName} {visitor.LastName}\nEvent: {evt.EventName}\nTickets: {numTickets}\nTotal: ${booking.TotalAmount:F2}",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    ClearBookingForm();
                    LoadBookings();
                    LoadEvents();
                    LoadDashboardData();
                }
                else
                {
                    MessageBox.Show("Failed to create booking. Please try again.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating booking: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearBookingForm()
        {
            cmbVisitor.SelectedIndex = -1;
            cmbEvent.SelectedIndex = -1;

            txtNumTickets.Text = "Number of Tickets";
            txtNumTickets.Foreground = Brushes.Gray;
            txtTotalPrice.Text = "Total: $0.00";
        }

        // ==================== ANALYTICS ====================

        private async void LoadAnalytics()
        {
            try
            {
                var demographics = await _dbService.GetVisitorDemographicsAsync();

                if (demographics.Count == 0)
                {
                    txtDemographics.Text = "No visitor data available yet.\n\nRegister some visitors with interests to see analytics here!";
                    return;
                }

                string result = "VISITOR INTERESTS BREAKDOWN\n";
                result += "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n";

                var sortedDemographics = demographics.OrderByDescending(x => x.Value).ToList();

                foreach (var item in sortedDemographics)
                {
                    result += $"🔹 {item.Key}: {item.Value} visitors\n";
                }

                result += $"\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n";
                result += $"Total Unique Interests: {demographics.Count}\n";

                txtDemographics.Text = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading analytics: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnRefreshAnalytics_Click(object sender, RoutedEventArgs e)
        {
            LoadAnalytics();
            MessageBox.Show("Analytics refreshed!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}