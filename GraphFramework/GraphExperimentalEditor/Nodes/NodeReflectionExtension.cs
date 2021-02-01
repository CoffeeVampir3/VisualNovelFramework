namespace VisualNovelFramework.GraphFramework.Editor.Nodes
{
    public static class NodeReflectionExtension
    {
        /// <summary>
        /// Thanks to OdinInspector for the idea, this is a direct adaptation of their
        /// work thanks Tor @OdinTeam for the help understanding and implementing it.
        /// </summary>
        public static System.Type[] GetGenericClassConstructorArguments(
            this System.Type candidateType,
            System.Type openGenericType)
        {
            //We've found our target class.
            if (candidateType.IsGenericType && 
                candidateType.GetGenericTypeDefinition() == openGenericType)
                return candidateType.GetGenericArguments();
            
            //Keep looking
            System.Type baseType = candidateType.BaseType;
            return baseType != null 
                ? baseType.GetGenericClassConstructorArguments(openGenericType) 
                : null;
        }
    }
}