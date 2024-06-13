namespace MauiPoC
{
    public partial class MainPage : ContentPage
    {
        private readonly LocalDbService _dbService;
        private int _editCustomerId;
        public string ExampleValue { get; set; } = "TEST";


        public MainPage(LocalDbService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            LoadCustomers();
        }

        private async void LoadCustomers()
        {
            var customers = await _dbService.GetCustomers();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                listView.ItemsSource = customers;
            });
        }

        private async void saveButton_Clicked(object sender, EventArgs e)
        {
            if (_editCustomerId == 0)
            {
                // Add customer
                await _dbService.Create(new Customer
                {
                    CustomerName = nameEntryField.Text,
                    Email = emailEntryField.Text,
                    Mobile = mobileEntryField.Text
                });
            }
            else
            {
                // Edit customer
                await _dbService.Update(new Customer
                {
                    Id = _editCustomerId,
                    CustomerName = nameEntryField.Text,
                    Email = emailEntryField.Text,
                    Mobile = mobileEntryField.Text
                });
                _editCustomerId = 0;
            }

            // Clear fields
            nameEntryField.Text = string.Empty;
            emailEntryField.Text = string.Empty;
            mobileEntryField.Text = string.Empty;

            // Reload customers
            LoadCustomers();
        }

        private async void listView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var customer = (Customer)e.Item;
            var action = await DisplayActionSheet("Action", "Cancel", null, "Edit", "Delete");

            switch (action)
            {
                case "Edit":
                    _editCustomerId = customer.Id;
                    nameEntryField.Text = customer.CustomerName;
                    emailEntryField.Text = customer.Email;
                    mobileEntryField.Text = customer.Mobile;
                    break;
                case "Delete":
                    await _dbService.Delete(customer);
                    LoadCustomers();
                    break;
            }
        }
    }
}
