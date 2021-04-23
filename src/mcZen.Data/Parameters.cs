using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;

namespace mcZen.Data
{
	public static class Parameters
	{
		/// <summary>
		/// Creates a sql parameters with automatic length trimming
		/// </summary>
		/// <param name="column">column name</param>
		/// <param name="value">value</param>
		/// <param name="maxLen">max length of the string/column</param>
		/// <returns></returns>
		public static SqlParameter Create(string column, string value, int maxLen)
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

		public static SqlParameter Create(string column, SqlDbType type)
		{
			return new SqlParameter(column, type);
		}

		public static SqlParameter Create(string column, object value)
		{
			if (value == null)
				return new SqlParameter(column, DBNull.Value);
			return new SqlParameter(column, value);
		}

		public static SqlParameter Create(string column, string value)
		{
			SqlParameter retVal = null;
			retVal = new SqlParameter(column, SqlDbType.NVarChar);
			retVal.Value = (value == null) ? DBNull.Value : (object)value;
			return retVal;
		}

		public static SqlParameter Create(string column, Guid guid)
		{
			return Create(column, guid, false);
		}

		public static SqlParameter Create(string column, Guid guid, bool emptyAsDBNull)
		{
			if (emptyAsDBNull && Guid.Empty == guid)
				return Create(column, SqlDbType.UniqueIdentifier, DBNull.Value);
			return new SqlParameter(column, guid);
		}

		public static SqlParameter Create<T>(string column, Nullable<T> value)
			where T : struct
		{
			if (!value.HasValue)
				return new SqlParameter(column, DBNull.Value);
			return new SqlParameter(column, value.Value);
		}

		public static SqlParameter Create<T,V>(string column, T v, Func<T,V> func)
		{
			if (v == null) return new SqlParameter(column, DBNull.Value);
			return Create(column, func(v));
		}

		public static SqlParameter Create<T>(string column, List<T> list, T defaultValue)
		{
			return Create<T>(column, list, 0, defaultValue);
		}

		public static SqlParameter Create<T>(string column, List<T> list, int index, T defaultValue)
		{
			if (list == null || index >= list.Count) return new SqlParameter(column, defaultValue);
			return new SqlParameter(column, list[index]);
		}

		public static SqlParameter Create(string column, SqlDbType dbType, object value)
		{
			SqlParameter retVal = new SqlParameter(column, dbType);
			retVal.Value = value;
			return retVal;
		}

		//public static SqlParameter Create(string column, bool isTime, TimeStamp uts)
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

		public static SqlParameter Create(string column, DateTime value)
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

		public static SqlParameter Create(string column, System.IO.Stream stream)
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

		public static SqlParameter Create(string column, SqlDbType type, System.IO.Stream stream)
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

		public static SqlParameter Create(string column, System.Xml.XmlDocument doc)
		{
			SqlParameter retVal = new SqlParameter(column, SqlDbType.Xml);
			System.IO.MemoryStream stream = new System.IO.MemoryStream();
			doc.Save(stream);
			stream.Position = 0;
			retVal.Value = new System.Data.SqlTypes.SqlXml(stream);
			return retVal;
		}

		public static SqlParameter Create(string column, System.Text.StringBuilder sb)
		{
			return Create(column, sb.ToString());
		}

	}
}
