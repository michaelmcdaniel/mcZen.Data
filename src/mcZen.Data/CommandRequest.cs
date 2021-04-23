using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace mcZen.Data
{
	/// <summary>
	/// Summary description for CommandRequest
	/// </summary>
	public class CommandRequest : IRequestAsync
	{
		private SqlCommand _Cmd = null;
		public CommandRequest(string query, params SqlParameter[] parameters) : this(query, CommandType.Text, parameters)
		{
		}
		public CommandRequest(string query, CommandType type, params SqlParameter[] parameters)
		{
			_Cmd = new SqlCommand(query);
			_Cmd.CommandType = type;
			if (parameters != null)
				_Cmd.Parameters.AddRange(parameters);
		}

		public CommandRequest(string query, CommandType type, int timeout, params SqlParameter[] parameters)
		{
			_Cmd = new SqlCommand(query);
			_Cmd.CommandType = type;
			_Cmd.CommandTimeout = timeout;
			if (parameters != null)
				_Cmd.Parameters.AddRange(parameters);
		}

		public virtual void Initialize(SqlConnection conn, SqlTransaction trans)
		{
			_Cmd.Connection = conn;
			_Cmd.Transaction = trans;
		}

		public SqlCommand Command
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
				throw new RequestException(_Cmd, ex);
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
				throw new RequestException(_Cmd, ex);
			}
		}
	}
}
