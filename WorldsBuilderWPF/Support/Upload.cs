using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WorldsBuilderWPF
{
    public partial class MainWindow : Window
    {

        private record World(byte[] byte_map, string title, string[] tags, byte[] preview);

        private void Upload(object sender, RoutedEventArgs e)
        {
            var window = new InputWindow();
            window.ShowDialog();
            if (!window.Acquired)
                return;

            HttpClient client = new();

            BinaryWriter st = new(new MemoryStream());
            this.DumpWorld(st);
            st.BaseStream.Position = 0;

            var world = new World(((MemoryStream)st.BaseStream).ToArray(), window.InTitle, window.InTags, this.TakeScreenShot());
            st.Close();
            var msg = new HttpRequestMessage(HttpMethod.Post, "http://[2a00:6d42:1242:1c00:0000:0000:0000:0015]:1980/add_world");
            msg.Content = new StringContent(JsonSerializer.Serialize(world), Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage reply = client.Send(msg);
                if (reply.IsSuccessStatusCode)
                    MessageBox.Show("Upload effettauto!");
                else
                    MessageBox.Show("Upload Fallito:\n" + reply.Content.ReadAsStringAsync().Result);
            }catch (Exception ex)
            {
                MessageBox.Show($"Errore non gestito!\nProcurati un ipv6 se non ne hai uno!\n\n{ex.Message}");
            }

        }

        private byte[] TakeScreenShot()
        {
            var state = this.WindowState;
            this.WindowState = WindowState.Maximized;
            this.UpdateLayout();
            
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)game_grid.ActualWidth, (int)game_grid.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(game_grid);
            PngBitmapEncoder pngImage = new PngBitmapEncoder();
            pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            this.WindowState = state;
            using (var stream = new MemoryStream())
            {
                pngImage.Save(stream);
                return stream.ToArray();
            }
        }
    }
}
