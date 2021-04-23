using System;
using System.Data;
using System.Configuration;

namespace mcZen.Data
{
	/// <summary>
	/// Summary description for IRequest
	/// </summary>
	public interface IRequest
	{
		int Execute();
		void Initialize(Microsoft.Data.SqlClient.SqlConnection conn, Microsoft.Data.SqlClient.SqlTransaction trans);
	}
}
