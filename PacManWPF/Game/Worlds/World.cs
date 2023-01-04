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
            //Chunk tmp;

            // background.Size = new Size(Config.CHUNK_WC * Config.ChunkSquare, Config.CHUNK_HC * Config.ChunkSquare);
            using (BinaryReader sr = new BinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read)))
            {
                PacmanGame.INSTANCE.InitGame(pacman_x: sr.ReadInt32(),
                                             pacman_y: sr.ReadInt32(),
                                             pacman_grad: sr.ReadInt32() * 90);

                byte[] md5 = new byte[Config.CHUNK_HC * Config.CHUNK_WC * 4];
                int idx = 0;
                Walls walls;

                foreach (var item in MainWindow.INSTANCE.game_grid.Children.OfType<System.Windows.Controls.Image>())
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
                        item.Tag = new WallTag(walls);
                    }
                    else if (v == -5)
                    {
                        this.SpawnGate = new(Grid.GetColumn(item), Grid.GetRow(item));
                        item.Source = ResourcesLoader.Gate;
                        item.Tag = new GateTag();
                    }
                    else
                        throw new Exception();

                    if (v != -4)
                        PacmanGame.INSTANCE.FreeAreas.Add(new Point(Grid.GetColumn(item), Grid.GetRow(item)));

                }

                md5 = MD5.Create().ComputeHash(md5);

                Type schema_type;
                foreach (Ghost ghost in Ghost.INSTANCES)
                {
                    var a = sr.ReadInt32();
                    switch (a)
                    {
                        case 0:
                            schema_type = typeof(CyclicSchemaMover);
                            break;
                        case 1:
                            schema_type = typeof(NextToBackSchemaMover);
                            break;
                        case 2:
                            schema_type = typeof(OneTimeSchemaMover);
                            break;
                        case 3:
                            schema_type = typeof(AutoMover);
                            break;
                        case 4:
                            schema_type = typeof(NoCachedAutoMover);
                            break;
                        default: 
                            throw new Exception();
                    }

#pragma warning disable CS8604
#pragma warning disable CS8600
                    if (schema_type.IsSubclassOf(typeof(SchemaBasedMover)))
                    {
                        var schema = new Point[sr.ReadInt32()];
                        for (int i = 0; i < schema.Length; i++)
                            schema[i] = new Point(sr.ReadInt32(), sr.ReadInt32());
                        ghost.SetSchema((BaseGhostMover)Activator.CreateInstance(schema_type, schema, ghost), SpawnPointOf(ghost));
                    } else if (schema_type.IsSubclassOf(typeof(NoCachedAutoMover)))
                    {
                        ghost.SetSchema((BaseGhostMover)Activator.CreateInstance(schema_type, 
                                                                                 Convert.ToHexString(md5),
                                                                                 new Point(sr.ReadInt32(), sr.ReadInt32()),
                                                                                 ghost), 
                                        SpawnPointOf(ghost));
                    }
#pragma warning restore CS8604
#pragma warning restore CS8600

                }
            }
        }

        private Point SpawnPointOf(Ghost ghost)
        {
            if (ghost.Type is PGs.Enums.GhostColors.Red)
                return this.SpawnGate;
            else if (ghost.Type is PGs.Enums.GhostColors.Pink)
                return new(this.SpawnGate.X, this.SpawnGate.Y + 1);
            else if (ghost.Type is PGs.Enums.GhostColors.Cyan)
                return new(this.SpawnGate.X - 1, this.SpawnGate.Y + 1);
            else
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
