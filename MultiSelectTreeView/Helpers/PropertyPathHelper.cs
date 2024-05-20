namespace System.Windows.Helpers
{
    public static class PropertyPathHelper
    {
        public static object GetObjectByPropertyPath(object target, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return target;
            }
            
            var p = target.GetType().GetProperty(path);
            if (p == null)
                throw new ArgumentException($"The property {path} could not be found.");

            var result = p.GetValue(target, null);
            return result;
        }
        
        public static object GetDeepPropertyValue(object obj, string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return obj;
            while (true)
            {
                if (path.Contains("."))
                {
                    string[] split = path.Split('.');
                    string remainingProperty = path.Substring(path.IndexOf('.') + 1);
                    if (obj != null)
                    {
                        obj = obj.GetType().GetProperty(split[0])?.GetValue(obj, null);
                    }
                    else
                    {
                        return null;
                    }
                    path = remainingProperty;
                    continue;
                }

                return obj?.GetType().GetProperty(path)?.GetValue(obj, null);
            }
        }
    }
}