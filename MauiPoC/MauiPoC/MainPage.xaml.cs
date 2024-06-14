namespace MauiPoC
{
    public partial class MainPage : ContentPage
    {
        IConnectivity _connectivity;
        IGeolocation _geolocation;
        private readonly LocalDbService _dbService;
        private int _editCustomerId;
        public string ExampleValue { get; set; } = "TEST";


        public MainPage(LocalDbService dbService,
                        IConnectivity connectivity,
                        IGeolocation geolocation
                        )
        {
            InitializeComponent();
            _dbService = dbService;
            _connectivity = connectivity;
            _geolocation = geolocation;
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

        private async Task<double> GetClosestThing()
        {
            var location = await _geolocation.GetLocationAsync();

            if (location == null)
            {
                location = await _geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(30)
                });
            }

            var howFar = location.CalculateDistance(new Location(-22.4269, -45.4530), DistanceUnits.Kilometers);

            return (double)howFar;
        }

        private async void saveButton_Clicked(object sender, EventArgs e)
        {
            var howFar = await GetClosestThing();

            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.DisplayAlert("Uh oh", "No Intetnet", "OK");
                return;
            }

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
