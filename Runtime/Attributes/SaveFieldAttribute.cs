using System;

namespace SaveFramework
{
    /// <summary>
    /// Alias for SaveAttribute to provide alternative naming
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class SaveFieldAttribute : SaveAttribute
    {
        public SaveFieldAttribute() : base()
        {
        }

        public SaveFieldAttribute(string key, params string[] aliases) : base(key, aliases)
        {
        }
    }
}