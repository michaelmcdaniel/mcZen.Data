using System;
using System.Collections.Generic;
using System.Text;

namespace mcZen.Data
{
	public class ConnectionFactory
	{
		private string _ConnectionString;
		public List<IRequest> _Requests = new List<IRequest>();

		public ConnectionFactory(string connString)
		{
			_ConnectionString = connString;
		}

		public static int Execute(IRequest request, string connString)
		{
			ConnectionFactory factory = new ConnectionFactory(connString);
			return factory.Execute(request)[0];
		}
		
		public ScalarRequest<T> Register<T>(T defaultValue, string query, params Microsoft.Data.SqlClient.SqlParameter[] sqlParameters)
		{
			ScalarRequest<T> retVal = new ScalarRequest<T>(defaultValue, query, sqlParameters);
			_Requests.Add(retVal);
			return retVal;
		}

		public int Register(IRequest request)
		{
			_Requests.Add(request);
			return _Requests.Count - 1;
		}

		public int Register(List<IRequest> request)
		{
			_Requests.AddRange(request);
			return _Requests.Count - 1;
		}

		public int Register(string query, params Microsoft.Data.SqlClient.SqlParameter[] sqlParameters)
		{
			_Requests.Add(new CommandRequest(query, sqlParameters));
			return _Requests.Count - 1;
		}



		public List<int> Execute(IRequest request)
		{
			Register(request);
			return Execute();
		}

		public List<int> Execute()
		{
			List<int> retVal = new List<int>();
			using (Microsoft.Data.SqlClient.SqlConnection conn = new Microsoft.Data.SqlClient.SqlConnection(_ConnectionString))
			{
				conn.Open();
				Microsoft.Data.SqlClient.SqlTransaction trans = conn.BeginTransaction();
				try
				{
					int initialized = _Requests.Count;
					for (int i = 0; i < initialized; i++)
					{
						if (_Requests[i] != null)
							_Requests[i].Initialize(conn, trans);
					}
					for (int i = 0; i < _Requests.Count; i++)
					{
						if (_Requests[i] != null)
						{
							//  This following line allows for chaining.  
							//  in other words, a executing request can add more requests.
							if (i >= initialized)
								_Requests[i].Initialize(conn, trans);
							retVal.Add(_Requests[i].Execute());
						}
						else
							retVal.Add(1);
					}
					trans.Commit();
				}
				catch (Exception)
				{
					try { trans.Rollback(); } catch(Exception) {}
					throw;
				}
				finally
				{
					_Requests.Clear();
				}
			}
			return retVal;
		}

		public static int Execute(string query, int timeout, string connectionString, params Microsoft.Data.SqlClient.SqlParameter[] parameters)
		{
			return Execute(new CommandRequest(query, System.Data.CommandType.Text, timeout, parameters), connectionString);
		}
	}
}
