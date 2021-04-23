using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;

namespace mcZen.Data
{
	public class RequestException : Exception
	{
		string _Query = "";
		List<Tuple<string, string>> _Parameters = new List<Tuple<string,string>>();

		public RequestException(SqlCommand cmd, Exception innerException)
			: base(innerException.Message, innerException)
		{
			_Query = cmd.CommandText;
			foreach (SqlParameter parameter in cmd.Parameters)
				_Parameters.Add(new Tuple<string, string>(parameter.ParameterName, GetValue(parameter.Value)));
		}

		private static string GetValue(object obj)
		{
			if (obj == null) return "null";
			if (obj == DBNull.Value) return "DBNull";
			return Tools.GenericConvert<string>(obj, "");
		}

		public override string ToString()
		{
			string str2;
			string className = GetType().FullName;
			if (string.IsNullOrEmpty(Message))
			{
				str2 = className;
			}
			else
			{
				str2 = className + ": " + Message;
			}
			str2 += Environment.NewLine + "Query: " + (_Query??"");
			_Parameters.ForEach((p) => { str2 += Environment.NewLine + "\t" + (p.Item1??"") + "='" + (p.Item2??"") + "'"; });
			str2 += Environment.NewLine;
			if (InnerException != null)
			{
				str2 = str2 + " ---> " + this.InnerException.ToString() + Environment.NewLine;
			}
			if (this.StackTrace != null)
			{
				str2 = str2 + Environment.NewLine + this.StackTrace;
			}
			return str2;
		}
	}
}
