using System.Runtime.CompilerServices;
using System.Threading;

namespace Enderlook.Unity.Toolset.Utils
{
    internal struct ReadWriteLock
    {
        private int readers;
        private int @lock;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadBegin()
        {
            while (Interlocked.Exchange(ref @lock, 1) != 0) ;
            readers++;
            @lock = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadEnd()
        {
            while (Interlocked.Exchange(ref @lock, 1) != 0) ;
            readers--;
            @lock = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBegin()
        {
            while (true)
            {
                while (Interlocked.Exchange(ref @lock, 1) != 0) ;
                if (readers > 0)
                    @lock = 0;
                else
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEnd() => @lock = 0;
    }
}