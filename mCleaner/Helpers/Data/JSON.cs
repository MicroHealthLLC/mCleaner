using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace mCleaner.Helpers.Data
{
    public static class JSON
    {
        public static string OpenJSONFiel(string filename)
        {
            string json = string.Empty;

            FileInfo fi = new FileInfo(filename);
            if (fi.Exists)
            {
                using (StreamReader reader = fi.OpenText())
                {
                    json = reader.ReadToEnd();
                }
            }

            return json;
        }

        public static bool isAddressFound(string json, string jsonaddress)
        {
            bool res = false;

            JObject d = JObject.Parse(json);
            string[] a_address = jsonaddress.Split('/');

            var a = d.SelectToken(string.Join(".", a_address));

            //foreach (string address in a_address)
            //{
            //    if (d[address] != null)
            //    {
            //        d = (JObject)d[address];
            //    }
            //    else
            //    {
            //        d = null;
            //        break;
            //    }
            //}

            res = a == null ? false : true;

            return res;
        }

        public static string RemoveElementFromAddress(string json, string jsonaddress)
        {
            string newjson = string.Empty;

            JObject d = JObject.Parse(json);
            string[] a_address = jsonaddress.Split('/');

            var a = d.SelectToken(string.Join(".", a_address));
            List<JToken> servers_to_delete = new List<JToken>();

            foreach (JToken s in a.Children())
            {
                servers_to_delete.Add(s);
            }

            foreach (JToken s in servers_to_delete)
            {
                s.Remove();
            }

            //servers.Remove();

            return (newjson = d.ToString());

            //foreach (string address in a_address)
            //{
            //    if (d[address] != null)
            //    {
            //        d = (JObject)d[address];
            //        Debug.WriteLine((d as JProperty).Name);
            //    }
            //    else
            //    {
            //        d = null;
            //        break;
            //    }
            //}
        }
    }
}
