using System.Threading.Tasks;

namespace VoiceVoxPlugin.Core
{
    public static class TaskExtension
    {
        public static void Synchronous(this Task t)
        {
            t.Wait();
        }

        public static T Synchronous<T>(this Task<T> t)
        {
            t.Wait();
            return t.Result;
        }
    }
}