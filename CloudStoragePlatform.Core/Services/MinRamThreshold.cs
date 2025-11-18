using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Services
{
    public static class MinRamThreshold
    {
        private const long MinRamBytes = 45L * 1024 * 1024;         // 45 MB
        private const int MaxRetries = 7;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(200);

        public static async Task<bool> WaitForRamOrTimeoutAsync(Func<Task> action)
        {
            int retries = 0;

            while (retries < MaxRetries)
            {
                long available = GetAvailableRamBytes();

                if (available >= MinRamBytes)
                {
                    // RAM sufficient — run immediately
                    await action();
                    return true;
                }

                await Task.Delay(RetryDelay);
                retries++;
            }

            // After 7 failures → force run the action
            await action();
            return false; // false means "threshold was ignored"
        }

        private static long GetAvailableRamBytes()
        {
            var info = GC.GetGCMemoryInfo();
            return info.TotalAvailableMemoryBytes;
        }
    }
}
