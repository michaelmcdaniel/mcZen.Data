using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace mcZen.Data
{
	/// <summary>
	/// The connection factory maintains a list of sql commands to execute within a single transaction
	/// </summary>
	public class ConnectionFactory
	{
		private string _ConnectionString;
		public List<ICommand> _Commands = new List<ICommand>();

		/// <summary>
		/// Constructor 
		/// </summary>
		/// <param name="connString">Sql Connection string</param>
		public ConnectionFactory(string connectionString)
		{
			_ConnectionString = connectionString;
		}

		/// <summary>
		/// Executes the given command using the given connection string
		/// </summary>
		/// <param name="command">Command to execute</param>
		/// <param name="connectionString">Sql connection string</param>
		/// <returns>integer from executing the command</returns>
		public static int Execute(ICommand command, string connectionString)
		{
			ConnectionFactory factory = new ConnectionFactory(connectionString);
			return factory.Execute(command)[0];
		}

		/// <summary>
		/// Asynchronously executes the given command using the given connection string
		/// </summary>
		/// <param name="command">Command to execute</param>
		/// <param name="connectionString">Sql connection string</param>
		/// <returns>integer from executing the command</returns>
		public static async System.Threading.Tasks.Task<int> ExecuteAsync(ICommand command, string connString)
		{
			ConnectionFactory factory = new ConnectionFactory(connString);
			factory.Register(command);
			return (await factory.ExecuteAsync()).First();
		}

		/// <summary>
		/// Registers a scalar command to be executed.
		/// </summary>
		/// <typeparam name="T">Type of the return value from a sql statement</typeparam>
		/// <param name="defaultValue">The default value returned if the sql statement returns null</param>
		/// <param name="query">Sql query that returns exactly 1 row with 1 column</param>
		/// <param name="sqlParameters">list of sql parameters to pass with the query</param>
		/// <returns>ScalarCommand that provides access to value upon execution.</returns>
		public ScalarCommand<T> Register<T>(T defaultValue, string query, params Microsoft.Data.SqlClient.SqlParameter[] sqlParameters)
		{
			ScalarCommand<T> retVal = new ScalarCommand<T>(defaultValue, query, sqlParameters);
			_Commands.Add(retVal);
			return retVal;
		}

		/// <summary>
		/// Registers a command to be executed.
		/// </summary>
		/// <param name="command">The command that will be executed</param>
		/// <returns>Index of the command</returns>
		public int Register(ICommand command)
		{
			_Commands.Add(command);
			return _Commands.Count - 1;
		}

		/// <summary>
		/// Registers a collection of commands to execute, in order
		/// </summary>
		/// <param name="command">list of commands</param>
		/// <returns>Index of last command</returns>
		public int Register(List<ICommand> commands)
		{
			_Commands.AddRange(commands);
			return _Commands.Count - 1;
		}

		/// <summary>
		/// Registers a sql text command to execute that performs some operation on data like an insert or update.
		/// </summary>
		/// <param name="query">sql query</param>
		/// <param name="sqlParameters">sql parameters to send with sql statement</param>
		/// <returns>Index of the command</returns>
		public int Register(string query, params Microsoft.Data.SqlClient.SqlParameter[] sqlParameters)
		{
			_Commands.Add(new Command(query, sqlParameters));
			return _Commands.Count - 1;
		}


		/// <summary>
		/// Registers then executes the current collection of registered commands
		/// </summary>
		/// <param name="command">A command to register</param>
		/// <returns>List of integers returned from each registered command</returns>
		public List<int> Execute(ICommand command)
		{
			Register(command);
			return Execute();
		}

		/// <summary>
		/// Registers then asynchronously executes the current collection of registered commands
		/// </summary>
		/// <param name="command">A command to register</param>
		/// <returns>List of integers returned from each registered command</returns>
		public async Task<IEnumerable<int>> ExecuteAsync(ICommandAsync command)
		{
			Register(command);
			return await ExecuteAsync();
		}

		/// <summary>
		/// Executes the current collection of registered commands
		/// </summary>
		/// <remarks>
		/// Opens the connection, begins a transaction, initializes all command, then executes each command.  Finally commits transaction.
		/// Upon failure, transaction is rolled-back.  Null commands automatically return 1 for execution.
		/// </remarks>
		/// <returns>List of integers returned from each registered command</returns>
		public List<int> Execute()
		{
			List<int> retVal = new List<int>();
			using (Microsoft.Data.SqlClient.SqlConnection conn = new Microsoft.Data.SqlClient.SqlConnection(_ConnectionString))
			{
				conn.Open();
				Microsoft.Data.SqlClient.SqlTransaction trans = conn.BeginTransaction();
				try
				{
					int initialized = _Commands.Count;
					for (int i = 0; i < initialized; i++)
					{
						if (_Commands[i] != null)
							_Commands[i].Initialize(conn, trans);
					}
					for (int i = 0; i < _Commands.Count; i++)
					{
						if (_Commands[i] != null)
						{
							//  This following line allows for chaining.  
							//  in other words, a executing command can add more commands.
							if (i >= initialized)
								_Commands[i].Initialize(conn, trans);
							retVal.Add(_Commands[i].Execute());
						}
						else
							retVal.Add(1);
					}
					trans.Commit();
				}
				catch (Exception)
				{
					try { trans.Rollback(); } catch (Exception) { }
					throw;
				}
				finally
				{
					_Commands.Clear();
				}
			}
			return retVal;
		}

		/// <summary>
		/// Asychronously executes the current collection of registered commands
		/// </summary>
		/// <remarks>
		/// Opens the connection, begins a transaction, initializes all commands, then serially awaits the execution of each command (if async.)  Finally commits transaction.
		/// Upon failure, transaction is rolled-back.  Null commands automatically return 1 for excution.
		/// </remarks>
		/// <returns>List of integers returned from each registered command</returns>
		public async Task<IEnumerable<int>> ExecuteAsync(System.Threading.CancellationToken cancellationToken = default)
		{
			List<int> retVal = new List<int>();
			using (Microsoft.Data.SqlClient.SqlConnection conn = new Microsoft.Data.SqlClient.SqlConnection(_ConnectionString))
			{
				await conn.OpenAsync();
				var trans = (Microsoft.Data.SqlClient.SqlTransaction)await conn.BeginTransactionAsync(cancellationToken);
				try
				{
					int initialized = _Commands.Count;
					for (int i = 0; i < initialized; i++)
					{
						if (_Commands[i] != null)
							_Commands[i].Initialize(conn, trans);
					}
					for (int i = 0; i < _Commands.Count; i++)
					{
						if (_Commands[i] != null)
						{
							//  This following line allows for chaining.  
							//  in other words, a executing command can add more commands.
							if (i >= initialized)
								_Commands[i].Initialize(conn, trans);
							if (_Commands[i] is ICommandAsync)
							{
								retVal.Add(await ((ICommandAsync)_Commands[i]).ExecuteAsync(cancellationToken));
							}
							else
							{
								retVal.Add(_Commands[i].Execute());
							}
						}
						else
							retVal.Add(1);
					}
					await trans.CommitAsync();
				}
				catch (Exception)
				{
					try { await trans.RollbackAsync(); } catch (Exception) { }
					throw;
				}
				finally
				{
					_Commands.Clear();
				}
			}
			return (IEnumerable<int>)retVal;
		}

		/// <summary>
		/// Executes sql statement
		/// </summary>
		/// <param name="query">Sql query to run</param>
		/// <param name="timeout">timeout for the query</param>
		/// <param name="connectionString">Sql connection string</param>
		/// <param name="parameters">Sql parameters to pass with query</param>
		/// <returns>integer from result of sql query (like rows affected)</returns>
		public static int Execute(string query, int timeout, string connectionString, params Microsoft.Data.SqlClient.SqlParameter[] parameters)
		{
			return Execute(new Command(query, System.Data.CommandType.Text, timeout, parameters), connectionString);
		}

	}
}
