using System.Collections.Generic;
using UnityEngine;

namespace UniverseEngine
{
    public delegate bool FunctionToInvoke(Args args);
    public delegate bool OnMatchFunction(Args args);

    public enum ELinkFuncType
    {
        LinkUrl,
        LinkFunction,
    }

    internal class FunctionContext
    {
        public string ID;
        public FunctionToInvoke Function;

        public void Invoke(Args args)
        {
            Function?.Invoke(args);
        }
    }

    internal class FunctionSystem : EngineSystem
    {
        readonly Dictionary<string, FunctionContext> m_Functions = new();

        public void RegisterFunction(string functionID, FunctionToInvoke function)
        {
            if (string.IsNullOrEmpty(functionID))
            {
                Log.Error("Function id is null or empty");
                return;
            }

            if (function == null)
            {
                Log.Error($"Function : {functionID}, function to invoke is null");
                return;
            }

            if (m_Functions.TryGetValue(functionID, out FunctionContext functionContext))
            {
                Log.Error($"Already registered function {functionContext.ID}-{ELinkFuncType.LinkFunction.ToString()}");
                return;
            }

            functionContext = new()
            {
                ID = functionID,
                Function = function,
            };

            m_Functions[functionID] = functionContext;
        }

        public void Invoke(ELinkFuncType funcType, Args args)
        {
            if (!args.IsValid)
            {
                Log.Error("Can not invoke function with null or empty functionID");
                return;
            }

            if (funcType == ELinkFuncType.LinkUrl)
            {
                string url = args.GetString(1);
                Application.OpenURL(url);
                return;
            }

            string functionID = args.GetString(1);
            if (m_Functions.TryGetValue(functionID, out FunctionContext functionContext))
            {
                functionContext.Invoke(args);
            }
        }
    }
}
