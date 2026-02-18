using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using System.Text;
using System.Linq;

namespace mcZen.Data.MySql
{
	/// <summary>
	/// Helper functions to build simple sql statements
	/// </summary>
	public static partial class Commands
	{
		/// <summary>
		/// Creates command that uses insert or update query based on id/key with columns set from parameters.  
		/// If id equals Empty Guid, then the key/id is set to Guid.NewGuid() and the values are inserted.  
		/// Otherwise, update is performed using the key as the where clause.
		/// </summary>
		/// <param name="table">The table name (no brackets)</param>
		/// <param name="timeout">Timeout to perform the query in</param>
		/// <param name="id">reference to id that will be set to Guid.NewGuid if it equals Guid.Empty</param>
		/// <param name="key">The key column name</param>
		/// <param name="parameters">Additional columns to set values for.</param>
		/// <returns>Request to be executed using a ConnectionFactory</returns>
		public static mcZen.Data.MySql.ICommand InitializeSave(string table, int timeout, ref Guid id, MySqlParameter key, params MySqlParameter[] parameters)
		{
			string query;
			IEnumerable<MySqlParameter> cc = parameters.Append(key);
			if (id == Guid.Empty)
			{
				key.Value = id = Guid.NewGuid();
				query = string.Format("INSERT INTO `{0}` (`{1}`) VALUES ({2})",
					table,
					string.Join("`,`", (from p in cc select p.ParameterName.Substring(1))),
					string.Join(",", (from p in cc select p.ParameterName)));
			}
			else
			{
				query = string.Format("UPDATE `{0}` SET {1} WHERE {2}", 
					table,
					string.Join(",", (from p in parameters select "`"+p.ParameterName.Substring(1)+"`="+p.ParameterName)), 
					"`"+key.ParameterName.Substring(1)+"`="+key.ParameterName);
			}
			return new mcZen.Data.MySql.Command(query, CommandType.Text, timeout, cc.ToArray());
		}

		/// <summary>
		/// Creates a command that will update all the given parameters into given table
		/// </summary>
		/// <param name="table">Sql table name (no brackets)</param>
		/// <param name="parameters">Columns to add to the insert</param>
		/// <returns>Command to be executed using a ConnectionFactory</returns>
		public static mcZen.Data.MySql.ICommand Update(string table, MySqlParameter key, params MySqlParameter[] parameters)
		{
			string query;
			IEnumerable<MySqlParameter> cc = parameters.Append(key);
			query = string.Format("UPDATE `{0}` SET {1} WHERE {2}",
				table,
				string.Join(",", (from p in parameters select "`" + p.ParameterName.Substring(1) + "`=" + p.ParameterName)),
				"`" + key.ParameterName.Substring(1) + "`=" + key.ParameterName);
			return new mcZen.Data.MySql.Command(query, CommandType.Text, cc.ToArray());
		}

		/// <summary>
		/// Creates a command that will insert all the given parameters into given table
		/// </summary>
		/// <param name="table">Sql table name (no brackets)</param>
		/// <param name="parameters">Columns to add to the insert</param>
		/// <returns>Command to be executed using a ConnectionFactory</returns>
		public static mcZen.Data.MySql.ICommand Insert(string table, params MySqlParameter[] parameters)
		{
			string query;
			IEnumerable<MySqlParameter> cc = parameters;
			query = string.Format("INSERT INTO `{0}` (`{1}`) VALUES ({2})",
				table,
				string.Join("`,`", (from p in cc select p.ParameterName.Substring(1))),
				string.Join(",", (from p in cc select p.ParameterName)));
			return new mcZen.Data.MySql.Command(query, CommandType.Text, cc.ToArray());
		}

		/// <summary>
		/// Creates a scalar command that will insert all the given parameters into given table, returning the identity column
		/// </summary>
		/// <param name="table">Sql table name (no brackets)</param>
		/// <param name="parameters">Columns to add to the insert</param>
		/// <returns>Command to be executed using a ConnectionFactory</returns>
		public static mcZen.Data.MySql.ScalarCommand<T> Insert<T>(Action<T> setKey, string table, string keyColumn, params MySqlParameter[] parameters)
		{
			string query;
			IEnumerable<MySqlParameter> cc = parameters;
			if (keyColumn.StartsWith("@")) keyColumn = keyColumn.Substring(1);
			query = string.Format("INSERT INTO `{0}` (`{1}`) VALUES ({2}) RETURNING `{3}`",
				table,
				string.Join("`,`", (from p in cc select p.ParameterName.Substring(1))),
				string.Join(",", (from p in cc select p.ParameterName)),
				keyColumn);
			return new mcZen.Data.MySql.ScalarCommand<T>(setKey, query, cc.ToArray());
		}

