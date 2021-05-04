using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace mcZen.Data
{
	/// <summary>
	/// Helper methods to generate standard sql queries
	/// </summary>
	public static class Queries
	{
		public static Regex s_StartsWithWhere = new Regex("\\s*WHERE\\s", RegexOptions.IgnoreCase);

		public static string ParametersToString(IEnumerable<SqlParameter> parameters)
		{
			StringBuilder retVal = new StringBuilder();
			foreach (var sp in parameters)
			{
				retVal.AppendFormat("{2}:{0}=\"{1}\"\r\n", sp.ParameterName, (sp.Value == null || sp.Value == DBNull.Value) ? "NULL" : sp.Value, sp.SqlDbType);
			}
			if (retVal.Length == 0) retVal.Append("None");
			return retVal.ToString();
		}

		public static string PagedQuery(IEnumerable<string> columns, string table, IEnumerable<string> joins, string filter, OrderBy orderBy, int page = 0, int size = -1, bool nolock = true)
		{
			return PagedQuery(string.Join(",", columns), table, joins, filter, orderBy, page, size, nolock);
		}

		/// <summary>
		/// returns a query that will return a paged result.  If no size is specified, then all results will be returned.
		/// </summary>
		/// <param name="columns">List of columns to include</param>
		/// <param name="table">Table name</param>
		/// <param name="joins">Full table joins [ JOIN TABLE_B ON TABLE_A.COLUMN=TABLE_B.COLUMN]</param>
		/// <param name="filter">filter for query [[WHERE] X=Y]</param>
		/// <param name="orderBy">REQUIRED: order by statement</param>
		/// <param name="page">zero based page index</param>
		/// <param name="size">size of a page</param>
		/// <param name="nolock">True if the given table should be nolocked</param>
		/// <returns></returns>
		public static string PagedQuery(string column, string table, IEnumerable<string> joins, string filter, OrderBy orderBy, int page = 0, int size = -1, bool nolock = true)
		{
			if (size > 0 && orderBy.Count == 0) throw new ArgumentOutOfRangeException("orderBy", "orderBy is required when size is specified");
			if (string.IsNullOrWhiteSpace(column)) column = "*";

			StringBuilder retVal = new StringBuilder();

			retVal.Append("SELECT ");
			if (size > 0)
			{
				retVal.Append("* FROM (SELECT ROW_NUMBER() OVER (ORDER BY ");
				retVal.Append(orderBy.ToString());
				retVal.Append(") AS RowNum, ");
			}
			retVal.Append(column);
			retVal.Append(" FROM ");
			retVal.Append(table);
			retVal.Append(nolock ? " (NOLOCK) " : " ");
			if (joins != null) retVal.Append(string.Join(" ", joins));
			if (!string.IsNullOrWhiteSpace(filter))
			{
				if (!s_StartsWithWhere.IsMatch(filter)) retVal.Append(" WHERE ");
				retVal.Append(filter);
			}
			if (size > 0)
			{
				retVal.Append(") [SortedQuery] WHERE [RowNum] >= ");
				retVal.Append((page * size + 1).ToString());
				retVal.Append(" AND [RowNum] < ");
				retVal.Append((page * size + 1 + size).ToString());
				retVal.Append(" ORDER BY [RowNum]");
			}
			else if (orderBy.Count > 0)
			{
				retVal.Append(" ORDER BY ");
				retVal.Append(orderBy.ToString());
			}
			return retVal.ToString();
		}

		public static string SelectCount(string table, string filter, IEnumerable<string> joins = null, bool nolock = true)
		{
			return Select("COUNT(*)", table, joins, filter, null, 0, nolock);
		}

		public static string Select(IEnumerable<string> columns, string table, IEnumerable<string> joins, string filter, OrderBy orderBy, int top = 0, bool nolock = true)
		{
			return Select(string.Join(",", columns), table, joins, filter, orderBy, top, nolock);
		}

		public static string Select(string column, string table, IEnumerable<string> joins, string filter, OrderBy orderBy, int top = 0, bool nolock = true)
		{
			if (string.IsNullOrWhiteSpace(column)) column = "*";

			StringBuilder retVal = new StringBuilder();

			retVal.Append("SELECT ");
			if (top > 0)
			{
				retVal.Append("TOP ");
				retVal.Append(top);
				retVal.Append(' ');
			}
			retVal.Append(column);
			retVal.Append(" FROM ");
			retVal.Append(table);
			retVal.Append(nolock ? " (NOLOCK) " : " ");
			if (joins != null) retVal.Append(string.Join(" ", joins) + " ");
			if (!string.IsNullOrWhiteSpace(filter))
			{
				if (!s_StartsWithWhere.IsMatch(filter)) retVal.Append(" WHERE ");
				retVal.Append(filter);
			}
			if (orderBy != null && orderBy.Count > 0)
			{
				retVal.Append(" ORDER BY ");
				retVal.Append(orderBy.ToString());
			}
			return retVal.ToString();
		}

	}
}
