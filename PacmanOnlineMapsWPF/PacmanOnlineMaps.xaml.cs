using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace PacmanOnlineMaps
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public record SearchedWorld(string world_id, string title, string[] tags, byte[] preview);
    public record World(string world_id, byte[] world_map, string title, string[] tags, byte[] preview) : SearchedWorld(world_id, title, tags, preview);
    public record SearchReq(string q, string[] tags);
    public record Constants(int MIN_TITLE_LEN, int MAX_TITLE_LEN);
    public record Response<ResultT>(bool ok, ResultT result, string? reason = null);

    class Api
    {
        public static string HOST = "http://localhost:8000/";
        public static Api INSTANCE { get; } = new();
        private HttpClient httpClient = new();
        public Constants Constants { get; private set; }


        private Api()
        {
            this.Constants = this.GetConstants();
        }


        private void HandleResponse<T>(Response<T> response)
        {

        }


        public SearchedWorld[] GetRandomWorlds()
        {
            var raw_reply = this.httpClient.PostAsync(Api.HOST + "get_random_worlds", new StringContent(string.Empty)).Result;

            var response = JsonSerializer.Deserialize<Response<SearchedWorld[]>>(raw_reply.Content.ReadAsStringAsync().Result)!;
            this.HandleResponse<SearchedWorld[]>(response);

            return response.result;
        }

        public SearchedWorld[] SearchWorlds(SearchReq searchReq, int offset)
        {
            var raw_msg = JsonSerializer.Serialize(searchReq)!;
            raw_msg = raw_msg.Substring(0, raw_msg.Length - 1) + $", \"offset\": {offset}}}";

            var msg = new StringContent(raw_msg, Encoding.UTF8, "application/json");

            var raw_reply = this.httpClient.PostAsync(Api.HOST + "search_worlds", msg).Result;

            var response = JsonSerializer.Deserialize<Response<SearchedWorld[]>>(raw_reply.Content.ReadAsStringAsync().Result)!;
            this.HandleResponse<SearchedWorld[]>(response);

            return response.result;
        }

        public byte[] GetWorld(string world_id)
        {
            var raw_reply = this.httpClient.GetAsync(Api.HOST + "get_world?world_id=" + world_id).Result;

            var response = JsonSerializer.Deserialize<Response<byte[]>>(raw_reply.Content.ReadAsStringAsync().Result)!;
            this.HandleResponse<byte[]>(response);

            return response.result;
        }

        public Constants GetConstants()
        {
            var raw_reply = this.httpClient.GetAsync(Api.HOST + "get_constants").Result;

            var response = JsonSerializer.Deserialize<Response<Constants>>(raw_reply.Content.ReadAsStringAsync().Result)!;
            this.HandleResponse<Constants>(response);

            this.Constants = response.result;
            return response.result;
        }

        public async Task<Constants> GetConstantsAsync()
        {
            var raw_reply = await this.httpClient.GetAsync(Api.HOST + "get_constants");
            var body = await raw_reply.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<Response<Constants>>(body)!;
            this.HandleResponse<Constants>(response);

            this.Constants = response.result;
            return response.result;
        }
    }

    public partial class PlugWindow : Window
    {
        public static string OUTPUT_PATH = "worlds";

        private int _offset = 0;
        private int Offset
        {
            get => _offset;
            set
            {
                this.page.Content = value + 1;
                this._offset = value;
            }
        }

        private Func<int, SearchedWorld[]>? last_call = null;

        public PlugWindow()
        {
            InitializeComponent();
            // this.Loaded += (s, e) => 
            this.FillGrid(Api.INSTANCE.GetRandomWorlds());
            var dp = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMinutes(5)
            };
            dp.Tick += async (s, e) => {
                this.report_label.Content = "Updating constants...";
                this.UpdateLayout();
                await Api.INSTANCE.GetConstantsAsync();
                this.report_label.Content = "Updating constants updated";
            };
            this.Closed += (s, e) => dp.Stop();
            dp.Start();
        }

        public void FillGrid(SearchedWorld[] worlds)
        {
            this.grid.Children.Clear();
            /*
            <StackPanel Grid.Row="0" Grid.Column="0" Margin="8">
                <Rectangle Fill="Red" Height="80"></Rectangle>
                <Label Content="Title" Foreground="Yellow"></Label>
                <Label Content="Hash: " Foreground="Yellow" FontSize="5"></Label>
            </StackPanel>             
             */
            Grid gr;
            DockPanel wp;
            BitmapImage img;
            int r = 0;
            int c = 0;
            int cl = 0;

            foreach (var world in worlds)
            {
                img = new();
                img.BeginInit();
                img.StreamSource = new MemoryStream(world.preview);
                img.EndInit();


                wp = new();
                wp.BeginInit();

                wp.Background = new SolidColorBrush(cl++ % 2 == 0 ? Colors.Red : Colors.Blue);
                gr = new();
                gr.BeginInit();
                gr.RowDefinitions.Add(new RowDefinition());
                gr.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(32) });
                gr.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(32) });
                gr.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(32) });

                gr.ColumnDefinitions.Add(new ColumnDefinition());
                gr.ColumnDefinitions.Add(new ColumnDefinition());

                gr.Children.Add(new Rectangle()
                {
                    Fill = new ImageBrush()
                    {
                        ImageSource = img,
                        Stretch = Stretch.Uniform
                    },
                });
                Grid.SetRow(gr.Children[0], 0);
                Grid.SetColumnSpan(gr.Children[0], 2);

                gr.Children.Add(new Label()
                {
                    Content = world.title,
                    FontSize = 16,
                    Foreground = new SolidColorBrush(Colors.Yellow)
                });
                Grid.SetRow(gr.Children[1], 1);

                gr.Children.Add(new Rectangle()
                {
                    Width = 32,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 0, 8, 0),
                    Fill = new ImageBrush()
                    {
                        ImageSource = new BitmapImage(new Uri(@"Images\download.png", UriKind.Relative))
                    }
                });
                gr.Children[2].MouseLeftButtonDown += (s, e) => Download(world.world_id, world.title);
                Grid.SetRow(gr.Children[2], 1);
                Grid.SetColumn(gr.Children[2], 1);

                gr.Children.Add(new Label()
                {
                    Content = "hash: " + world.world_id,
                    FontSize = 8,
                    Foreground = new SolidColorBrush(Colors.Yellow)
                });
                Grid.SetRow(gr.Children[3], 2);
                Grid.SetColumnSpan(gr.Children[3], 2);

                gr.Children.Add(new Label()
                {
                    Content = "Tags: " + string.Join("; ", world.tags),
                    Foreground = new SolidColorBrush(Colors.Yellow)
                });
                Grid.SetRow(gr.Children[4], 3);
                Grid.SetColumnSpan(gr.Children[4], 2);

                gr.EndInit();
                wp.Children.Add(gr);
                wp.EndInit();
                this.grid.Children.Add(wp);

                Grid.SetRow(wp, r);
                Grid.SetColumn(wp, c);

                if (++c == 5)
                {
                    c = 0;
                    r++;
                }
            }
        }

        private void Download(string world_id, string title)
        {
            if (!Directory.Exists(PlugWindow.OUTPUT_PATH))
                Directory.CreateDirectory(PlugWindow.OUTPUT_PATH);

            this.report_label.Content = $"Downloading {title} - ({world_id})...";
            this.UpdateLayout();
            using (var fs = new BinaryWriter(new FileStream(PlugWindow.OUTPUT_PATH + "/" + title + ".world", FileMode.OpenOrCreate, FileAccess.Write)))
                fs.Write(Api.INSTANCE.GetWorld(world_id));

            this.report_label.Content = $"{title} - ({world_id}) downloaded";
        }

        private void search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.search.Text == "")
                this.search_placeholder.Visibility = Visibility.Visible;
            else if (this.search_placeholder.Visibility is Visibility.Visible)
                this.search_placeholder.Visibility = Visibility.Hidden;
        }

        private void search_tags_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.search_tags.Text == "")
                this.search_tags_placeholder.Visibility = Visibility.Visible;
            else if (this.search_tags.Visibility is Visibility.Visible)
                this.search_tags_placeholder.Visibility = Visibility.Hidden;
        }


        private void DoSearch(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(this.search.Text))
            {
                this.last_call = null;
                this.FillGrid(Api.INSTANCE.GetRandomWorlds());
                return;
            }

            if (this.search.Text.Length > Api.INSTANCE.Constants.MAX_TITLE_LEN || this.search.Text.Length < Api.INSTANCE.Constants.MIN_TITLE_LEN)
            {
                MessageBox.Show("Testo troppo lungo/corto");
                return;
            }

            string[] tags;
            this.report_label.Content = "Searching...";
            this.UpdateLayout();
            if (string.IsNullOrEmpty(this.search_tags.Text))
                tags = new string[0];
            else
                tags = this.search_tags.Text.Split("; ");

            var title = this.search.Text;
            var search = new SearchReq(title, tags);
            this.Offset = 0;
            this.last_call = (offset) => Api.INSTANCE.SearchWorlds(search, offset);
            this.FillGrid(this.last_call!.Invoke(this.Offset));
            this.report_label.Content = "Done";
        }

        private void PrevIndex(object sender, RoutedEventArgs e)
        {
            if (this.Offset == 0)
                return;

            this.Offset--;
            if (this.last_call is not null)
                this.FillGrid(this.last_call.Invoke(this.Offset));
        }

        private void NextIndex(object sender, RoutedEventArgs e)
        {
            if (this.grid.Children.Count < 10)
                return;

            this.Offset++;
            if (this.last_call is not null)
                this.FillGrid(this.last_call.Invoke(this.Offset));
        }
    }
}
