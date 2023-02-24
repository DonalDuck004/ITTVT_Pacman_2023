using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PacmanOnlineMapsWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 



    public partial class PlugWindow : Window
    {
#if DEBUG
        [STAThread]
        public static void Main()
        {
            var app = new System.Windows.Application();
            app.StartupUri = new Uri("PacmanOnlineMaps.xaml", UriKind.Relative);
            app.Run();
        }
#endif

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
                try
                {
                    await Api.INSTANCE.GetConstantsAsync();
                    this.report_label.Content = "Failed to update constants";
                }
                catch (Exception)
                {
                    this.report_label.Content = "Updated constants";

                }

            };
            this.Closed += (s, e) => dp.Stop();
            dp.Start();
        }

        public void FillGrid(SearchedWorld[] worlds)
        {
            this.grid.Children.Clear();

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
                        ImageSource = new BitmapImage(new Uri(@"pack://application:,,,/Assets/Images/download.png"))
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
            try
            {
                using (var fs = new BinaryWriter(new FileStream(PlugWindow.OUTPUT_PATH + "/" + title + ".world", FileMode.OpenOrCreate, FileAccess.Write)))
                    fs.Write(Api.INSTANCE.GetWorld(world_id));

                this.report_label.Content = $"{title} - ({world_id}) downloaded";
            } catch(Exception)
            {
                this.report_label.Content = $"{title} - ({world_id}) failed";
            }
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
