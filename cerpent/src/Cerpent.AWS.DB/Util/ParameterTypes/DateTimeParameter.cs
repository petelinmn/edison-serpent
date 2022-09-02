using Dapper;
using Npgsql;
using NpgsqlTypes;
using System.Data;

namespace Cerpent.AWS.DB.Util.ParameterTypes
{
    public class DateTimeParameter : SqlMapper.ICustomQueryParameter
    {
        private readonly DateTime _value;

        public DateTimeParameter(DateTime value) => _value = value;
        public void AddParameter(IDbCommand command, string name) =>
            command.Parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Timestamp)
            {
                Value = _value
            });
    }
}
