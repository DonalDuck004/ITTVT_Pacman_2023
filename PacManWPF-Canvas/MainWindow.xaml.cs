using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using PacManWPF.Utils;
using PacManWPF_Canvas.MapEntries.Characters;
using PacManWPF_Canvas.Utils;
using PacManWPF_Canvas.Worlds;

namespace PacManWPF_Canvas
{

    public record KeyboardStatus(bool Top, bool Left, bool Bottom, bool Right);
    public record GameEntitiesStatus(PacmanMapEntry Pacman, List<GhostMapEntry> GhostMapEntries);

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty FpsProperty = DependencyProperty.Register("Fps", typeof(double), typeof(MainWindow));
        public double Fps { get => (double)this.GetValue(FpsProperty); set => this.SetValue(FpsProperty, value); }
        public const bool EXPERIMENTAL_KEEP_DOWN = true;

        private GameMap GameMap; // Temporary
        private DispatcherTimer FrameClock;
        private DispatcherTimer TickClock;
        private readonly Stopwatch stopwatch = Stopwatch.StartNew();
        private List<Key> InputKeys = new List<Key>();


        public MainWindow()
        {
            InitializeComponent();

            this.FrameClock = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1).Divide(120)
            };
            this.FrameClock.Tick += (s, e) => this.EmitFrame();
            this.FrameClock.Start();

            this.TickClock = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1).Divide(750)
            };
            this.TickClock.Tick += (s, e) => this.EmitTick();
            this.TickClock.Start();

            this.GameMap = new GameMap();
            Grid.SetRow(this.GameMap.Canvas, 1);
            this.DataContext = this; 
            this.GameWrapper.Children.Add(this.GameMap.Canvas);

            this.MinWidth = Config.EXCPETED_WIDTH;
            this.MinHeight = Config.EXCPETED_HEIGHT + 32;

            this.KeyDown += (s, e) =>
            {
                if (e.Key == Key.W || e.Key == Key.A || e.Key == Key.S || e.Key == Key.D)
                {
                    this.InputKeys.Remove(e.Key);
                    this.InputKeys.Add(e.Key);
                }
                if (e.Key == Key.Escape)
                    this.GameMap.Pause();
            };


            this.KeyUp += (s, e) =>
            {
                if (EXPERIMENTAL_KEEP_DOWN)
                {
                    if (this.InputKeys.Count > 1)
                        this.InputKeys.Remove(e.Key);
                }
                else
                    this.InputKeys.Remove(e.Key);

                if (e.Key == Key.Escape)
                    this.GameMap.Resume();
            };
            this.Loaded += (s, e) => this.GameMap.Load();
            this.DrawGrid();
        }


        public void EmitTick()
        {
            Key? last;

            bool forced_kw_down = false;

            if (EXPERIMENTAL_KEEP_DOWN)
            {
                last = this.InputKeys.Count == 0 ? null : this.InputKeys[^1];
                this.InputKeys.RemoveAll(k => !Keyboard.IsKeyDown(k));

                if (last is not null)
                {
                    this.InputKeys.Add(last.Value);
                    forced_kw_down = true;
                }
            }
            last = this.InputKeys.Count == 0 ? null : this.InputKeys[^1];

            var w = last == Key.W;
            var a = last == Key.A;
            var s = last == Key.S;
            var d = last == Key.D;

            var state = new KeyboardStatus(Top: w, 
                                           Left: a,
                                           Bottom: s,
                                           Right: d);

            var suggested_fallback = this.GameMap.Process(state);
            if (EXPERIMENTAL_KEEP_DOWN && forced_kw_down && suggested_fallback != null)
            {
                this.InputKeys.Clear();
                this.InputKeys.Add(suggested_fallback == Direction.Up ? Key.W :
                                   suggested_fallback == Direction.Left ? Key.A :
                                   suggested_fallback == Direction.Down ? Key.S :
                                   Key.D);
            }
        }

        [Conditional("DEBUG")]
        public void DrawGrid()
        {
            for (int i = 0; i < Config.EXCPETED_WIDTH; i += Config.CHUNK_CS)
            {
                var line = new Line()
                {
                    X1 = i,
                    X2 = i,
                    Y1 = 0,
                    Y2 = Config.EXCPETED_HEIGHT,
                    Stroke = new SolidColorBrush(Colors.White)
                };
                this.GameMap.Canvas.Children.Add(line);
            }

            for (int i = 0; i < Config.EXCPETED_HEIGHT; i += Config.CHUNK_CS)
            {
                var line = new Line()
                {
                    X1 = 0,
                    X2 = Config.EXCPETED_WIDTH,
                    Y1 = i,
                    Y2 = i,
                    Stroke = new SolidColorBrush(Colors.White)
                };
                this.GameMap.Canvas.Children.Add(line);
            }
        }

        public void EmitFrame()
        {
            this.GameMap.Frame();
            
            if (stopwatch.ElapsedMilliseconds >= 1000)
            {
                this.Fps = this.GameMap.Frames / (stopwatch.ElapsedMilliseconds / 1000.0);
                this.GameMap.ResetStats();
                stopwatch.Restart();
                // Debug.WriteLine(fps);
            }
        }
    }
}