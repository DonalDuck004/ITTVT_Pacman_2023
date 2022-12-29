using System.Windows;
using System.Threading;
using System.IO;
using System;
using Microsoft.Data.Sqlite;
using System.Runtime.ConstrainedExecution;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using PacManWPF.Utils;
using System.Windows.Shapes;
using System.Numerics;

using Point = System.Drawing.Point;
using System.Windows.Input;

namespace PacManWPF.Game.PGs.Movers
{
    public class MLDataCollectorSchemaMover : Abs.BaseGhostMover
    {
        private const string ML_DB_NAME = "ml_temp.sqlite";
        private string hash;
        private SqliteConnection connection;
        private Point Position;

        public static Semaphore sem = new Semaphore(1, 1);

        public MLDataCollectorSchemaMover(string hash, Point start_point)
        {
            try
            {
                sem.WaitOne();
                bool intialize = !File.Exists(ML_DB_NAME);
                connection = new SqliteConnection("Data Source=" + ML_DB_NAME);
                connection.Open();
                this.hash = hash;
                this.Position = start_point;
                if (intialize)
                    this.LoadSchema();
                RegWorld();

            }
            finally
            {
                sem.Release();
            }

        }

        public override System.Drawing.Point GetPos()
        {
            return this.Position;
        }

        private void RegWorld()
        {
            SqliteCommand cur = this.connection.CreateCommand();
            string sql = "INSERT INTO Worlds(WorldName, WorldHash) VALUES($wn, $wh) ON CONFLICT DO NOTHING";
            cur.CommandText = sql;
            cur.Parameters.Clear();
            cur.Parameters.AddWithValue("$wn", "TODO");
            cur.Parameters.AddWithValue("$wh", this.hash);
            cur.ExecuteNonQuery();
        }

        // private int fixed_idx(int idx) => idx == PacmanGame.INSTANCE.FreeAreas.Count ? 0 : idx == -1 ? (PacmanGame.INSTANCE.FreeAreas.Count - 1) : idx;

        List<Point>? cached_way = null;

        private void _NearPoints(Point from,
                                 List<Point> root,
                                 List<List<Point>> done,
                                 Point dest,
                                 int limit)
        {
            if (root.Count > limit)
                return;

            if (done.Count != 0)
                lock (done)
                    if (root.Count > done.Select(x => x.Count).Order().First()) return;
            

            List<Point> near = new();
            foreach (var point in PacmanGame.INSTANCE.FreeAreas)
            {
                if (near.Count == 4)
                    break;
                if (root.Contains(point))
                    continue;


                if (IsValid(from, point))
                {
                    if (point.X == dest.X && point.Y == dest.Y)
                    {
                        root.Add(point);
                        lock (done)
                            done.Add(root);
                        return;
                    }
                    else
                        near.Add(point);
                }
            }

            if (near.Count == 0)
                return;

            var e = near.GetEnumerator();
            e.MoveNext();
            var first = e.Current;
            List<Point> bck;

            while (e.MoveNext())
            {
                bck = new(root)
                {
                    e.Current
                }; // Shallow copy
                _NearPoints(e.Current, bck, done, dest, limit);
            }

            root.Add(first);
            _NearPoints(first, root, done, dest, limit);
        }

        private bool IsValid(Point from, Point check)
        {
            return ((check.X == from.X &&
                    (Math.Abs(check.Y - from.Y) == 1 ||
                    (from.Y == 0 && check.Y == Config.CHUNK_HC - 1) ||
                    (from.Y == Config.CHUNK_HC - 1 && check.Y == 0))
                    )
                    ||
                    (check.Y == from.Y &&
                    (Math.Abs(check.X - from.X) == 1 ||
                    (from.X == 0 && check.X == Config.CHUNK_WC - 1) ||
                    (from.X == Config.CHUNK_WC - 1 && check.X == 0))
                    ));
        }



        private List<Point> NearPoints(Point from, Point dest, int limit, Ghost self)
        {
            if (from == dest)
                return new() { from, dest };

            List<Thread> threads = new List<Thread>();
            List<List<Point>> done = new List<List<Point>>();

            foreach (var item in PacmanGame.INSTANCE.FreeAreas)
            {
                if (IsValid(from, item))
                {
                    if (item.X == dest.X && item.Y == dest.Y)
                        return new() { from, item };

                    threads.Add(new Thread(() => _NearPoints(item, new() { from, item }, done, dest, limit)));
                }
            }

            foreach (var thread in threads)
                thread.Start();
            foreach (var thread in threads)
                thread.Join();

            if (done.Count == 0)
                return new() { from, from };

            if (done.Count == 1)
                return done[0];

            return done.OrderBy(x => x.Count).First();
        }

