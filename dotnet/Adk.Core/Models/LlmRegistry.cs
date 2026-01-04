using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Adk.Core.Models
{
    // C# doesn't have first-class types with static property constraints easily.
    // We can use a factory delegate or just Type.
    // I'll use Type and reflection, or a factory function.

    public class LlmRegistry
    {
        // Key is regex pattern string
        private static readonly Dictionary<string, Type> RegistryDict = new Dictionary<string, Type>();
        // Using a simple Dictionary for cache for now, no LRU impl
        private static readonly Dictionary<string, Type> ResolveCache = new Dictionary<string, Type>();

        public static BaseLlm NewLlm(string model)
        {
            var llmType = Resolve(model);
            return (BaseLlm)Activator.CreateInstance(llmType, new object[] { model })!;
        }

        public static void Register(Type llmType, IEnumerable<string> supportedModels)
        {
            foreach (var regex in supportedModels)
            {
                if (RegistryDict.ContainsKey(regex))
                {
                    // Log update
                }
                RegistryDict[regex] = llmType;
            }
        }

        public static Type Resolve(string model)
        {
            if (ResolveCache.TryGetValue(model, out var cachedType))
            {
                return cachedType;
            }

            foreach (var kvp in RegistryDict)
            {
                var pattern = $"^{kvp.Key}$";
                if (Regex.IsMatch(model, pattern))
                {
                    ResolveCache[model] = kvp.Value;
                    return kvp.Value;
                }
            }

            throw new Exception($"Model {model} not found.");
        }
    }
}
