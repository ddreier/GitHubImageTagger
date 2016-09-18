using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GitHubImageTagger.Core
{
    public static class Extensions
    {
        public static int IntFromSql(this ApplicationDbContext _context, string sql)
        {
            int count;
            using (var connection = _context.Database.GetDbConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    string result = command.ExecuteScalar().ToString();

                    int.TryParse(result, out count);
                }
            }

            return count;
        }
    }
}
