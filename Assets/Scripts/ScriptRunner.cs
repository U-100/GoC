using System.Collections.Generic;
using System.Threading.Tasks;
using Jint;

public class ScriptRunner
{
	public async Task Execute(Dictionary<string, object> api, string script) => await Task.Run(() => ExecuteImpl(api, script));

	private void ExecuteImpl(Dictionary<string, object> api, string script)
	{
		Engine engine = new Engine();
		
		foreach (KeyValuePair<string, object> kvp in api)
			engine.SetValue(kvp.Key, kvp.Value);

		engine.Execute(script);
	}
}