		/// <summary>
		/// Creates command that uses insert or update query based on key parameter set from parameters.  
		/// If key value equals null, then insert command is generated (minus key column)  
		/// Otherwise, update is performed using the key as the where clause.
		/// </summary>
		/// <param name="table"></param>
		/// <param name="key"></param>
		/// <param name="parameters"></param>
		/// <returns>Command to be executed using a ConnectionFactory</returns>
		public static mcZen.Data.MySql.ICommand InitializeSave(string table, MySqlParameter key, params MySqlParameter[] parameters)
		{
			string query;
			IEnumerable<MySqlParameter> cc;
			if (key==null)
			{
				cc = parameters;
				query = string.Format("INSERT INTO `{0}` (`{1}`) VALUES ({2})",
					table,
					string.Join("`,`", (from p in cc select p.ParameterName.Substring(1))),
					string.Join(",", (from p in cc select p.ParameterName)));
			}
			else
			{
				cc = parameters.Append(key);
				query = string.Format("UPDATE `{0}` SET {1} WHERE {2}", 
					table,
					string.Join(",", (from p in parameters select "`"+p.ParameterName.Substring(1)+"`="+p.ParameterName)), 
					"`"+key.ParameterName.Substring(1)+"`="+key.ParameterName);
			}
			return new mcZen.Data.MySql.Command(query, CommandType.Text, cc.ToArray());
		}

		/// <summary>
		/// Creates command that uses insert or update query based on id/key with columns set from parameters.  
		/// If id equals Empty Guid, then the key/id is set to Guid.NewGuid() and the values are inserted.  
		/// Otherwise, update is performed using the key as the where clause.
		/// </summary>
		/// <param name="table">The table name (no brackets)</param>
		/// <param name="id">reference to id that will be set to Guid.NewGuid if it equals Guid.Empty</param>
		/// <param name="key">The key column name</param>
		/// <param name="parameters">Additional columns to set values for.</param>
		/// <returns>Command to be executed using a ConnectionFactory</returns>
		public static mcZen.Data.MySql.ICommand InitializeSave(string table, ref Guid id, MySqlParameter key, params MySqlParameter[] parameters)
		{
			string query;
			IEnumerable<MySqlParameter> cc = parameters.Append(key);
			if (id == Guid.Empty)
			{
				key.Value = (id = Guid.NewGuid());
				query = string.Format("INSERT INTO `{0}` (`{1}`) VALUES ({2})",
					table,
					string.Join("`,`", (from p in cc select p.ParameterName.Substring(1))),
					string.Join(",", (from p in cc select p.ParameterName)));
			}
			else
			{
				query = string.Format("UPDATE `{0}` SET {1} WHERE {2}", 
					table,
					string.Join(",", (from p in parameters select "`"+p.ParameterName.Substring(1)+"`="+p.ParameterName)), 
					"`"+key.ParameterName.Substring(1)+"`="+key.ParameterName);
			}
			return new mcZen.Data.MySql.Command(query, CommandType.Text, cc.ToArray());
		}

		public static ICommand Delete(string table, int timeout, MySqlParameter key)
		{
			return new mcZen.Data.MySql.Command("DELETE FROM " + table + " WHERE " + key.ParameterName.Substring(1) + "=" + key.ParameterName, CommandType.Text, timeout, key);
		}

		public static ICommand Delete(string table, MySqlParameter key)
		{
			return new mcZen.Data.MySql.Command("DELETE FROM " + table + " WHERE " + key.ParameterName.Substring(1) + "=" + key.ParameterName, CommandType.Text, key);
		}


		internal static void AppendAnd(ref bool first, System.Text.StringBuilder builder)
		{
			if (first)
			{
				first = !first;
			}
			else
			{
				builder.Append(" AND ");
			}
		}
	}
}
