using System;
using System.Data;
using MySqlConnector;

namespace mcZen.Data.MySql
{
	/// <summary>
	/// A simple sql query that gets executed.
	/// </summary>
	public class Command : ICommandAsync
	{
		private MySqlCommand _Cmd = null;
		public Command(string query, params MySqlParameter[] parameters) : this(query, CommandType.Text, parameters)
		{
		}

		public Command(string query, CommandType type, params MySqlParameter[] parameters)
		{
			_Cmd = new MySqlCommand(query);
			_Cmd.CommandType = type;
			if (parameters != null)
				_Cmd.Parameters.AddRange(parameters);
		}

		public Command(string query, CommandType type, int timeout, params MySqlParameter[] parameters)
		{
			_Cmd = new MySqlCommand(query);
			_Cmd.CommandType = type;
			_Cmd.CommandTimeout = timeout;
			if (parameters != null)
				_Cmd.Parameters.AddRange(parameters);
		}

		public Command(string query, CommandType type, TimeSpan timeout, params MySqlParameter[] parameters)
		{
			_Cmd = new MySqlCommand(query);
			_Cmd.CommandType = type;
			_Cmd.CommandTimeout = (int)timeout.TotalSeconds;
			if (parameters != null)
				_Cmd.Parameters.AddRange(parameters);
		}

		public virtual void Initialize(MySqlConnection conn, MySqlTransaction trans)
		{
			_Cmd.Connection = conn;
			_Cmd.Transaction = trans;
		}

		/// <summary>
		/// Internal MySqlCommand
		/// </summary>
		public MySqlCommand InternalCommand
		{
			get { return _Cmd; }
		}

		public virtual int Execute()
		{
			try
			{
				return _Cmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				throw new CommandException(_Cmd, ex);
			}
		}
		public virtual async System.Threading.Tasks.Task<int> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
		{
			try
			{
				return await _Cmd.ExecuteNonQueryAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				throw new CommandException(_Cmd, ex);
			}
		}
	}
}
