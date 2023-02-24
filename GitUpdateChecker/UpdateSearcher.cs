using System;
using System.ComponentModel;
using System.Net;
using System.IO;
using System.Text.Json;
using System.Net.Http;
using System.Security.Cryptography.Xml;
using System.Threading;
using System.Windows.Threading;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Diagnostics;
using System.Reflection;

namespace GitUpdateChecker
{
    public class UpdateSearcher
    {
        public static UpdateSearcher INSTANCE = new UpdateSearcher();
        private Timer timer;
        public int[]? Version = null;
        private HttpClient client;
        public int[]? Purposed = null;


        private UpdateSearcher() {
            this.timer = new(new TimerCallback(CBK), null, Timeout.Infinite, Timeout.Infinite); // every 10 mins
            this.client = new();
        }

        public void Start(bool CheckForAlreadyDownloaded)
        {
            if (this.Version is null)
                throw new Exception("AO, controlla se hai impostato la versione cogl");

            try
            {
                if (CheckForAlreadyDownloaded && Directory.Exists("Update") && Directory.EnumerateFiles("Update").Any())
                {
                    string version = Path.Combine("Update", "VERSION.TXT");
                    if (!File.Exists(version))
                        Directory.Delete("Update", true);
                    else
                    {
                        var DownloadedVersion = this.GetDownloadedVersion();

                        if (DownloadedVersion is not null && VersionCheck(DownloadedVersion, this.Version!))
                        {
                            var action = MessageBox.Show($"Versione {string.Join(".", DownloadedVersion)} scaricata \n\nVuoi installare?", "Aggiornamento", MessageBoxButton.YesNo);
                            if (action == MessageBoxResult.Yes)
                                this.RunUpdate();
                        }
                        else
                            Directory.Delete("Update", true);
                    }
                }
            } catch (Exception)
            {

            }

            this.timer.Change(0, 1000 * 100);
        }

        private bool VersionCheck(int[] New, int[] Current) => New[0] >= Current[0] && New[1] > Current[1];

        private int[]? GetDownloadedVersion()
        {
            string version = Path.Combine("Update", "VERSION.TXT");
            if (File.Exists(version))
            {
                var DownloadedVersion = new int[2];
                using (var x = new BinaryReader(new FileStream(Path.Combine("Update", "VERSION.TXT"), FileMode.Open, FileAccess.Read)))
                {
                    DownloadedVersion[0] = x.ReadInt32();
                    DownloadedVersion[1] = x.ReadInt32();
                }

                return DownloadedVersion;
            }

            return null;
        }

        private void RunUpdate()
        {
            var from = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location)!, "Updater", "UpdateInstaller.exe");
            Process.Start(new ProcessStartInfo(from)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = Process.GetCurrentProcess().Id.ToString()
            });            
        }

        public void Stop()
        {
            this.timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void CBK(object? sender)
        {
            const string URL = "https://api.github.com/repos/DonalDuck004/ITTVT_Pacman_2023/releases";
            var msg = new HttpRequestMessage(HttpMethod.Get, URL);
            msg.Headers.Add("User-Agent", "Pacman-Client");


            var response = this.client.Send(msg);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var js = JsonSerializer.Deserialize<Dictionary<string, object>[]>(response.Content.ReadAsStream())!.First();
                var tag_name = js["tag_name"].ToString()!;
                var LastVersion = tag_name.Substring(8).Split(".").Select(int.Parse).ToArray();
                var DownloadedVersion = GetDownloadedVersion();
                if (this.Purposed is not null && LastVersion[0] == this.Purposed[0] && LastVersion[1] == this.Purposed[1])
                    return;

                if (DownloadedVersion is not null && VersionCheck(DownloadedVersion, this.Version!) && !VersionCheck(LastVersion, DownloadedVersion))
                    return;

                if (VersionCheck(LastVersion, this.Version!))
                {
                    var body = js["body"].ToString()!;
                    var prerelease = js["prerelease"].ToString() == "True";
                    var published_at = DateTime.Parse(js["published_at"].ToString()!);

                    var assets = ((JsonElement)js["assets"]).Deserialize<Dictionary<string, object>[]>()!;

                    
                    var action = MessageBox.Show($"{body}\n\nVuoi installare?", $"Aggiornamento {tag_name}", MessageBoxButton.YesNo);
                    this.Purposed = LastVersion;
                    if (action == MessageBoxResult.Yes)
                    {
                        if (Directory.Exists("Update"))
                            Directory.Delete("Update", true);

                        Directory.CreateDirectory("Update");
                        string fn;
                        Stream s;
                        const int BUFF_SIZE = 1024 * 1024;
                        int l;
                        var buffer = new byte[BUFF_SIZE];
                        var files = new string[assets.Length];

                        foreach (var asset in assets)
                        {
                            fn = Path.Combine("Update", asset["name"].ToString()!);
                            msg = new HttpRequestMessage(HttpMethod.Get, asset["browser_download_url"].ToString()!);
                            msg.Headers.Add("User-Agent", "Pacman-Client");
                            s = this.client.Send(msg).Content.ReadAsStream();
                            using (var sw = new FileStream(fn, FileMode.Create, FileAccess.Write))
                                while ((l = s.Read(buffer, 0, buffer.Length)) != 0)
                                    sw.Write(buffer, 0, l);
                        }

                        MessageBox.Show("Download completato!\nChiudi per aggiornare");
                        using (var x = new BinaryWriter(new FileStream(Path.Combine("Update", "VERSION.TXT"), FileMode.Create, FileAccess.Write)))
                        {
                            x.Write(LastVersion[0]);
                            x.Write(LastVersion[1]);
                        }
                        this.RunUpdate();
                    }
                }
            }
        }
    }
}
