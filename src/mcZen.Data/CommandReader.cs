using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace mcZen.Data
{
	/// <summary>
	/// Command that gets executed where function is called for every row processed.
	/// </summary>
	public class CommandReader : Command
	{
		private int _RecordsAffected = 0;
		private Func<SqlDataReader, System.Threading.Tasks.Task<bool>> _ReadFunc;
		private event OnCompleteEventHandler _OnComplete;

		public CommandReader(string query, params SqlParameter[] parameters)
			: base(query, parameters) { _ReadFunc = Read; }

		public CommandReader(string query, CommandType type, params SqlParameter[] parameters)
			: base(query, type, parameters) { _ReadFunc = Read; }

		public CommandReader(string query, CommandType type, TimeSpan timeout, params SqlParameter[] parameters)
			: base(query, type, timeout, parameters) { _ReadFunc = Read; }

		public CommandReader(string query, CommandType type, int timeout, params SqlParameter[] parameters)
			: base(query, type, timeout, parameters) { _ReadFunc = Read; }


		/// <summary>
		/// New Command that executes given sql and calls given function for each returned row.
		/// </summary>
		/// <param name="func">Function to call for each row.  Return true to continue and false to stop processing.</param>
		/// <param name="query">Sql query to run</param>
		/// <param name="parameters">sql parameters to pass with query</param>
		public CommandReader(Func<SqlDataReader, bool> func, string query, params SqlParameter[] parameters)
			: base(query, parameters) { _ReadFunc = (r)=>Task.FromResult(func(r)); }

		/// <summary>
		/// New Command that executes given sql and calls given function for each returned row.
		/// </summary>
		/// <param name="func">Function to call for each row.  Return true to continue and false to stop processing.</param>
		/// <param name="query">Sql query to run</param>
		/// <param name="type">Type of query the command will run</param>
		/// <param name="timeout">Timeout for the query to execute in.</param>
		/// <param name="parameters">sql parameters to pass with query</param>
		public CommandReader(Func<SqlDataReader, bool> func, string query, CommandType type, TimeSpan timeout, params SqlParameter[] parameters)
			: base(query, type, timeout, parameters) { _ReadFunc = (r) => Task.FromResult(func(r)); }

		/// <summary>
		/// New Command that executes given sql and calls given function for each returned row.
		/// </summary>
		/// <param name="func">Function to call for each row.  Return true to continue and false to stop processing.</param>
		/// <param name="query">Sql query to run</param>
		/// <param name="type">Type of query the command will run</param>
		/// <param name="timeout">Timeout for the query to execute in.</param>
		/// <param name="parameters">sql parameters to pass with query</param>
		public CommandReader(Func<SqlDataReader, bool> func, string query, CommandType type, int timeout, params SqlParameter[] parameters)
			: base(query, type, timeout, parameters) { _ReadFunc = (r) => Task.FromResult(func(r)); }

		/// <summary>
		/// New Command that executes given sql and asynchronously calls given function for each returned row.
		/// </summary>
		/// <param name="func">async function to call for each row.  Return true to continue and false to stop processing. Each call is awaited.</param>
		/// <param name="query">Sql query to run</param>
		/// <param name="type">Type of query the command will run</param>
		/// <param name="timeout">Timeout for the query to execute in.</param>
		/// <param name="parameters">sql parameters to pass with query</param>
		public CommandReader(Func<SqlDataReader, Task<bool>> func, string query, CommandType type, TimeSpan timeout, params SqlParameter[] parameters)
			: base(query, type, timeout, parameters) { _ReadFunc = func; }

		/// <summary>
		/// New Command that executes given sql and asynchronously calls given function for each returned row.
		/// </summary>
		/// <param name="func">async function to call for each row.  Return true to continue and false to stop processing. Each call is awaited.</param>
		/// <param name="query">Sql query to run</param>
		/// <param name="type">Type of query the command will run</param>
		/// <param name="timeout">Timeout for the query to execute in.</param>
		/// <param name="parameters">sql parameters to pass with query</param>
		public CommandReader(Func<SqlDataReader, Task<bool>> func, string query, CommandType type, int timeout, params SqlParameter[] parameters)
			: base(query, type, timeout, parameters) { _ReadFunc = func; }

		/// <summary>
		/// New Command that executes given sql and calls given function for each returned row.
		/// </summary>
		/// <param name="action">Function to call for each row, until all rows are processed.</param>
		/// <param name="query">Sql query to run</param>
		/// <param name="type">Type of query the command will run</param>
		/// <param name="timeout">Timeout for the query to execute in.</param>
		/// <param name="parameters">sql parameters to pass with query</param>
		public CommandReader(Action<SqlDataReader> action, string query, CommandType type, TimeSpan timeout, params SqlParameter[] parameters)
			: base(query, type, timeout, parameters) { _ReadFunc = (a) => { action(a); return Task.FromResult(true); }; }

		/// <summary>
		/// New Command that executes given sql and calls given function for each returned row.
		/// </summary>
		/// <param name="action">Function to call for each row, until all rows are processed.</param>
		/// <param name="query">Sql query to run</param>
		/// <param name="type">Type of query the command will run</param>
		/// <param name="timeout">Timeout for the query to execute in.</param>
		/// <param name="parameters">sql parameters to pass with query</param>
		public CommandReader(Action<SqlDataReader> action, string query, CommandType type, int timeout, params SqlParameter[] parameters)
			: base(query, type, timeout, parameters) { _ReadFunc = (a) => { action(a); return Task.FromResult(true); }; }

		/// <summary>
		/// New Command that executes given sql and calls given function for each returned row.
		/// </summary>
		/// <param name="action">async function to call for each row until all rows are processed.</param>
		/// <param name="query">Sql query to run</param>
		/// <param name="type">Type of query the command will run</param>
		/// <param name="timeout">Timeout for the query to execute in.</param>
		/// <param name="parameters">sql parameters to pass with query</param>
		public CommandReader(Func<SqlDataReader, Task> action, string query, CommandType type, TimeSpan timeout, params SqlParameter[] parameters)
			: base(query, type, timeout, parameters) { _ReadFunc = async (a) => { await action(a); return true; }; }

		/// <summary>
		/// New Command that executes given sql and calls given function for each returned row.
		/// </summary>
		/// <param name="action">async function to call for each row until all rows are processed.</param>
		/// <param name="query">Sql query to run</param>
		/// <param name="type">Type of query the command will run</param>
		/// <param name="timeout">Timeout for the query to execute in.</param>
		/// <param name="parameters">sql parameters to pass with query</param>
		public CommandReader(Func<SqlDataReader, Task> action, string query, CommandType type, int timeout, params SqlParameter[] parameters)
			: base(query, type, timeout, parameters) { _ReadFunc = async (a) => { await action(a); return true; }; }

		/// <summary>
		/// Event to call when all rows are processed.
		/// </summary>
		public event OnCompleteEventHandler OnComplete
		{
			add { _OnComplete += value; }
			remove { _OnComplete -= value; }
		}

		/// <summary>
		/// Execute sql command and call function
		/// </summary>
		/// <returns>number of records affected</returns>
		public override int Execute()
		{
			SqlDataReader reader = null;
			try
			{
				reader = InternalCommand.ExecuteReader();
			}
			catch (SqlException ex)
			{
				throw new CommandException(InternalCommand, ex);
			}
			_RecordsAffected = reader.RecordsAffected;
			try
			{
				while (reader.Read() && _ReadFunc != null && _ReadFunc(reader).Result) ;
			}
			finally
			{
				reader.Close();
			}
			if (_OnComplete != null) _OnComplete(this, EventArgs.Empty);
			return _RecordsAffected;
		}

		/// <summary>
		/// Asynchronously execute sql command and await call to function
		/// </summary>
		/// <param name="cancellationToken">a cancellation token</param>
		/// <returns>number of rows affected</returns>
		public override async System.Threading.Tasks.Task<int> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
		{
			SqlDataReader reader = null;
			try
			{
				reader = await InternalCommand.ExecuteReaderAsync(cancellationToken);
			}
			catch (SqlException ex)
			{
				throw new CommandException(InternalCommand, ex);
			}
			_RecordsAffected = reader.RecordsAffected;
			try
			{
				while (!cancellationToken.IsCancellationRequested && await reader.ReadAsync() && _ReadFunc != null && await _ReadFunc(reader)) ;
			}
			finally
			{
				await reader.CloseAsync();
			}
			if (_OnComplete != null) _OnComplete(this, EventArgs.Empty);
			return _RecordsAffected;
		}


		protected virtual System.Threading.Tasks.Task<bool> Read(SqlDataReader reader)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// Get the number of rows affected by execution of query.
		/// </summary>
		public int RecordsAffected
		{
			get { return _RecordsAffected; }
		}
	}
}
