using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace mcZen.Data
{
	/// <summary>
	/// This Class Manages an Order By Clause
	/// </summary>
	public class OrderBy
	{
		private static Regex s_IsFunction = new Regex("^\\s*(?'function'\\w+)\\((?'args'.*)\\)\\s*$", RegexOptions.Singleline);
		private static Regex s_SortMatch = new Regex(@"(?:\s*(?'column'(?:.+?))(?:\s+(?'dir'DESC|ASC))?\s*)$", RegexOptions.Singleline);
		private static Regex s_SQLParser = new Regex("\\s+ORDER\\s+BY\\s+(?'sort'[^;]*)$", RegexOptions.Singleline | RegexOptions.IgnoreCase);
		public class Sort
		{
			private string _Column;
			private string _Direction;
			public Sort(Match m)
			{
				if (m.Groups["dir"].Value.Length > 0)
					_Direction = m.Groups["dir"].Value.ToUpper();
				else
					_Direction = "ASC";
				_Column = m.Groups["column"].Value;
			}

			public Sort(Sort copy)
			{
				_Column = copy._Column;
				_Direction = copy._Direction;
			}

			/// <summary>
			/// Create Ascending sort on given column.
			/// </summary>
			/// <param name="column">the column name</param>
			public Sort(string column)
			{
				_Column = column;
				_Direction = "ASC";
			}

			public string Column
			{
				get { return _Column; }
				set { _Column = value; }
			}

			/// <summary>
			/// The direction of the sort.  Should be DESC or ASC.
			/// </summary>
			public string Direction
			{
				get { return _Direction; }
				set { _Direction = value.ToUpper(); }
			}

			public bool Descending
			{
				get { return _Direction == "DESC"; }
				set { _Direction = (value) ? "DESC" : "ASC"; }
			}

			/// <summary>
			/// overridden.  returns "column direction" 
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				return _Column + " " + _Direction;
			}

			/// <summary>
			/// returns "prefix.column direction"
			/// </summary>
			/// <param name="prefix">given prefix to prepend to column name</param>
			/// <returns></returns>
			public string ToString(string prefix)
			{
				if (string.IsNullOrEmpty(prefix))
					return ToString();
				// strip existing prefix
				Match m = s_IsFunction.Match(_Column);
				if (m.Success)
				{
					int dotIndex = m.Groups["args"].Value.LastIndexOf('.');
					string column = (dotIndex >= 0) ? m.Groups["args"].Value.Substring(dotIndex + 1) : m.Groups["args"].Value;
					return m.Groups["function"].Value + "(" + (prefix.EndsWith(".") ? "" : ".") + column + ") " + _Direction;
				}
				else
				{
					int dotIndex = _Column.LastIndexOf('.');
					string column = (dotIndex >= 0) ? _Column.Substring(dotIndex + 1) : _Column;
					return (prefix.EndsWith(".") ? "" : ".") + column + " " + _Direction;
				}
			}

			public static string Join(List<Sort> sorts)
			{
				return Join(sorts.ToArray());
			}

			/// <summary>
			/// comma separates all given sorts
			/// </summary>
			/// <param name="sorts">list of sorts</param>
			/// <returns>comma separated sorts</returns>
			public static string Join(params Sort[] sorts)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				bool first = true;
				foreach (Sort sort in sorts)
				{
					if (first) first = !first;
					else sb.Append(",");
					sb.Append(sort);
				}
				return sb.ToString();
			}

			public static string Join(string prefix, List<Sort> sorts)
			{
				return Join(prefix, sorts.ToArray());
			}

			public static string Join(string prefix, params Sort[] sorts)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				bool first = true;
				foreach (Sort sort in sorts)
				{
					if (first) first = !first;
					else sb.Append(",");
					sb.Append(sort.ToString(prefix));
				}
				return sb.ToString();
			}

			public static List<Sort> Split(string sort)
			{
				//string[] splits = sort.Split(',');
				List<string> splits = new List<string>();
				int start = 0;
				while (start +1 < sort.Length && sort[start] == ' ') start++;
				int current = start;
				for (; current < sort.Length; current++)
				{
					if (sort[current] == '[') { while (current < sort.Length && sort[current] != ']') { current++; } }
					else if (sort[current] == '(') { while (current < sort.Length && sort[current] != ')') { current++; } }
					else if (sort[current] == ',')
					{
						splits.Add(sort.Substring(start, current - start));
						while (current+2 < sort.Length && sort[current+1] == ' ') current++;
						start = current+1;
					}
				}
				if (current > start) splits.Add(sort.Substring(start, current - start));
				List<Sort> retVal = new List<Sort>();
				foreach (string s in splits)
				{
					Match m = s_SortMatch.Match(s);
					if (m.Success && m.Groups["column"].Value.Length > 0)
					{
						retVal.Add(new Sort(m));
					}
				}
				return retVal;
			}
		}

		private List<Sort> _Sorts;
		public OrderBy(OrderBy copy)
		{
			_Sorts = new List<Sort>();
			foreach (Sort sort in copy._Sorts)
			{
				_Sorts.Add(new Sort(sort));
			}
		}

		public OrderBy()
		{
			_Sorts = new List<Sort>();
		}

		/// <summary>
		/// adds the given sort to the end of sort list.  (will remove it if it already exists)
		/// </summary>
		/// <param name="sort"></param>
		public void Add(Sort sort)
		{
			// remove this sort if it already exists
			_Sorts.RemoveAll(delegate(Sort s) {
				return s.Column == sort.Column;
			});
			_Sorts.Add(sort);
		}

		/// <summary>
		/// adds the given sorts to the end of sort list.  (will remove any if it already exists)
		/// </summary>
		/// <param name="sorts"></param>
		public void Add(List<Sort> sorts)
		{
			foreach (Sort sort in sorts)
			{
				_Sorts.RemoveAll(delegate(Sort s)
				{
					return s.Column == sort.Column;
				});
			}
			_Sorts.AddRange(sorts);
		}

		/// <summary>
		/// Inserts sort at specified index (will remove any if it already exists)
		/// </summary>
		/// <param name="index">zero based index</param>
		/// <param name="sort">sort to add</param>
		public void Insert(int index, Sort sort)
		{
			_Sorts.RemoveAll(delegate(Sort s)
			{
				return s.Column == sort.Column;
			});
			_Sorts.Insert(index, sort);
		}

		/// <summary>
		/// The number of sorts currently housed.
		/// </summary>
		public int Count
		{
			get { return _Sorts.Count; }
		}

		/// <summary>
		/// returns sort at given index.
		/// </summary>
		/// <param name="index">Zero based index</param>
		/// <returns>Sort at specified index</returns>
		public Sort this[int index]
		{
			get { return _Sorts[index]; }
		}

		/// <summary>
		/// returns sort at index.
		/// </summary>
		/// <param name="index"></param>
		public void RemoveAt(int index)
		{
			_Sorts.RemoveAt(index);
		}

		/// <summary>
		/// Creates sort based on comma separated list of sorts
		/// </summary>
		/// <param name="existingSort">existing sort</param>
		public OrderBy(string existingSort)
		{
			_Sorts = Sort.Split(existingSort);
		}

		/// <summary>
		/// Creates sort based on comma separated list of sorts, adding to sort as primary column sort
		/// </summary>
		/// <param name="existingSort">existing sort</param>
		/// <param name="newSortColumn">new column to add as primary sort</param>
		public OrderBy(string existingSort, string newSortColumn)
		{
			_Sorts = Sort.Split(existingSort);
			Sort newSort = new Sort(newSortColumn);
			if (!string.IsNullOrEmpty(newSortColumn))
			{
				for (int i = 1; i < _Sorts.Count; i++)
				{
					if (_Sorts[i].Column == newSort.Column)
					{
						_Sorts.RemoveAt(i);
						break;
					}
				}
				if (_Sorts.Count > 0 && _Sorts[0].Column == newSort.Column)
				{
					_Sorts[0].Descending = !_Sorts[0].Descending;
				}
				else if (_Sorts.Count == 0)
				{
					_Sorts.Add(newSort);
				}
				else
				{
					_Sorts.Insert(0, newSort);
				}
			}
		}

		/// <summary>
		/// replace sort at given index
		/// </summary>
		/// <param name="index">Zero based index</param>
		/// <param name="newColumn">new column</param>
		/// <returns></returns>
		public int Replace(int index, string newColumn)
		{
			int columnsAdded = 0;
			List<Sort> sorts = Sort.Split(newColumn);
			if (sorts.Count > 0)
			{
				_Sorts[index].Column = sorts[0].Column;
				for(int i = 1; i < sorts.Count; i++)
				{
					sorts[i].Descending = _Sorts[index].Descending;
					_Sorts.Insert(index + i, sorts[i]);
					columnsAdded++;
				}
			}
			return columnsAdded;
		}

		/// <summary>
		/// reverses the order of the current sort
		/// </summary>
		/// <returns>new Sort</returns>
		public OrderBy Reverse()
		{
			OrderBy copy = new OrderBy(this);
			Action<Sort> reverse = delegate(Sort sort)
			{
				sort.Descending = !sort.Descending;
			};
			copy._Sorts.ForEach(reverse);
			return copy;
		}

		public override string ToString()
		{
			return Sort.Join(_Sorts);
		}

		public string ToString(string prefix)
		{
			return Sort.Join(prefix, _Sorts);
		}

		/// <summary>
		/// Trys to create sort from given sql statement.
		/// </summary>
		/// <param name="query">sql query</param>
		/// <returns>OrderBy object of sorts</returns>
		public static OrderBy FromQuery(string query)
		{
			OrderBy orderby = new OrderBy();
			Match m;
			if ((m=s_SQLParser.Match(query)).Success)
			{
				orderby._Sorts = Sort.Split(m.Groups["sort"].Value);
			}
			return orderby;
		}
	}
}
