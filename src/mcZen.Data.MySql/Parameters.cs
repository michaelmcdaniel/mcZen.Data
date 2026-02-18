using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using MySqlConnector;

namespace mcZen.Data.MySql
{
	/// <summary>
	/// Sql Parameter helper functions
	/// </summary>
	public static class Parameters
	{
		/// <summary>
		/// Creates a sql parameters with automatic length trimming
		/// </summary>
		/// <param name="column">column name</param>
		/// <param name="value">value</param>
		/// <param name="maxLen">max length of the string/column</param>
		/// <returns></returns>
		public static MySqlParameter Create(string column, string value, int maxLen)
		{
			if (value != null && value.Length > maxLen)
			{
				return new MySqlParameter(column, value.Substring(0, maxLen));
			}
			else
			{
				return new MySqlParameter(column, value);
			}
		}

		public static MySqlParameter Create(string column, MySqlDbType type)
		{
			return new MySqlParameter(column, type);
		}

		public static MySqlParameter Create(string column, object value)
		{
			if (value == null)
				return new MySqlParameter(column, DBNull.Value);
			return new MySqlParameter(column, value);
		}

		public static MySqlParameter Create(string column, string value)
		{
			MySqlParameter retVal = null;
			retVal = new MySqlParameter(column, MySqlDbType.VarChar);
			retVal.Value = (value == null) ? DBNull.Value : (object)value;
			return retVal;
		}

		public static MySqlParameter Create(string column, Guid guid)
		{
			return Create(column, guid, false);
		}

		public static MySqlParameter Create(string column, Guid guid, bool emptyAsDBNull)
		{
			if (emptyAsDBNull && Guid.Empty == guid)
				return Create(column, MySqlDbType.Guid, DBNull.Value);
			return new MySqlParameter(column, guid);
		}

		public static MySqlParameter Create<T>(string column, T value) where T: struct
		{
            return new MySqlParameter(column, value);
        }

        public static MySqlParameter Create<T>(string column, Nullable<T> value)
			where T : struct
		{
			if (!value.HasValue)
				return new MySqlParameter(column, DBNull.Value);
			return new MySqlParameter(column, value.Value);
		}

		public static MySqlParameter Create<T,V>(string column, T v, Func<T,V> func)
		{
			if (v == null) return new MySqlParameter(column, DBNull.Value);
			return Create(column, func(v));
		}

		public static MySqlParameter Create<T>(string column, List<T> list, T defaultValue)
		{
			return Create<T>(column, list, 0, defaultValue);
		}

		public static MySqlParameter Create<T>(string column, List<T> list, int index, T defaultValue)
		{
			if (list == null || index >= list.Count) return new MySqlParameter(column, defaultValue);
			return new MySqlParameter(column, list[index]);
		}

		public static MySqlParameter Create(string column, MySqlDbType dbType, object value)
		{
			MySqlParameter retVal = new MySqlParameter(column, dbType);
			retVal.Value = value;
			return retVal;
		}

		//public static MySqlParameter Create(string column, bool isTime, TimeStamp uts)
		//{
		//	MySqlParameter retVal = null;
		//	if (isTime)
		//	{
		//		retVal = new MySqlParameter(column, DbType.DateTime);
		//		retVal.Value = (uts.IsEmpty) ? (object)DBNull.Value : (object)uts.DateTime;
		//	}
		//	else
		//	{
		//		retVal = new MySqlParameter(column, DbType.Guid);
		//		retVal.Value = (uts.IsEmpty || string.IsNullOrEmpty(uts.UserID)) ? (object)DBNull.Value : (object)uts.UserID;
		//	}
		//	return retVal;
		//}

		public static MySqlParameter Create(string column, DateTime value)
		{
			DateTime date = value.Date;
			MySqlParameter retVal = new MySqlParameter(column, MySqlDbType.DateTime);
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

		public static MySqlParameter Create(string column, System.IO.Stream stream)
		{
			MySqlParameter retVal = new MySqlParameter(column, MySqlDbType.Blob);
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

		public static MySqlParameter Create(string column, MySqlDbType type, System.IO.Stream stream)
		{
			MySqlParameter retVal = new MySqlParameter(column, type);
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

		public static MySqlParameter Create(string column, System.Xml.XmlDocument doc)
		{
			MySqlParameter retVal = new MySqlParameter(column, MySqlDbType.Text);
			System.IO.MemoryStream stream = new System.IO.MemoryStream();
			doc.Save(stream);
			stream.Position = 0;
			retVal.Value = Encoding.UTF8.GetString(stream.ToArray());
			return retVal;
		}

		public static MySqlParameter Create(string column, System.Text.StringBuilder sb)
		{
			return Create(column, sb.ToString());
		}

	}
}
