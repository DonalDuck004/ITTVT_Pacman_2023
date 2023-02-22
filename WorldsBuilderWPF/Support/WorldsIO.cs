using System.Diagnostics;
using System.IO;
using System.Linq;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;

using WorldsBuilderWPF.Types;

using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;
using System.Collections.Generic;


namespace WorldsBuilderWPF
{
    public partial class MainWindow : Window
    {
        private void OnLoadClick(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new() { Multiselect = false, CheckFileExists = true, CheckPathExists = true };
            dialog.ShowDialog();
            if (dialog.SafeFileName == "")
                return;

            this.Title = dialog.FileName;

            BinaryReader reader = new(new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read));

            this.SetPacman(x: reader.ReadInt32(), y: reader.ReadInt32(), rt: reader.ReadInt32());

            Walls walls;
            Color color;

            foreach (var item in this.game_grid.Children.OfType<Image>())
            {

                if (object.ReferenceEquals(item, this.ghosts[0].image) ||
                    object.ReferenceEquals(item, this.ghosts[1].image) ||
                    object.ReferenceEquals(item, this.ghosts[2].image) ||
                    object.ReferenceEquals(item, this.ghosts[3].image) ||
                    object.ReferenceEquals(item, this.PacmanCeil) ||
                    object.ReferenceEquals(item, FocusEffect))
                    continue;

                switch (reader.ReadInt32())
                {
                    case -1:
                        item.Source = MainWindow.PacDotImage;
                        item.Tag = WallTag.PAC_DOT;
                        break;
                    case -2:
                        item.Source = MainWindow.PowerPelletImage;
                        item.Tag = WallTag.POWER_PELLET;
                        break;
                    case -3:
                        item.Source = MainWindow.EmptyImage;
                        item.Tag = WallTag.EMPTY;
                        break;
                    case -4:
                        walls = (Walls)reader.ReadInt32();
                        color = Color.FromArgb(reader.ReadByte(),
                                               reader.ReadByte(),
                                               reader.ReadByte());
                        item.Source = MainWindow.GetImage(walls, color);
                        item.Tag = new WallTag(walls, color);
                        break;
                    case -5:
                        item.Source = MainWindow.GateImage;
                        item.Tag = WallTag.GATE;
                        break;
                    case -6:
                        item.Source = MainWindow.UnspawnableImage;
                        item.Tag = WallTag.UNSPAWNABLE;
                        break;
                    default:
                        continue;
                        // throw new Exception();
                }

            }

            GhostEngines engine;
            List<System.Drawing.Point> positions = new();

            foreach (var item in ghosts)
            {
                item.ClearGrid();
                engine = (GhostEngines)reader.ReadInt32();

                if (engine.SupportsSchema())
                {
                    positions = new();

                    for (int _ = reader.ReadInt32(); _ >= 0; _--)
                        positions.Add(new(reader.ReadInt32(), reader.ReadInt32()));
                    
                    item.SetPositions(positions, engine);
                }else
                    item.SetPosition(new(reader.ReadInt32(), reader.ReadInt32()), engine);

            }
            reader.Close();
        }

        public void DumpWorld(BinaryWriter output_stream)
        {

            output_stream.Write(Grid.GetColumn(this.PacmanCeil));
            output_stream.Write(Grid.GetRow(this.PacmanCeil));
            output_stream.Write(this.pacman_rotation_combo_box.SelectedIndex);

            int c = 0;
            foreach (var field in this.game_grid.Children.OfType<Image>())
            {
                if (object.ReferenceEquals(field, this.ghosts[0].image) ||
                    object.ReferenceEquals(field, this.ghosts[1].image) ||
                    object.ReferenceEquals(field, this.ghosts[2].image) ||
                    object.ReferenceEquals(field, this.ghosts[3].image) ||
                    object.ReferenceEquals(field, this.PacmanCeil) ||
                    object.ReferenceEquals(field, MainWindow.FocusEffect))
                    continue;
                c++;


                if (((Tag)field.Tag).tag is Tags.PacDot)
                    output_stream.Write(-1);
                else if (((Tag)field.Tag).tag is Tags.PowerPellet)
                    output_stream.Write(-2);
                else if (((Tag)field.Tag).tag is Tags.Empty)
                    output_stream.Write(-3);
                else if (field.Tag is WallTag wt)
                {
                    output_stream.Write(-4);
                    output_stream.Write((int)wt.wall);
                    output_stream.Write(wt.color.R);
                    output_stream.Write(wt.color.G);
                    output_stream.Write(wt.color.B);
                }
                else if (((Tag)field.Tag).tag is Tags.Gate)
                    output_stream.Write(-5);
                else if (((Tag)field.Tag).tag is Tags.Unspawnable)
                    output_stream.Write(-6);
                else
                    throw new Exception("Invalid field");
            }

            foreach (var ghost in this.ghosts)
            {
                output_stream.Write((int)ghost.engine);

                if (ghost.engine.SupportsSchema())
                {
                    Debug.Assert(ghost.positions is not null);
                    output_stream.Write(ghost.positions.Count);
                    foreach (var item in ghost.positions)
                    {
                        output_stream.Write(item.X);
                        output_stream.Write(item.Y);
                    }
                }
                else
                {
                    output_stream.Write(Grid.GetColumn(ghost.image));
                    output_stream.Write(Grid.GetRow(ghost.image));
                }
            }
        }

        public void Save()
        {
            SaveFileDialog dialog = new();
            dialog.ShowDialog();
            if (dialog.FileName == "")
                return;

            BinaryWriter stream = new(new FileStream(dialog.FileName, FileMode.OpenOrCreate, FileAccess.Write));

            this.DumpWorld(stream);
            stream.Close();
        }
    }
}
