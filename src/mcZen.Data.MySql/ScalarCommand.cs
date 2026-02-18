using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using MySqlConnector;

namespace mcZen.Data.MySql
{
	/// <summary>
	/// A sql statement that returns a single value
	/// </summary>
	/// <example>
	/// ConnectionFactory factory = new ConnectionFactory(connectionString);
	/// var scalar = new ScalarCommand&lt;int&gt;("SELECT COUNT(*) FROM [TABLE] WHERE timestamp>@ts", new MySqlParameter("@ts", DateTime.UtcNow)); 
	/// DateTime actioned = DateTime.MinValue;
	/// var scalar2 = new ScalarCommand&lt;DateTime&gt;(v=>actionTotal=v, DateTime.MaxValue, "SELECT MAX(timestamp) FROM [TABLE] WHERE timestamp>@ts", new MySqlParameter("@ts", DateTime.UtcNow)); 
	///	factory.Register(scalar);
	///	factory.Register(scalar2);
	///	factory.Execute();
	///	int total = scalar.Value
	/// </example>
	/// <typeparam name="T">The value of the expected return type</typeparam>
	public class ScalarCommand<T> : CommandReader
	{
		T _Obj;
		public event Action<T> OnValue;

		public ScalarCommand(Action<T> action, T defaultValue, string query, params MySqlParameter[] parameters)
			: this(defaultValue, query, parameters)
		{
			if (action != null) OnValue += action;
		}

		public ScalarCommand(Action<T> action, T defaultValue, string query, System.Data.CommandType commandType, params MySqlParameter[] parameters)
			: this(defaultValue, query, commandType, parameters)
		{
			if (action != null) OnValue += action;
		}

		public ScalarCommand(Action<T> action, string query, params MySqlParameter[] parameters)
			: this(query, parameters)
		{
			if (action != null) OnValue += action;
		}

		public ScalarCommand(Action<T> action, string query, System.Data.CommandType commandType, params MySqlParameter[] parameters)
			: this(query, commandType, parameters)
		{
			if (action != null) OnValue += action;
		}

		public ScalarCommand(string query, params MySqlParameter[] parameters)
			: base(query, parameters)
		{
			_Obj = default(T);
		}

		public ScalarCommand(string query, System.Data.CommandType commandType, params MySqlParameter[] parameters)
			: base(query, commandType, parameters)
		{
			_Obj = default(T);
		}

		public ScalarCommand(T defaultValue, string query, params MySqlParameter[] parameters)
			: base(query, parameters)
		{
			_Obj = defaultValue;
		}

		public ScalarCommand(T defaultValue, string query, System.Data.CommandType commandType, params MySqlParameter[] parameters)
			: base(query, commandType, parameters)
		{
			_Obj = defaultValue;
		}

		public ScalarCommand(T defaultValue, string query, System.Data.CommandType commandType, TimeSpan timeout, params MySqlParameter[] parameters)
			: base(query, commandType, timeout, parameters)
		{
			_Obj = defaultValue;
		}

		protected override System.Threading.Tasks.Task<bool> Read(MySqlConnector.MySqlDataReader reader)
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
