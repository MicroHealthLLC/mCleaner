using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;

namespace mCleaner.Helpers
{
    public class SQLite
    {
        public SQLite() { }
        private static SQLite _i = new SQLite();
        public static SQLite I { get { return _i; } }

        public bool Vacuum(string file)
        {
            bool res = false;

            string sqlitefile = "Data Source=" + file;
            Debug.WriteLine("open sqlite file: {0}", sqlitefile);

            Debug.WriteLine("sqliteconnection");
            using (SQLiteConnection conn = new SQLiteConnection(sqlitefile))
            {
                Debug.WriteLine("database open");
                conn.Open();
                Debug.WriteLine("sqlitecommand");
                SQLiteCommand comm = conn.CreateCommand();

                {
                    Debug.WriteLine("executing non query");
                    comm.CommandText = "vacuum";
                    comm.CommandType = System.Data.CommandType.Text;

                    try
                    {
                        comm.ExecuteNonQuery();
                        res = true;
                    }
                    catch (Exception ex)
                    {
                        res = false;
                    }
                }

                Debug.WriteLine("close");
                conn.Close();
            }

            return res;
        }

        public static string ExecuteNonQuery(string file, string sql)
        {
            string result = string.Empty;

            FileInfo fi = new FileInfo(file);
            string sqlitefile = "Data Source=" + fi.FullName;

            using (SQLiteConnection conn = new SQLiteConnection(sqlitefile))
            {
                conn.Open();
                SQLiteCommand comm = conn.CreateCommand();

                Debug.WriteLine("query: " + sql);
                comm.CommandText = sql;
                comm.CommandType = System.Data.CommandType.Text;

                try
                {
                    comm.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string msg = string.Empty;
                    msg = "ERROR in SQLite.ExecuteNonQuery('{0}', '{1}') msg: {2}\r\n{3}";
                    msg = string.Format(msg, file, sql, ex.Message, ex.StackTrace);
                    result = msg;
                }

                conn.Close();
            }

            return result;
        }
    }
}
