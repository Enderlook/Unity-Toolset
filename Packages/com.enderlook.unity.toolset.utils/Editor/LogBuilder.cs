using System.Text;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Utils
{
    internal struct LogBuilder
    {
        private static StringBuilder stringBuilder;

        private StringBuilder builder;

        private LogBuilder(StringBuilder builder) => this.builder = builder;

        public static LogBuilder GetLogger(int initialCapacity = 0)
        {
            StringBuilder builder = Interlocked.Exchange(ref stringBuilder, null);
            if (builder is null)
                builder = new StringBuilder(initialCapacity);
            else
                builder.EnsureCapacity(initialCapacity);
            return new LogBuilder(builder);
        }

        public LogBuilder Append(string value)
        {
            builder.Append(value);
            return this;
        }

        public LogBuilder Append(object value)
        {
            builder.Append(value);
            return this;
        }

        public void LogError()
        {
            string result = builder.ToString();
            builder.Clear();
            stringBuilder = builder;
            builder = null;
            Debug.LogError(result);
        }

        public LogBuilder RemoveLast(int characters)
        {
            builder.Length -= characters;
            return this;
        }
    }
}