        public override void NextFrame(Ghost self)
        {
            if (!self.IsDied)
                this.cached_way = null;

           var center = new Point(Config.CHUNK_WC / 2, Config.CHUNK_HC / 2);
            if (self.IsDied) {
                if (cached_way is null)
                {
                    cached_way = NearPoints(this.Position,
                                            center,
                                            1000,
                                            self);
                    cached_way.RemoveAt(0);
                }

                if (cached_way.Count == 0)
                    return;

                this.Position = cached_way[0];
                cached_way.RemoveAt(0);

            }
            // !self.IsDied && !Pacman.INSTANCE.IsDrugged
            this.Position = NearPoints(this.Position,
                                            Pacman.INSTANCE.Position,
                                            10,
                                            self)[1];
        }


        private long? RegPos(SqliteCommand cur, int x, int y, Ghost self, long? previus_id = null, int? flow = null)
        {
            string sql = @"INSERT INTO Positions(ReferToWorld, X, Y, PacmanX, PacmanY, PacmanIsDrugged, PreviusPos, Flow, Ghost) VALUES($wd, $x, $y, $px, $py, $pd, $pi, $f, $gh) RETURNING ID";
            cur.CommandText = sql;
            cur.Parameters.AddWithValue("$wd", this.hash);
            cur.Parameters.AddWithValue("$x", x);
            cur.Parameters.AddWithValue("$y", y);
            cur.Parameters.AddWithValue("$px", Pacman.INSTANCE.Position.X);
            cur.Parameters.AddWithValue("$py", Pacman.INSTANCE.Position.Y);
            cur.Parameters.AddWithValue("$pd", Pacman.INSTANCE.IsDrugged);
            cur.Parameters.AddWithValue("$pi", previus_id is null ? DBNull.Value : previus_id);
            cur.Parameters.AddWithValue("$f", flow is null ? 0 : flow);
            cur.Parameters.AddWithValue("$gh", self.Type);
            SqliteDataReader result = cur.ExecuteReader();
            result.Read();
            object r = result[0];
            result.Close();

            if (r is DBNull)
                return null;
            return (long)r;
        }

        private long? last_id = null;

        private void RegisterPos(int x, int y, int flow, Ghost self)
        {
            sem.WaitOne();

            if (this.last_id is null)
            {
                this.last_id = this.RegPos(this.connection.CreateCommand(), (int)this.Position.X, (int)this.Position.Y, self);
            }

            this.last_id = this.RegPos(this.connection.CreateCommand(), x, y, self, this.last_id, flow);

            sem.Release();

            /*try
            {
                string sql = @"INSERT INTO Positions(ReferToWorld, X, Y, PacmanX, PacmanY, PreviusPos) VALUES($wd, $x, $y, $px, $py, $pp)";
                cur.CommandText = sql;


                sql = "SELECT MAX(ID)"

                Debug.WriteLine(tmp);
            }
            catch
            {

            }*/

        }

        private void LoadSchema()
        {
            string sql = @"CREATE TABLE IF NOT EXISTS Worlds(
                            WorldName VARCHAR(512),
                            WorldHash VARCHAR(32) PRIMARY KEY)";
            SqliteCommand cur = this.connection.CreateCommand();
            cur.CommandText = sql;
            cur.ExecuteNonQuery();


            sql = @"CREATE TABLE IF NOT EXISTS Ghosts(ID INTEGER PRIMARY KEY,
                                                      Color VARCHAR(16))";
            cur.CommandText = sql;
            cur.ExecuteNonQuery();

            sql = @"INSERT INTO Ghosts(ID, Color) VALUES ($0, $1),  ($2, $3),  ($4, $5),  ($6, $7)";

            cur.CommandText = sql;

            var Keys = Enum.GetNames(typeof(Enums.GhostColors));
            var Values = (int[])Enum.GetValues(typeof(Enums.GhostColors));

            int c = 0;
            for (int i = 0; i < Keys.Length; i++)
            {
                cur.Parameters.AddWithValue($"${c++}", Values[i]);
                cur.Parameters.AddWithValue($"${c++}", Keys[i]);
            }

            cur.ExecuteNonQuery();

            sql = @"CREATE TABLE IF NOT EXISTS Positions(ID INTEGER PRIMARY KEY AUTOINCREMENT,
                                                          ReferToWorld REFERENCES Worlds(WorldHash),
                                                          X INT NOT NULL, 
                                                          Y INT NOT NULL,
                                                          PacmanX INT NOT NULL, 
                                                          PacmanY INT NOT NULL, 
                                                          PacmanIsDrugged INT NOT NULL, 
                                                          flow INT,
                                                          Ghost REFERENCES Ghosts(ID), 
                                                          PreviusPos REFERENCES Positions(ID))";
            cur.CommandText = sql;
            cur.ExecuteNonQuery();
        }
    }
}
