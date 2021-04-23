using System;
using System.Collections.Generic;
using System.Text;

namespace mcZen.Data
{
	internal static class Tools
	{
		public static TT GenericConvert<FT, TT>(FT value)
		{
			System.ComponentModel.TypeConverter converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(TT));
			if (converter.CanConvertFrom(typeof(FT)))
			{
				return (TT)converter.ConvertFrom(value);
			}
			converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(FT));
			if (converter.CanConvertTo(typeof(TT)))
			{
				return (TT)converter.ConvertTo(value, typeof(TT));
			}

			throw new FormatException(string.Format("Cannot convert from type: \"{0}\" to type: \"{1}\"", typeof(FT).Name, typeof(TT).Name));
		}

		public static TT GenericConvert<FT, TT>(FT value, TT defaultValue)
		{
			if (value == null) return defaultValue;
			System.ComponentModel.TypeConverter converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(TT));
			if (converter.CanConvertFrom(typeof(FT)))
			{
				return (TT)converter.ConvertFrom(value);
			}
			converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(FT));
			if (converter.CanConvertTo(typeof(TT)))
			{
				return (TT)converter.ConvertTo(value, typeof(TT));
			}

			return defaultValue;
		}

		public static TT GenericConvert<TT>(object value, TT defaultValue)
		{
			if (value == null) return defaultValue;
			System.ComponentModel.TypeConverter converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(TT));
			Type ft = value.GetType();
			if (converter.CanConvertFrom(ft))
			{
				return (TT)converter.ConvertFrom(value);
			}
			converter = System.ComponentModel.TypeDescriptor.GetConverter(ft);
			if (converter.CanConvertTo(typeof(TT)))
			{
				return (TT)converter.ConvertTo(value, typeof(TT));
			}

			return defaultValue;
		}

		public static DateTime GenericConvert(string value, DateTime defaultValue)
		{
			DateTime retVal;
			if (string.IsNullOrEmpty(value) || !DateTime.TryParse(value, out retVal)) retVal = defaultValue;
			return retVal;
		}

		public static string GenericConvert(DateTime value, string defaultValue)
		{
			return value.ToString("MM/dd/yyyy HH:mm:ss");
		}
	}
}
