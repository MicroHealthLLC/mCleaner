using CodeBureau;
using mCleaner.Helpers;
using mCleaner.Helpers.Data;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;
using mCleaner.Properties;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;

namespace mCleaner.Logics.Commands
{
    public class CommandLogic_Chrome : iActions
    {
        static string ChromeDefaultPath = string.Empty;
        static int ChromeCurrentVersion = 0;
        static bool Shred = Settings.Default.ShredFiles;

        private action _Action = new action();
        public action Action
        {
            get { return _Action; }
            set
            {
                if (_Action != value)
                {
                    _Action = value;
                }
            }
        }

        public void Execute(bool apply = false)
        {
            SEARCH search = (SEARCH)StringEnum.Parse(typeof(SEARCH), Action.search);

            switch (search)
            {
                case SEARCH.file:
                    EnqueueFiles();
                    break;
                case SEARCH.glob:
                    break;
            }
        }

        void EnqueueFiles()
        {
            if (File.Exists(Action.path))
            {
                COMMANDS command = (COMMANDS)StringEnum.Parse(typeof(COMMANDS), Action.command);

                Worker.I.EnqueTTD(new Model_ThingsToDelete()
                {
                    FullPathName = Action.path,
                    IsWhitelisted = false,
                    OverWrite = false,
                    WhatKind = THINGS_TO_DELETE.clamwin,
                    command = command,
                    search = SEARCH.file
                });
            }

            ChromeDefaultPath = Environment.ExpandEnvironmentVariables("%localappdata%");
            ChromeDefaultPath = Path.Combine(ChromeDefaultPath, @"Google\Chrome\User Data\Default");
        }

        static int ChromeVersion()
        {
            int ret = 0;

            FileInfo fi = new FileInfo(Path.Combine(ChromeDefaultPath, "History"));

            string sqlitefile = "Data Source=" + fi.FullName;

            using (SQLiteConnection conn = new SQLiteConnection(sqlitefile))
            {
                conn.Open();
                SQLiteCommand comm = conn.CreateCommand();

                string sql = string.Empty;
                sql = "select value from meta where key='version';";

                comm.CommandText = sql;
                comm.CommandType = System.Data.CommandType.Text;

                try
                {
                    SQLiteDataReader reader = comm.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        ret = Convert.ToInt16(reader["value"].ToString());
                    }
                    else
                    {
                        ret = 0;
                    }
                }
                catch (Exception ex)
                {
                    ret = 0;
                }
                conn.Close();
            }

            return ret;
        }

        static string CreateRandomBlobQuery(string[] cols, string table, string condition = "")
        {
            string sql = string.Empty;
            string[] a_col = new string[cols.Length];
            string sets = string.Empty;
            for (int i = 0; i < cols.Length; i++)
            {
                a_col[i] = cols[i] + "=randomblob(length(100))";
            }
            sets = string.Join(",", a_col);

            sql = string.Format("update {0} set {1} {2};", table, sets, condition);

            return sql;
        }

        static int GetIDsFromURLs(string file, string url)
        {
            int ret = 0;

            string sqlitefile = "Data Source=" + file;

            using (SQLiteConnection conn = new SQLiteConnection(sqlitefile))
            {
                conn.Open();
                SQLiteCommand comm = conn.CreateCommand();

                comm.CommandText = string.Format("select id from urls where url='{0}'", url);
                comm.CommandType = System.Data.CommandType.Text;

                try
                {
                    SQLiteDataReader reader = comm.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        ret = reader.GetInt32(reader.GetOrdinal("id"));
                    }
                }
                catch (Exception ex)
                {
                    ret = 0;
                }

                conn.Close();
            }

