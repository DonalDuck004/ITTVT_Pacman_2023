using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Windows.Threading;

namespace GitUpdateChecker
{
    public class UpdateSearcher
    {
        public UpdateSearcher INSTANCE = new UpdateSearcher();
        private Timer timer;
        public int[]? Version = null;
        private HttpClient client;

        public UpdateSearcher() {
            this.timer = new Timer(new TimerCallback(CBK), null, Timeout.Infinite, Timeout.Infinite); // every 10 mins
            this.client = new HttpClient();
        }

        public void Start()
        {
            if (this.Version is null)
                throw new Exception("AO, controlla se hai impostato la versione cogl");

            this.timer.Change(0, 1000 * 100);
        }

        public void Stop()
        {
            this.timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void CBK(object? sender)
        {

        }
    }
}
