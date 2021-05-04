using System;
using System.Data;
using System.Configuration;

namespace mcZen.Data
{
	/// <summary>
	/// Interface for a standard command to be executed using a connection factory
	/// </summary>
	public interface ICommand
	{
		/// <summary>
		/// Executes the current command
		/// </summary>
		/// <returns>integer value returned by command, typically row affected.</returns>
		int Execute();

		/// <summary>
		/// Initializes the current command
		/// </summary>
		/// <param name="conn">Opened Sql Connection</param>
		/// <param name="trans">Sql Transaction</param>
		void Initialize(Microsoft.Data.SqlClient.SqlConnection conn, Microsoft.Data.SqlClient.SqlTransaction trans);
	}

	/// <summary>
	/// Interface for an asynchronous command to be executed using a connection factory
	/// </summary>
	public interface ICommandAsync : ICommand
	{
		/// <summary>
		/// Executes the current command
		/// </summary>
		/// <returns>integer value returned by command, typically row affected.</returns>
		System.Threading.Tasks.Task<int> ExecuteAsync(System.Threading.CancellationToken cancellationToken);
	}
}
