using System.Diagnostics;

namespace MuteLight
{
    public static class Utils
    {
        public static string GetProcessName(int processId)
        {
            var processName = processId.ToString();
            try
            {
                processName = Process.GetProcessById(processId).ProcessName;
            }
            catch
            {
                // ignored
            }

            return processName;
        }
    }
}
