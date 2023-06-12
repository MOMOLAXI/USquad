namespace UniverseEngine
{
    public static partial class UniverseEngine
    {
        public static void RegisterFunction(string functionID, FunctionToInvoke function)
        {
            EngineSystem<FunctionSystem>.System.RegisterFunction(functionID, function);
        }

        public static void InvokeFunction(ELinkFuncType funcType, Args args)
        {
            EngineSystem<FunctionSystem>.System.Invoke(funcType, args);
        }
    }
}
