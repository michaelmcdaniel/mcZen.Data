using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Microsoft.Data.SqlClient;

namespace mcZen.Data
{
	public class ScalarRequest<T> : CommandReaderRequest
	{
		T _Obj;
		public event Action<T> OnValue;

		public ScalarRequest(Action<T> action, T defaultValue, string query, params SqlParameter[] parameters)
			: this(defaultValue, query, parameters)
		{
			if (action != null) OnValue += action;
		}

		public ScalarRequest(Action<T> action, string query, params SqlParameter[] parameters)
			: this(query, parameters)
		{
			if (action != null) OnValue += action;
		}

		public ScalarRequest(string query, params SqlParameter[] parameters)
			: base(query, parameters)
		{
			_Obj = default(T);
		}

		public ScalarRequest(T defaultValue, string query, params SqlParameter[] parameters)
			: base(query, parameters)
		{
			_Obj = defaultValue;
		}

		protected override System.Threading.Tasks.Task<bool> Read(Microsoft.Data.SqlClient.SqlDataReader reader)
		{
			if (!reader.IsDBNull(0))
			{
				_Obj = (T)reader.GetValue(0);
			}
			Action<T> d = OnValue;
			if (d != null) d(_Obj);
				
			return System.Threading.Tasks.Task.FromResult(false);
		}

        public T Value {
            get {
                return _Obj;
            }
        }
	}
}
