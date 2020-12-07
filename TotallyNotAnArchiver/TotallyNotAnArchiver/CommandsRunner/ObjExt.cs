using System;

namespace TotallyNotAnArchiver.CommandsRunner
{
    public static class ObjExt
    {
        public static V PipeTo<T,V>(this T obj, Func<T,V> Pipe)
        {
            return Pipe(obj);
        }
        public static void FinalPipeTo<T>(this T obj, Action<T> Pipe)
        {
            Pipe(obj);
        }
    }
}