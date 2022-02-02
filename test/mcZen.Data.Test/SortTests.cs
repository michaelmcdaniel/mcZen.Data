using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using mcZen.Data;
namespace mcZen.Data.Test
{
	[TestClass]
	public class SortTests
	{

		[TestMethod]
		public void TestParsing()
		{
			var s1 = OrderBy.Sort.Split("Column DESC");
			Assert.AreEqual(1, s1.Count);
			Assert.AreEqual("Column DESC", s1[0].ToString());
			s1 = OrderBy.Sort.Split("Column1 DESC, Column2 ASC");
			Assert.AreEqual(2, s1.Count);
			Assert.AreEqual("Column1 DESC", s1[0].ToString());
			Assert.AreEqual("Column2 ASC", s1[1].ToString());
			s1 = OrderBy.Sort.Split("ISNULL(Column1,Column2) DESC, Column2 ASC");
			Assert.AreEqual(2, s1.Count);
			Assert.AreEqual("ISNULL(Column1,Column2) DESC", s1[0].ToString());
			Assert.AreEqual("Column2 ASC", s1[1].ToString());

		}
	}
}
