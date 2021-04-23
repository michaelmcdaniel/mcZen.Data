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
		private static Regex s_SortMatch = new Regex(@"(?:\s*(?'column'(?:\[(?>.*?\]))|(?:[^,]*?))(?:\s+(?'dir'DESC|ASC))?\s*)$", RegexOptions.Singleline);
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

			public override string ToString()
			{
				return _Column + " " + _Direction;
			}

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
				string[] splits = sort.Split(',');
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

		public void Add(Sort sort)
		{
			// remove this sort if it already exists
			_Sorts.RemoveAll(delegate(Sort s) {
				return s.Column == sort.Column;
			});
			_Sorts.Add(sort);
		}

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

		public void Insert(int index, Sort sort)
		{
			_Sorts.RemoveAll(delegate(Sort s)
			{
				return s.Column == sort.Column;
			});
			_Sorts.Insert(index, sort);
		}

		public int Count
		{
			get { return _Sorts.Count; }
		}

		public Sort this[int index]
		{
			get { return _Sorts[index]; }
		}

		public void RemoveAt(int index)
		{
			_Sorts.RemoveAt(index);
		}

		public OrderBy(string existingSort)
		{
			_Sorts = Sort.Split(existingSort);
		}

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
