using System.Collections;
using System.Collections.Generic;

namespace CheckerApi.Extensions
{
    public static class QueueExtensions
    {
        public static Queue<string> ConditionEnqueue(this Queue<string> queue, string item)
        {
            queue.Enqueue(item);
            if (queue.Count > 100)
            {
                queue.Dequeue();
            }

            return queue;
        }
    }
}
