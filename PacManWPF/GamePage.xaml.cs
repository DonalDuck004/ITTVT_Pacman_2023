using PacManWPF.Animations;
using PacManWPF.Game.PGs.Enums;
using PacManWPF.Game.PGs;
using PacManWPF.Game;
using PacManWPF.Game.Worlds;
using PacManWPF.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
using PacManWPF.Game.Tags;

namespace PacManWPF
{
    /// <summary>
    /// Logica di interazione per GamePage.xaml
    /// </summary>
    public partial class GamePage : UserControl
    {
        public static GamePage? Current { get; private set; } = null;
        public static Grid? CurrentGrid => Current is null ? null : Current!.game_grid;
        private static Random rnd = new Random();

        public GamePage(int world_idx)
        {
            InitializeComponent();
            if (GamePage.Current is not null)
            {
                GamePage.CurrentGrid!.Children.Remove(Pacman.INSTANCE.CeilObject);
                foreach (var item in Ghost.INSTANCES)
                    GamePage.CurrentGrid!.Children.Remove(item.CeilObject);
            }

            GamePage.Current = this;
            this.world_label.Content = WorldLoader.Worlds[world_idx].Name;
            WorldLoader.Worlds[world_idx].Apply();
            this.world_id_label.Content = WorldLoader.Worlds[world_idx].ID;

            if (UIWindow.INSTANCE.KeyListener is null)
            {
                UIWindow.INSTANCE.KeyListener = new Thread(UIWindow.INSTANCE.MovementListener);
                UIWindow.INSTANCE.KeyListener.Start();
            }

            foreach (var img in this.game_grid.Children.OfType<Image>())
                RenderOptions.SetBitmapScalingMode(img, Game.RuntimeSettingsHandler.CurrentGraphic);
        }

        public void SpawnFood()
        {
            if (rnd.Next(200) == 0)
            {
                Debug.Assert(WorldLoader.CurrentWorld is not null);
                var world = WorldLoader.CurrentWorld;
                var values = (FoodTypes[])Enum.GetValues(typeof(FoodTypes));
                var ceils = this.game_grid.Children.OfType<Image>().Where(x => x.Tag is EmptyTag).Where(x => !Pacman.INSTANCE.IsAt(Grid.GetColumn(x), Grid.GetRow(x))).Where(x => !world.IsInSpawnArea(Grid.GetColumn(x), Grid.GetRow(x))).ToArray();
                if (ceils.Length == 0)
                    return;

                var ceil = ceils[rnd.Next(ceils.Length)];
                var food = values[rnd.Next(2, values.Length)];
                var guid = Guid.NewGuid();
                ceil.Source = ResourcesLoader.GetImage(food);
                if (!RuntimeSettingsHandler.AnimationsEnabled)
                {
                    ceil.Tag = new FoodTag(food, null, guid);
                    var timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(10) };
                    timer.Tick += (s, e) => {
                        timer.Stop();
                        if (ceil.Tag is not FoodTag || ((FoodTag)ceil.Tag).guid is null)
                            return;

                        Guid ID = ((FoodTag)ceil.Tag).guid!.Value;
                        if (ID == guid)
                        {
                            ceil.Source = ResourcesLoader.EmptyImage;
                            ceil.Tag = EmptyTag.INSTANCE;
                        }
                    };
                    timer.Start();
                    return;
                }

                var animation = new SpecialFoodAnimation();
                ceil.Tag = new FoodTag(food, animation, guid);

                animation.Completed += (s, e) =>
                {
                    if (ceil.Tag is not FoodTag || ((FoodTag)ceil.Tag).animation is null || ((FoodTag)ceil.Tag).guid is null)
                        return;

                    Guid ID = ((FoodTag)ceil.Tag).guid!.Value;

                    if (ID == guid)
                    {
                        ceil.Opacity = 1.0;
                        ceil.Tag = EmptyTag.INSTANCE;
                        ceil.Source = ResourcesLoader.EmptyImage;
                    }
                };


                ceil.BeginAnimation(UIElement.OpacityProperty, animation);
            }
        }
    }
}
