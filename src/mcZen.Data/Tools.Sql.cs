using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Linq;

namespace mcZen
{
	public static partial class Tools
	{
		public static class Sql
		{
			public static SqlParameter CreateParameter(string column, string value, int maxLen)
			{
				if (value != null && value.Length > maxLen)
				{
					return new SqlParameter(column, value.Substring(0, maxLen));
				}
				else
				{
					return new SqlParameter(column, value);
				}
			}

			public static SqlParameter CreateParameter(string column, SqlDbType type)
			{
				return new SqlParameter(column, type);
			}

			public static SqlParameter CreateParameter(string column, object value)
			{
				if (value == null)
					return new SqlParameter(column, DBNull.Value);
				return new SqlParameter(column, value);
			}

			public static SqlParameter CreateParameter(string column, string value)
			{
				SqlParameter retVal = null;
				retVal = new SqlParameter(column, SqlDbType.NVarChar);
				retVal.Value = (value == null)?DBNull.Value:(object)value;
				return retVal;
			}

			public static SqlParameter CreateParameter(string column, Guid guid)
			{
				return CreateParameter(column, guid, false);
			}

			public static SqlParameter CreateParameter(string column, Guid guid, bool emptyAsDBNull)
			{
				if (emptyAsDBNull && Guid.Empty == guid)
					return CreateParameter(column, SqlDbType.UniqueIdentifier, DBNull.Value);
				return new SqlParameter(column, guid);
			}

			public static SqlParameter CreateParameter<T>(string column, Nullable<T> value)
				where T : struct
			{
				if (!value.HasValue)
					return new SqlParameter(column, DBNull.Value);
				return new SqlParameter(column, value.Value);
			}

			public delegate object Get<T>(T t);
			public static SqlParameter CreateParameter<T>(string column, T v, Get<T> del)
			{
				if (v == null) return new SqlParameter(column, DBNull.Value);
				return CreateParameter(column, del(v));
			}

			public static SqlParameter CreateParameter<T>(string column, List<T> list, T defaultValue)
			{
				return CreateParameter<T>(column, list, 0, defaultValue);
			}

			public static SqlParameter CreateParameter<T>(string column, List<T> list, int index, T defaultValue)
			{
				if (list == null || index >= list.Count) return new SqlParameter(column, defaultValue);
				return new SqlParameter(column, list[index]);
			}

			public static SqlParameter CreateParameter(string column, SqlDbType dbType, object value)
			{
				SqlParameter retVal = new SqlParameter(column, dbType);
				retVal.Value = value;
				return retVal;
			}

			//public static SqlParameter CreateParameter(string column, bool isTime, TimeStamp uts)
			//{
			//	SqlParameter retVal = null;
			//	if (isTime)
			//	{
			//		retVal = new SqlParameter(column, DbType.DateTime);
			//		retVal.Value = (uts.IsEmpty) ? (object)DBNull.Value : (object)uts.DateTime;
			//	}
			//	else
			//	{
			//		retVal = new SqlParameter(column, DbType.Guid);
			//		retVal.Value = (uts.IsEmpty || string.IsNullOrEmpty(uts.UserID)) ? (object)DBNull.Value : (object)uts.UserID;
			//	}
			//	return retVal;
			//}

			public static SqlParameter CreateParameter(string column, DateTime value)
			{
				DateTime date = value.Date;
				SqlParameter retVal = new SqlParameter(column, SqlDbType.DateTime);
				if (date.Equals(DateTime.MinValue.Date) || date.Equals(DateTime.MaxValue.Date))
				{
					retVal.Value = DBNull.Value;
				}
				else
				{
					retVal.Value = value;
				}
				return retVal;
			}

			public static SqlParameter CreateParameter(string column, System.IO.Stream stream)
			{
				SqlParameter retVal = new SqlParameter(column, SqlDbType.Image);
				if (stream == null || stream.Length == 0)
				{
					retVal.Value = DBNull.Value;
				}
				else
				{
					byte[] data = new byte[stream.Length];
					stream.Read(data, 0, (int)stream.Length);
					retVal.Value = data;
				}
				return retVal;
			}

			public static SqlParameter CreateParameter(string column, SqlDbType type, System.IO.Stream stream)
			{
				SqlParameter retVal = new SqlParameter(column, type);
				if (stream == null || stream.Length == 0)
				{
					retVal.Value = DBNull.Value;
				}
				else
				{
					byte[] data = new byte[stream.Length];
					stream.Read(data, 0, (int)stream.Length);
					retVal.Value = data;
				}
				return retVal;
			}

			public static SqlParameter CreateParameter(string column, System.Xml.XmlDocument doc)
			{
				SqlParameter retVal = new SqlParameter(column, SqlDbType.Xml);
				System.IO.MemoryStream stream = new System.IO.MemoryStream();
				doc.Save(stream);
				stream.Position = 0;
				retVal.Value = new System.Data.SqlTypes.SqlXml(stream);
				return retVal;
			}

			public static SqlParameter CreateParameter(string column, System.Text.StringBuilder sb)
			{
				return CreateParameter(column, sb.ToString());
			}

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

			public static void AppendAnd(ref bool first, System.Text.StringBuilder builder)
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
}
