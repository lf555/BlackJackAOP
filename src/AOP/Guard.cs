using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BlackJackAOP
{
    internal static class Guard
    {
        public static T ArgumentNotNull<T>(T value, [CallerArgumentExpression("value")] string? paramName = null) where T : class
        {
            if (value is null) throw new ArgumentNullException(paramName);
            return value;
        }

        public static string ArgumentNotNullOrWhitespace(string value, [CallerArgumentExpression("value")] string? paramName = null)
        {
            if (value is null) throw new ArgumentNullException(paramName);
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("指定的参数不能为空。", paramName);
            return value;
        }

        public static string ArgumentNotNullOrEmpty(string value, [CallerArgumentExpression("value")] string? paramName = null)
        {
            if (value is null) throw new ArgumentNullException(paramName);
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("指定的参数不能为空。", paramName);
            return value;
        }
    }
}
