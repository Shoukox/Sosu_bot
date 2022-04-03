using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;

namespace Sosu
{
    public class Database
    {
        private readonly string connString;
        private NpgsqlConnection conn;
        public Database(string connString)
        {
            this.connString = connString;

            conn = new NpgsqlConnection(connString);
        }

        /// <param name="add">-1 - not using | 0 - update | 1 - add</param>
        public async Task InsertOrUpdateOsuUsersTable(long id, string osuname, int add = -1, double pp = 0)
        {
            try
            {
                if(conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Open();
                }
                if (add == 1)
                {
                    var cmd = await new NpgsqlCommand($"INSERT INTO osuusers(id, osuname, pp) VALUES ({id}, '{osuname}', {pp})", conn).ExecuteNonQueryAsync();
                }
                else if (add == 0)
                {
                    var cmd = await new NpgsqlCommand($"UPDATE osuusers SET osuname='{osuname}', pp={pp.ToString()/*.Replace(",", ".")*/} WHERE id={id}", conn).ExecuteNonQueryAsync();
                }
                conn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        /// <param name="add">-1 - not using | 0 - update | 1 - add</param>
        public async Task InsertOrUpdateOsuChatsTable(long lastbeatmapid, long chatid, int add = -1, List<long> members = null)
        {
            try
            {
                if (members == null) members = new List<long>();

                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Open();
                }
                if (add == 1)
                {
                    var cmd = await new NpgsqlCommand($"INSERT INTO osuchats(lastbeatmapid, chatid, members) VALUES ({lastbeatmapid}, {chatid}), ARRAY[{(members.Count == 0 ? "" : string.Join(",", members) )}]::bigint[]", conn).ExecuteNonQueryAsync();
                }
                else if (add == 0)
                {
                    var cmd = await new NpgsqlCommand($"UPDATE osuchats SET lastbeatmapid={lastbeatmapid}, members=ARRAY[{string.Join(",", members)}]::bigint[] WHERE chatid={chatid}", conn).ExecuteNonQueryAsync();
                }
                conn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
        public List<object[]> GetData(string query, int count)
        {
            if (conn.State != System.Data.ConnectionState.Open)
            {
                conn.Open();
            }
            var cmd = new NpgsqlCommand(query, conn);
            var reader = cmd.ExecuteReader();

            List<object[]> data = new List<object[]>();
            while (reader.Read())
            {
                var obj = new object[count];
                for (int i = 0; i <= count - 1; i++) obj[i] = reader.GetProviderSpecificValue(i);
                data.Add(obj);
            }
            conn.Close();
            return data;
        }
    }
}
