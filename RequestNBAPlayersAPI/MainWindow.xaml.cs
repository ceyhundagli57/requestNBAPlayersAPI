using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RequestNBAPlayersAPI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string Url = "https://www.balldontlie.io/api/v1/players";
        private readonly HttpClient _httpClient;
        private int CurrentPage = 1;
        private int TotalPageNo = 2;
        private List<string> Positions;
        private string SelectedPosition;

        public event PropertyChangedEventHandler PropertyChanged;


        public MainWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            Positions = new List<string>();
            DataContext = this;
            LoadData();

        }

        public class Player
        {
            public int Id { get; set; }
            public string First_name { get; set; }
            public string Last_name { get; set; }
            public string Position { get; set; }

            public string FullName
            {
                get { return $"{First_name} {Last_name}"; }
            }
        }

        public List<Player> Players { get; set; }
        public class PlayerList
        {
            public List<Player> Data { get; set; }
            public MetaData Meta { get; set; }
        }
        public List<string> AllPositions
        {
            get { return Positions; }
            set
            {
                Positions = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AllPositions)));
            }
        }
        private void FilterPlayers()
        {
            if (PositionOfPlayer == "All players")
            {
                ListBoxPlayers.ItemsSource = Players;
            }
            else
            {
                var filteredPlayers = Players.Where(p => p.Position == PositionOfPlayer);
                ListBoxPlayers.ItemsSource = filteredPlayers;
            }

            ListBoxPlayers.DisplayMemberPath = "FullName";
        }
        public string PositionOfPlayer
        {
            get { return SelectedPosition; }
            set
            {
                SelectedPosition = value;
                FilterPlayers();
            }
        }
        public class MetaData
        {
            public int TotalPages { get; set; }
        }
        private async Task LoadData()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{Url}?page={CurrentPage}");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PlayerList>(json);
                Players = result.Data;
                TotalPageNo = result.Meta.TotalPages;
                AllPositions = Players.Select(p => p.Position).Distinct().ToList();
                AllPositions.Add("All players");
                PositionOfPlayer = "All players";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            LabelPageNo.Content = CurrentPage.ToString();
            ComboBoxPosition.ItemsSource = AllPositions;
            ComboBoxPosition.SetBinding(Selector.SelectedItemProperty, new Binding("PositionOfPlayer") { Mode = BindingMode.TwoWay });
            if (CurrentPage == 1)
            {
                ButtonPrevious.Visibility = Visibility.Hidden;
            }
            else if (CurrentPage > 1)
            {
                ButtonPrevious.Visibility = Visibility.Visible;
            }
        }


        private async void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage++;
            await LoadData();
            FilterPlayers();
        }

        private async void ButtonPrevious_Click_1(object sender, RoutedEventArgs e)
        {
            CurrentPage--;
            await LoadData();
            FilterPlayers();
        }

        

       
    }
}
