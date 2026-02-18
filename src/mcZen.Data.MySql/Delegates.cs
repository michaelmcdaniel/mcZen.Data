using System;
using System.Collections.Generic;
using MySqlConnector;
using System.Text;

namespace mcZen.Data.MySql
{
	public delegate bool ReadDelegate(MySqlDataReader reader);
	public delegate void OnCompleteEventHandler(object sender, EventArgs e);
}
