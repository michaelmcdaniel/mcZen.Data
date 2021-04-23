using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Linq;

namespace mcZen.Data
{
	public static partial class Commands
	{
		public static mcZen.Data.IRequest InitializeSave(string table, int timeout, ref Guid id, SqlParameter key, params SqlParameter[] parameters)
		{
			string query;
			IEnumerable<SqlParameter> cc = parameters.Append(key);
			if (id == Guid.Empty)
			{
				key.Value = id = Guid.NewGuid();
				query = string.Format("INSERT INTO [{0}] ([{1}]) VALUES ({2})",
					table,
					string.Join("],[", (from p in cc select p.ParameterName.Substring(1))),
					string.Join(",", (from p in cc select p.ParameterName)));
			}
			else
			{
				query = string.Format("UPDATE [{0}] SET {1} WHERE {2}", 
					table,
					string.Join(",", (from p in parameters select "["+p.ParameterName.Substring(1)+"]="+p.ParameterName)), 
					"["+key.ParameterName.Substring(1)+"]="+key.ParameterName);
			}
			return new Data.CommandRequest(query, CommandType.Text, timeout, cc.ToArray());
		}

		public static mcZen.Data.IRequest Insert(string table, params SqlParameter[] parameters)
		{
			string query;
			IEnumerable<SqlParameter> cc = parameters;
				query = string.Format("INSERT INTO [{0}] ([{1}]) VALUES ({2})",
					table,
					string.Join("],[", (from p in cc select p.ParameterName.Substring(1))),
					string.Join(",", (from p in cc select p.ParameterName)));
			return new Data.CommandRequest(query, CommandType.Text, cc.ToArray());
		}

		public static mcZen.Data.IRequest InitializeSave(string table, SqlParameter key, params SqlParameter[] parameters)
		{
			string query;
			IEnumerable<SqlParameter> cc;
			if (key==null)
			{
				cc = parameters;
				query = string.Format("INSERT INTO [{0}] ([{1}]) VALUES ({2})",
					table,
					string.Join("],[", (from p in cc select p.ParameterName.Substring(1))),
					string.Join(",", (from p in cc select p.ParameterName)));
			}
			else
			{
				cc = parameters.Append(key);
				query = string.Format("UPDATE [{0}] SET {1} WHERE {2}", 
					table,
					string.Join(",", (from p in parameters select "["+p.ParameterName.Substring(1)+"]="+p.ParameterName)), 
					"["+key.ParameterName.Substring(1)+"]="+key.ParameterName);
			}
			return new Data.CommandRequest(query, CommandType.Text, cc.ToArray());
		}

		public static mcZen.Data.IRequest InitializeSave(string table, ref Guid id, SqlParameter key, params SqlParameter[] parameters)
		{
			string query;
			IEnumerable<SqlParameter> cc;
			if (id == Guid.Empty)
			{
				key.Value = (id = Guid.NewGuid());
				cc = parameters;
				query = string.Format("INSERT INTO [{0}] ([{1}]) VALUES ({2})",
					table,
					string.Join("],[", (from p in cc select p.ParameterName.Substring(1))),
					string.Join(",", (from p in cc select p.ParameterName)));
			}
			else
			{
				cc = parameters.Append(key);
				query = string.Format("UPDATE [{0}] SET {1} WHERE {2}", 
					table,
					string.Join(",", (from p in parameters select "["+p.ParameterName.Substring(1)+"]="+p.ParameterName)), 
					"["+key.ParameterName.Substring(1)+"]="+key.ParameterName);
			}
			return new Data.CommandRequest(query, CommandType.Text, cc.ToArray());
		}


		public static mcZen.Data.IRequest InitializeSave(string table, out bool update, object obj, SqlParameter key, SqlParameter createDate, SqlParameter createUser, SqlParameter modDate, SqlParameter modUser, params SqlParameter[] parameters)
		{
			StringBuilder query = new StringBuilder();
			//obj.Modified.Stamp();
			//modDate.Value = obj.Modified.DateTime;
			//modUser.Value = obj.Modified.UserID;

			List<SqlParameter> newParams = new List<SqlParameter>();
			if (Guid.Empty.Equals(key.Value))
			{
				//obj.Created.Stamp(obj.Modified);
				//createDate.Value = obj.Created.DateTime;
				//createUser.Value = obj.Created.UserID;

				update = false;
				//obj.ID = Guid.NewGuid();
				//key.Value = obj.ID;
				query.Append("INSERT INTO ");
				query.Append(table);
				query.Append(" ([");
				for (int i = 0; i < parameters.Length; i++)
				{
					newParams.Add(parameters[i]);
					query.Append(parameters[i].ParameterName.Substring(1));
					query.Append("],[");
				}
				query.Append(createDate.ParameterName.Substring(1));
				query.Append("],[");
				query.Append(createUser.ParameterName.Substring(1));
				query.Append("],[");
				query.Append(modDate.ParameterName.Substring(1));
				query.Append("],[");
				query.Append(modUser.ParameterName.Substring(1));
				query.Append("],[");
				query.Append(key.ParameterName.Substring(1));
				query.Append("]) VALUES (");
				for (int i = 0; i < parameters.Length; i++)
				{
					query.Append(parameters[i].ParameterName);
					query.Append(',');
				}
				query.Append(createDate.ParameterName);
				query.Append(',');
				query.Append(createUser.ParameterName);
				query.Append(',');
				query.Append(modDate.ParameterName);
				query.Append(',');
				query.Append(modUser.ParameterName);
				query.Append(',');
				query.Append(key.ParameterName);
				query.Append(')');
				newParams.Add(createDate);
				newParams.Add(createUser);
			}
			else
			{
				update = true;
				//key.Value = obj.ID;
				query.Append("UPDATE ");
				query.Append(table);
				query.Append(" SET ");
				for (int i = 0; i < parameters.Length; i++)
				{
					newParams.Add(parameters[i]);
					query.Append('[');
					query.Append(parameters[i].ParameterName.Substring(1));
					query.Append("]=");
					query.Append(parameters[i].ParameterName);
					query.Append(',');
				}

				query.Append("[");
				query.Append(modDate.ParameterName.Substring(1));
				query.Append("]=");
				query.Append(modDate.ParameterName);
				query.Append(",[");
				query.Append(modUser.ParameterName.Substring(1));
				query.Append("]=");
				query.Append(modUser.ParameterName);
				query.Append(" WHERE [");
				query.Append(key.ParameterName.Substring(1));
				query.Append("]=");
				query.Append(key.ParameterName);
			}
			newParams.Add(modDate);
			newParams.Add(modUser);
			newParams.Add(key);

			return new mcZen.Data.CommandRequest(query.ToString(), newParams.ToArray());
		}

		internal static void AppendAnd(ref bool first, System.Text.StringBuilder builder)
		{
			if (first)
			{
				first = !first;
			}
			else
			{
				builder.Append(" AND ");
			}
		}
	}
}
