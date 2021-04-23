using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace mcZen.Data
{
	/// <summary>
	/// Summary description for CommandReaderRequest
	/// </summary>
	public class CommandReaderRequest : CommandRequest
	{
		private int _RecordsAffected = 0;
		private Func<SqlDataReader, System.Threading.Tasks.Task<bool>> _ReadFunc;
		private event OnCompleteEventHandler _OnComplete;

		public CommandReaderRequest(string query, params SqlParameter[] parameters)
			: base(query, parameters) { _ReadFunc = Read; }

		public CommandReaderRequest(Func<SqlDataReader, bool> func, string query, params SqlParameter[] parameters)
			: base(query, parameters) { _ReadFunc = (r)=>Task.FromResult(func(r)); }

		public CommandReaderRequest(Func<SqlDataReader, bool> func, string query, CommandType type, int timeout, params SqlParameter[] parameters)
			: base(query, type, timeout, parameters) { _ReadFunc = (r) => Task.FromResult(func(r)); }

		public CommandReaderRequest(Func<SqlDataReader, Task<bool>> func, string query, CommandType type, int timeout, params SqlParameter[] parameters)
			: base(query, type, timeout, parameters) { _ReadFunc = func; }

		public CommandReaderRequest(Action<SqlDataReader> action, string query, CommandType type, int timeout, params SqlParameter[] parameters)
			: base(query, type, timeout, parameters) { _ReadFunc = (a) => { action(a); return Task.FromResult(true); }; }

		public CommandReaderRequest(Func<SqlDataReader, Task> action, string query, CommandType type, int timeout, params SqlParameter[] parameters)
			: base(query, type, timeout, parameters) { _ReadFunc = async (a) => { await action(a); return true; }; }

		public event OnCompleteEventHandler OnComplete
		{
			add { _OnComplete += value; }
			remove { _OnComplete -= value; }
		}

		public override int Execute()
		{
			SqlDataReader reader = null;
			try
			{
				reader = Command.ExecuteReader();
			}
			catch (SqlException ex)
			{
				throw new RequestException(Command, ex);
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

		public override async System.Threading.Tasks.Task<int> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
		{
			SqlDataReader reader = null;
			try
			{
				reader = await Command.ExecuteReaderAsync(cancellationToken);
			}
			catch (SqlException ex)
			{
				throw new RequestException(Command, ex);
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

		public int RecordsAffected
		{
			get { return _RecordsAffected; }
		}
	}
}
