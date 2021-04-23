using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Text;

namespace mcZen.Data
{
	public delegate bool ReadDelegate(SqlDataReader reader);
	public delegate void OnCompleteEventHandler(object sender, EventArgs e);
}
