# mcZen.Data

Simple transaction command based sql access.

### Fetch Values

```List<KeyValuePair<Guid,int>> values = new List<KeyValuePair<Guid, int>>();
ConnectionFactory.Execute(
    new CommandReaderRequest(
	r => {
	    values.Add(new KeyValuePair<Guid, int>(r.GetGuid(0), r.GetInt32(1)));
	}, 
	"SELECT [key],[column] FROM [Table] WHERE [column]>@value", 
	System.Data.CommandType.Text,
	_options.Value.Timeout, 
	new SqlParameter("@value", 10)
    ), 
    _options.Value.ConnectionString);
```

### Insert Values

```ConnectionFactory factory = new ConnectionFactory(_options.Value.ConnectionString);
factory.Register(mcZen.Data.Commands.Insert("[Table]", new SqlParameter("@key", Guid.NewGuid()), new SqlParameter("@value", 5)));
factory.Register(mcZen.Data.Commands.Insert("[Table]", new SqlParameter("@key", Guid.NewGuid()), new SqlParameter("@value", 3)));
factory.Execute();'
```
