using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;

namespace mcZen.Data
{
	public static class SqlDataReaderExtensions
	{

		public static T GetValue<T>(this SqlDataReader reader, string column, T defaultValue)
		{
			object value = reader[column];
			if (value == DBNull.Value)
				return defaultValue;
			return (T)value;
		}

		public static T GetValue<T>(this SqlDataReader reader, int columnIndex, T defaultValue)
		{
			if (columnIndex < 0) return defaultValue;
			object value = reader[columnIndex];
			if (value == DBNull.Value)
				return defaultValue;
			return (T)value;
		}

		public static Nullable<T> GetValue<T>(this SqlDataReader reader, string column) where T : struct
		{
			object value = reader[column];
			if (value == DBNull.Value)
				return null;
			return (T)value;
		}

		public static Nullable<T> GetValue<T>(this SqlDataReader reader, int columnIndex) where T : struct
		{
			if (columnIndex < 0) return null;
			object value = reader[columnIndex];
			if (value == DBNull.Value)
				return null;
			return (T)value;
		}

		public static DateTime GetValue(this SqlDataReader reader, string column, DateTime defaultValue, DateTimeKind kind)
		{
			object value = reader[column];
			if (value == DBNull.Value)
				return defaultValue;
			if (value is DateTimeOffset)
			{
				DateTimeOffset dtoValue = (DateTimeOffset)value;
				if (kind == DateTimeKind.Local) return dtoValue.LocalDateTime;
				else if (kind == DateTimeKind.Utc) return dtoValue.UtcDateTime;
				else value = dtoValue.DateTime;
			}
			DateTime tmp = (DateTime)value;
			return new DateTime(tmp.Year, tmp.Month, tmp.Day, tmp.Hour, tmp.Minute, tmp.Second, tmp.Millisecond, kind);
		}

		public static DateTimeOffset GetValue(this SqlDataReader reader, string column, DateTimeOffset defaultValue)
		{
			object value = reader[column];
			if (value == DBNull.Value)
				return defaultValue;
			if (value is DateTime)
			{
				DateTime tmp = (DateTime)value;
				return new DateTimeOffset(tmp.Year, tmp.Month, tmp.Day, tmp.Hour, tmp.Minute, tmp.Second, tmp.Millisecond, TimeSpan.Zero);
			}
			return (DateTimeOffset)value;
		}

		/// <summary>
		/// Takes one time and makes it the Kind given
		/// This does not change the time.
		/// </summary>
		internal static DateTime Make(this DateTime dt, DateTimeKind dtk)
		{
			return new DateTime(dt.Ticks, dtk);
		}

		/// <summary>
		/// Returns the time in the timezone based off the ticks. (looses timezone information!!!)
		/// </summary>
		/// <param name="dt">The date time to convert. (looses any timezone information)</param>
		/// <param name="tzi">The target timezone for the returned time to be in.</param>
		/// <param name="forceAmbiguous2DST">If the absolute time is ambiguous in the target timezone, this forces the time to DST.</param>
		/// <returns>The absolute time in the target Timezone</returns>
		internal static DateTimeOffset Make(this DateTimeOffset dt, TimeZoneInfo tzi, bool forceAmbiguous2DST)
		{
			return (new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, DateTimeKind.Unspecified)).Make(tzi, forceAmbiguous2DST);
		}

		internal static DateTimeOffset Make(this DateTime dt, TimeZoneInfo tzi, bool forceAmbiguous2DST)
		{
			DateTimeOffset retVal;
			switch (dt.Kind)
			{
				case DateTimeKind.Local:
					retVal = TimeZoneInfo.ConvertTime(dt.Make(DateTimeKind.Unspecified).Make(TimeZoneInfo.Local, forceAmbiguous2DST), tzi);
					break;
				case DateTimeKind.Utc:
					retVal = TimeZoneInfo.ConvertTime(dt.Make(DateTimeKind.Unspecified).Make(TimeZoneInfo.Utc, forceAmbiguous2DST), tzi);
					break;
				case DateTimeKind.Unspecified:
				default:
					TimeSpan adjustment = TimeSpan.Zero;
					if (tzi.IsInvalidTime(dt)) // in the spring between 2:00-2:59am
					{
						// we have a problem and cannot convert.
						throw new ArgumentException("DateTime represents an invalid time during the Daylight Savings Transition.", "dt");
					}
					if (tzi.IsAmbiguousTime(dt)) // only when fall back to std time use the force
					{
						if (forceAmbiguous2DST) adjustment = Time.GetDaylightRule(dt, tzi).DaylightDelta;
					}
					else if (tzi.IsDaylightSavingTime(dt))
					{
						adjustment = Time.GetDaylightRule(dt, tzi).DaylightDelta;
					}
					retVal = new DateTimeOffset(dt.Ticks, tzi.BaseUtcOffset + adjustment);
					break;
			}
			return retVal;
		}

		/// <summary>
		/// Creates a DateTimeOffset in the given timezone.  This function expects the sql value to be UTC.
		/// </summary>
		/// <param name="reader">The Sql Reader</param>
		/// <param name="column">The column that has a UTC DateTime</param>
		/// <param name="defaultValue">if the column is null, this value is returned</param>
		/// <param name="tz">The TimeZone to return the value in</param>
		/// <returns>The DateTime in the given Timezone with the correct offset from utc.</returns>
		public static DateTimeOffset GetValue(this SqlDataReader reader, string column, DateTimeOffset defaultValue, TimeZoneInfo tz)
		{
			if (tz == null) throw new ArgumentNullException("Timezone is required.", "tz");
			object value = reader[column];
			if (value == DBNull.Value) return defaultValue;
			DateTime dt = ((DateTime)value).Make(DateTimeKind.Utc);
			return ((DateTime)value).Make(DateTimeKind.Utc).Make(tz, false);
		}

		public static string GetValue(this SqlDataReader reader, string column, string defaultValue, bool defaultIfEmpty)
		{
			string value = GetValue<string>(reader, column, defaultValue);
			if (defaultIfEmpty && string.IsNullOrEmpty(value)) return defaultValue;
			return value;
		}

		public static T GetEnum<T>(this SqlDataReader reader, string column, T defaultValue)
		{
			object value = reader[column];
			if (value == DBNull.Value)
				return defaultValue;
			try
			{
				return (T)Enum.ToObject(typeof(T), value);
			}
			catch (ArgumentException)
			{
			}
			return defaultValue;
		}


	}
}
