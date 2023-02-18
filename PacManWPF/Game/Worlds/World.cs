using System.IO;
using System.Windows.Controls;
using System;
using PacManWPF.Utils;
using PacManWPF.Game.PGs;
using PacManWPF.Game.PGs.Movers;
using PacManWPF.Game.PGs.Movers.Abs;
using System.Security.Cryptography;
using System.Drawing;
using System.Linq;
using PacManWPF.Game.Tags;
using PacManWPF.Game.PGs.Enums;

namespace PacManWPF.Game.Worlds
{
    class World
    {
        private string filename;
        public string Name { get; private set; }
        public string? ID { get; private set; } = null;
        public int PacDotCount { get; private set; }

        public Point SpawnGate { get; private set; }


        public World(string file)
        {
            filename = file;

            Name = Path.GetFileName(file).Split(".")[0];
        }

        public void Apply()
        {
            WorldLoader.CurrentWorld = this;

            int v;
            this.PacDotCount = 0;

            using (BinaryReader sr = new (new FileStream(filename, FileMode.Open, FileAccess.Read)))
            {
                PacmanGame.INSTANCE.InitGame(pacman_x: sr.ReadInt32(),
                                             pacman_y: sr.ReadInt32(),
                                             pacman_grad: sr.ReadInt32() * 90);

                byte[] md5 = new byte[Config.CHUNK_HC * Config.CHUNK_WC * 4];
                int idx = 0;
                Walls walls;

                foreach (var item in GamePage.Current!.game_grid.Children.OfType<System.Windows.Controls.Image>())
                {

                    if (object.ReferenceEquals(Pacman.INSTANCE.CeilObject, item) || 
                        object.ReferenceEquals(Ghost.INSTANCES[0].CeilObject, item) ||
                        object.ReferenceEquals(Ghost.INSTANCES[1].CeilObject, item) ||
                        object.ReferenceEquals(Ghost.INSTANCES[2].CeilObject, item) ||
                        object.ReferenceEquals(Ghost.INSTANCES[3].CeilObject, item))
                        continue;

                    v = sr.ReadInt32();
                    BitConverter.GetBytes(v).CopyTo(md5, idx);
                    idx += 4;

                    item.Opacity = 1.0;
                    if (v == -1)
                    {
                        item.Source = ResourcesLoader.PacDot;
                        item.Tag = new FoodTag(FoodTypes.PacDot);
                        this.PacDotCount++;
                    }
                    else if (v == -2)
                    {
                        item.Source = ResourcesLoader.PowerPellet;
                        item.Tag = new FoodTag(FoodTypes.PowerPellet);
                    }
                    else if (v == -3)
                    {
                        item.Source = ResourcesLoader.EmptyImage;
                        item.Tag = EmptyTag.INSTANCE;
                    }
                    else if (v == -4)
                    {
                        walls = (Walls)sr.ReadInt32();
                        item.Source = ResourcesLoader.GetImage(walls, System.Drawing.Color.FromArgb(sr.ReadByte(),
                                                                                          sr.ReadByte(),
                                                                                          sr.ReadByte()));
                        item.Tag = WallTag.New(walls);
                    }
                    else if (v == -5)
                    {
                        this.SpawnGate = new(Grid.GetColumn(item), Grid.GetRow(item));
                        item.Source = ResourcesLoader.Gate;
                        item.Tag = GateTag.INSTANCE;
                    }
                    else
                        throw new Exception();

                    if (v != -4)
                        PacmanGame.INSTANCE.FreeAreas.Add(new (Grid.GetColumn(item), Grid.GetRow(item)));

                }

                this.ID = Convert.ToHexString(MD5.Create().ComputeHash(md5));

                Type engine_class;
                GhostEngines engine;

                foreach (Ghost ghost in Ghost.INSTANCES)
                {
                    engine = (GhostEngines)sr.ReadInt32();
                    engine_class = BaseGhostMover.GetClassByEngine(engine);

                    if (engine_class.IsSubclassOf(typeof(NoCachedAutoMover)) || typeof(NoCachedAutoMover) == engine_class)
                    {
                        ghost.SetSchema((BaseGhostMover?)Activator.CreateInstance(engine_class,
                                                                                 new Point(sr.ReadInt32(), sr.ReadInt32()),
                                                                                 ghost)!,
                                        SpawnPointOf(ghost));
                    }
                    else if (engine_class.IsSubclassOf(typeof(SchemaBasedMover)))
                    {
                        var schema = new Point[sr.ReadInt32()];
                        for (int i = 0; i < schema.Length; i++)
                            schema[i] = new(sr.ReadInt32(), sr.ReadInt32());

                        ghost.SetSchema((BaseGhostMover)Activator.CreateInstance(engine_class, schema, ghost)!, SpawnPointOf(ghost));
                    } else if (typeof(FixedPositionMover) == engine_class)
                        ghost.SetSchema((FixedPositionMover)Activator.CreateInstance(engine_class, 
                                                                                     new Point(sr.ReadInt32(), sr.ReadInt32()),
                                                                                     ghost)!,
                                        SpawnPointOf(ghost));

                }
            }
        }

        private Point SpawnPointOf(Ghost ghost)
        {
            if (ghost.Type is GhostColors.Red)
                return this.SpawnGate;

            if (ghost.Type is GhostColors.Pink)
                return new(this.SpawnGate.X, this.SpawnGate.Y + 1);

            if (ghost.Type is GhostColors.Cyan)
                return new(this.SpawnGate.X - 1, this.SpawnGate.Y + 1);

            return new(this.SpawnGate.X + 1, this.SpawnGate.Y + 1);
        }

        public bool IsInSpawnArea(Point pos) => IsInSpawnArea(pos.X, pos.Y);

        public bool IsInSpawnArea(int x, int y) {
            if (this.SpawnGate.X == x && this.SpawnGate.Y == y)
                return true;

            if (this.SpawnGate.X == x && this.SpawnGate.Y + 1 == y)
                return true;

            if (this.SpawnGate.X - 1 == x && this.SpawnGate.Y + 1 == y)
                return true;

            return this.SpawnGate.X + 1 == x && this.SpawnGate.Y + 1 == y;
        }
    }
}