            return ret;
        }

        public static bool CleanKeywords(string file)
        {
            bool res = false;

            string sqlitefile = "Data Source=" + file;

            using (SQLiteConnection conn = new SQLiteConnection(sqlitefile))
            {
                conn.Open();
                SQLiteCommand comm = conn.CreateCommand();

                string sql = string.Empty;

                if (Shred)
                {
                    string[] cols = { "short_name", "keyword", "favicon_url", "originating_url", "suggest_url" };
                    sql += CreateRandomBlobQuery(cols, "keywords", "where not date_created = 0");
                }

                sql += "delete from keywords where not date_created = 0;";

                comm.CommandText = sql;

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
                conn.Close();
            }

            return res;
        }

        public static bool CleanHistory(string file)
        {
            bool ret = false;
            string sql = string.Empty;

            FileInfo fi = new FileInfo(file);

            // open json file
            string json = JSON.OpenJSONFiel(Path.Combine(fi.Directory.FullName, "Bookmarks"));

            #region // get bookmark urls
            List<string> urls = new List<string>();
            JObject basenode = JObject.Parse(json);
            JToken basetoken = basenode["roots"]["bookmark_bar"]["children"];

            Stack<JToken> nodes = new Stack<JToken>();
            nodes.Push(basetoken);

            while (nodes.Count > 0)
            {
                JToken node = nodes.Pop();

                foreach (JToken t in node.Children())
                {
                    if (t.SelectToken("type") == null) continue;

                    if (t.SelectToken("type").ToString() == "folder")
                    {
                        nodes.Push(t["children"]);
                    }
                    else if (t.SelectToken("type").ToString() == "url")
                    {
                        //Console.WriteLine(t.SelectToken("id"));
                        urls.Add(t.SelectToken("url").ToString());
                    }
                }
            }
            #endregion

            #region // get ids from urls table in History database file
            List<int> ids = new List<int>();
            foreach (string url in urls)
            {
                int int_result = GetIDsFromURLs(fi.FullName, url);

                if (int_result != 0)
                {
                    ids.Add(int_result);
                }
            }
            #endregion

            #region // craete query
            string where = string.Empty;
            string[] cols = new string[] {
                                "urls", "title"
                            };

            if (ids.Count > 0)
            {
                where = string.Format("where id not in ({0})", string.Join(",", ids.ToArray()));
            }

            if (Shred)
            {
                sql += CreateRandomBlobQuery(cols, "urls", where);
            }
            sql += string.Format("delete from urls {0};", where);
            sql += string.Format("delete from visits;");
            cols = new string[] {
                        "lower_term", "term"
                   };
            if (Shred)
            {
                sql += CreateRandomBlobQuery(cols, "keyword_search_terms");
            }
            sql += "delete from keyword_search_terms;";
            #endregion

            #region // execute query
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
                    ret = true;
                }
                catch (Exception ex)
                {
                    ret = false;
                }

                conn.Close();
            }
            #endregion

            return ret;
        }

        public static bool CleanFavIcons(string file)
        {
            bool ret = true;
            string history_db = Path.Combine(ChromeDefaultPath, "History");
            string sql = string.Empty;
            string where = string.Empty;

            sql = string.Format("attach database \"{0}\" as History;", history_db);

            // icon mapping
            string[] cols = new string[] {
                "page_url"
            };
            where = "where page_url not in (select distinct url from History.urls)";
            if (Shred)
            {
                sql += CreateRandomBlobQuery(cols, "icon_mapping", where);
            }
            sql += string.Format("delete from icon_mapping {0};", where);

            // favicon images
            cols = new string[] {
                "image_data"
            };
            where = "where id not in (select distinct id from icon_mapping)";
            if (Shred)
            {
                sql += CreateRandomBlobQuery(cols, "favicon_bitmaps", where);
            }
            sql += string.Format("delete from favicon_bitmaps {0};", where);

            ChromeCurrentVersion = ChromeVersion();

            // favicon bitmaps
            if (ChromeCurrentVersion < 28)
            {
                cols = new string[] { "url", "image_data" };
            }
            else
            {
                cols = new string[] { "url" };
            }
            where = "where id not in (select distinct icon_id from icon_mapping)";
            if (Shred)
            {
                sql += CreateRandomBlobQuery(cols, "favicons", where);
            }
            sql += string.Format("delete from favicons {0};", where);

            SQLite.ExecuteNonQuery(file, sql);

            return ret;
        }

        public static bool CleanDatabases(string file)
        {
            bool ret = true;
            string sql = string.Empty;
            sql = "";
            string where = string.Empty;
            string[] cols = new string[] {
                "origin", "name", "description"
            };
            where = "where origin not like 'chrome-%'";
            if (Shred)
            {
                sql += CreateRandomBlobQuery(cols, "Databases", where);
            }
            sql = string.Format("delete from Databases {0};", where);
            SQLite.ExecuteNonQuery(file, sql);
            return ret;
        }

        public static bool CleanAutofill(string file)
        {
            bool ret = true;

            string sql = string.Empty;
            sql = "";
            string where = string.Empty;
            string[] cols = new string[] {
                "name", "value", "value_lower"
            };
            if (Shred)
            {
                sql += CreateRandomBlobQuery(cols, "autofill", where);
            }
            sql = string.Format("delete from Databases {0};", where);
            SQLite.ExecuteNonQuery(file, sql);

            return ret;
        }

        public static bool IsChromeRunning()
        {
            bool ret = false;
            foreach (Process proc in Process.GetProcesses())
            {
                if (proc.ProcessName.ToLower().Contains("chrome"))
                {
                    if (proc.MainWindowTitle != string.Empty)
                    {
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }
    }
}
