using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace mcZen.Data
{
	/// <summary>
	/// A simple sql query that gets executed.
	/// </summary>
	public class Command : ICommandAsync
	{
		private SqlCommand _Cmd = null;
		public Command(string query, params SqlParameter[] parameters) : this(query, CommandType.Text, parameters)
		{
		}

		public Command(string query, CommandType type, params SqlParameter[] parameters)
		{
			_Cmd = new SqlCommand(query);
			_Cmd.CommandType = type;
			if (parameters != null)
				_Cmd.Parameters.AddRange(parameters);
		}

		public Command(string query, CommandType type, int timeout, params SqlParameter[] parameters)
		{
			_Cmd = new SqlCommand(query);
			_Cmd.CommandType = type;
			_Cmd.CommandTimeout = timeout;
			if (parameters != null)
				_Cmd.Parameters.AddRange(parameters);
		}

		public Command(string query, CommandType type, TimeSpan timeout, params SqlParameter[] parameters)
		{
			_Cmd = new SqlCommand(query);
			_Cmd.CommandType = type;
			_Cmd.CommandTimeout = (int)timeout.TotalSeconds;
			if (parameters != null)
				_Cmd.Parameters.AddRange(parameters);
		}

		public virtual void Initialize(SqlConnection conn, SqlTransaction trans)
		{
			_Cmd.Connection = conn;
			_Cmd.Transaction = trans;
		}

		/// <summary>
		/// Internal SqlCommand
		/// </summary>
		public SqlCommand InternalCommand
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
