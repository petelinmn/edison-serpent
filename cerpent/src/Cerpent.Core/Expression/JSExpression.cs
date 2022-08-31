using Jint;
using Jint.Native;
using Newtonsoft.Json.Linq;

namespace Cerpent.Core.Expression;

public class JSExpression
{
    static JsValue GetJsValue(string command, object argument)
    {
        var objJToken = JToken.FromObject(argument);
        var engine = objJToken.Cast<JProperty?>().Aggregate(new Engine(),
            (current, property) => 
                current.SetValue(property.Name, property.Value));

        return engine.Execute($"result = ({command})")
            .GetValue("result");
    }

    public static bool Condition(string command, object argument) =>
        GetJsValue(command, argument).AsBoolean();
    
    public static double Calculate(string command, object argument) =>
        GetJsValue(command, argument).AsNumber();
}
