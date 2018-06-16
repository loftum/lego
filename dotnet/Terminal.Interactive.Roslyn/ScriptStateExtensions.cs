using Microsoft.CodeAnalysis.Scripting;

namespace Terminal.Interactive.Roslyn
{
    public static class ScriptStateExtensions
    {
        public static object GetResult(this ScriptState state)
        {
            return state.ReturnValue ?? state.Exception;
        }
    }
}