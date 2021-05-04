using System;
using System.Collections.Generic;
using System.Text;

namespace mcZen.Data
{
	internal static class Time
	{
		static Dictionary<string, TimeZoneInfo> s_TimeZones = new Dictionary<string, TimeZoneInfo>(StringComparer.InvariantCultureIgnoreCase);
		static Time()
		{
			TimeZoneInfo.ClearCachedData();
			System.Collections.ObjectModel.ReadOnlyCollection<TimeZoneInfo> stz = TimeZoneInfo.GetSystemTimeZones();
			foreach (TimeZoneInfo tzi in stz)
			{
				s_TimeZones.Add(tzi.Id, tzi);
			}
		}

		public static TimeZoneInfo GetTimeZone(string id)
		{
			TimeZoneInfo retVal;
			if (!s_TimeZones.TryGetValue(id, out retVal))
				retVal = TimeZoneInfo.Utc;
			return retVal;
		}


		public static TimeZoneInfo.AdjustmentRule GetDaylightRule(DateTime dt, TimeZoneInfo tzi)
		{
			TimeZoneInfo.AdjustmentRule[] rules = tzi.GetAdjustmentRules();
			foreach (TimeZoneInfo.AdjustmentRule rule in rules)
			{
				if (rule.DateStart.Date < dt.Date && dt.Date < rule.DateEnd.Date)
				{
					return rule;
				}
			}
			return null;
		}

		public static TimeZoneInfo.AdjustmentRule GetDaylightRule(DateTimeOffset dt, TimeZoneInfo tzi)
		{
			TimeZoneInfo.AdjustmentRule[] rules = tzi.GetAdjustmentRules();
			foreach (TimeZoneInfo.AdjustmentRule rule in rules)
			{
				if (rule.DateStart.Date < dt.Date && dt.Date < rule.DateEnd.Date)
				{
					return rule;
				}
			}
			return null;
		}

		public enum Resolution
		{
			Year = 1, Month = 2, Day = 3, Hour = 4, Minute = 5, Second = 6, Millisecond = 7
		}

	}
}
