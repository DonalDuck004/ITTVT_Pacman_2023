using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace WorldsBuilderWPF
{
    public partial class MainWindow : Window
    {

        private record World(byte[] byte_map, string title, string[] tags, byte[] preview);

        private void UpLoad(object sender, RoutedEventArgs e)
        {
            var window = new InputWindow();
            if (window.ShowDialog() is false)
                return;

            var title = window.title_box.Text;
            var tags = window.tags_box.Text.Split("; ");

            HttpClient client = new();

            BinaryWriter st = new(new MemoryStream());
            this.DumpWorld(st);
            st.BaseStream.Position = 0;

            var world = new World(((MemoryStream)st.BaseStream).ToArray(), title, tags, this.GetWindowScreen());
            st.Close();
            var content = new StringContent(JsonSerializer.Serialize(world), Encoding.UTF8, "application/json");

            var a = client.PostAsync("http://localhost:8000/add_world", content).Result;
        }

        public byte[] GetWindowScreen()
        {

            RenderTargetBitmap renderTargetBitmap = new((int)this.game_grid.ActualWidth, (int)this.game_grid.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(this.game_grid);
            PngBitmapEncoder pngImage = new();
            pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            using (var stream = new MemoryStream())
            {
                pngImage.Save(stream);
                return stream.ToArray();
            }
        }
    }
}
