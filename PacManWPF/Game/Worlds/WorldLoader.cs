using System.IO;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System;
using PacManWPF.Utils;
using PacManWPF.Game.PGs;
using PacManWPF.Game.PGs.Movers;
using PacManWPF.Game.PGs.Movers.Abs;
using System.Security.Cryptography;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Linq;

namespace PacManWPF.Game.Worlds
{

    class World
    {
        private string filename;
        public string Name { get; private set; }
        public int TotalPoints { get; private set; }

        public World(string file)
        {
            filename = file;

            Name = Path.GetFileName(file).Split(".")[0];
        }

        public void Apply(MainWindow app)
        {
            WorldLoader.CurrentWorld = this;
            SoundEffectsPlayer.Play(SoundEffectsPlayer.START);

            int v;
            this.TotalPoints = 0;
            //Chunk tmp;

            // background.Size = new Size(Config.CHUNK_WC * Config.ChunkSquare, Config.CHUNK_HC * Config.ChunkSquare);
            using (BinaryReader sr = new BinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read)))
            {
                PacmanGame.INSTANCE.InitGame(pacman_x: sr.ReadInt32(),
                                             pacman_y: sr.ReadInt32(),
                                             pacman_grad: sr.ReadInt32() * 90);

                byte[] md5 = new byte[Config.CHUNK_HC * Config.CHUNK_WC * 4];
                int idx = 0;


                foreach (var item in MainWindow.INSTANCE.game_grid.Children.OfType<System.Windows.Shapes.Rectangle>())
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
                        item.Fill = ResourcesLoader.SmallPoint;
                        this.TotalPoints++;
                    }
                    else if (v == -2)
                    {
                        item.Fill = ResourcesLoader.Drug;
                    }
                    else if (v == -3)
                    {
                        item.Fill = null;
                    }
                    else if (v == -4)
                        item.Fill = ResourcesLoader.GetImage((Walls)sr.ReadInt32(), System.Drawing.Color.FromArgb(sr.ReadByte(),
                                                                                                                  sr.ReadByte(),
                                                                                                                  sr.ReadByte()));
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
                            schema_type = typeof(MLDataCollectorSchemaMover);
                            break;
                        default: 
                            throw new Exception();
                    }

#pragma warning disable CS8604
#pragma warning disable CS8600
                    if (schema_type.IsSubclassOf(typeof(SchemaBasedMover)))
                    {
                        Point[] schema = new Point[sr.ReadInt32()];
                        for (int i = 0; i < schema.Length; i++)
                            schema[i] = new Point(sr.ReadInt32(), sr.ReadInt32());
                        ghost.SetSchema((BaseGhostMover)Activator.CreateInstance(schema_type, schema));
                    } else if (schema_type == typeof(MLDataCollectorSchemaMover))
                    {
                        ghost.SetSchema((BaseGhostMover)Activator.CreateInstance(schema_type, 
                                                                                 Convert.ToHexString(md5), 
                                                                                 new Point(sr.ReadInt32(), sr.ReadInt32())));
                    }
#pragma warning restore CS8604
#pragma warning restore CS8600

                }

            }
        }
    }

    static class WorldLoader
    {
        private static List<World>? _cache = null;
        public static World? CurrentWorld { internal set; get; } = null;
        public static List<World> Worlds
        {
            get
            {
                if (_cache is null)
                {
                    _cache = new List<World>();

                    foreach (string file in Directory.GetFiles(Config.WORLD_DIR))
                        _cache.Add(new World(file));

                }

                return _cache;
            }
        }

        public static void DropCache()
        {
            _cache = null;
        }
    }
}
