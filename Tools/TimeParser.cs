using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace App1.Tools
{
    public static class TimeParser
    {
        public static string ReduceTime(string time)
        {
            if (time.Contains(':'))
            {
                var parts = time.Split(':');
                int mins = int.Parse(parts[0]);
                if (mins == 0)
                {
                    int secs = int.Parse(parts[1]) - 1;
                    if (secs == -1)
                        return "00:00";
                    if (secs < 10)
                        return "00:0" + secs.ToString();
                    return "00:" + secs.ToString();
                }
                else
                {
                    int secs = int.Parse(parts[1]) - 1;
                    if (secs == -1)
                    {
                        mins--;
                        return "0" + mins.ToString() + ":59";
                    }
                    else
                    {
                        if (secs < 10)
                            return parts[0] + ":0" + (int.Parse(parts[1]) - 1).ToString();
                        return parts[0] + ':' + (int.Parse(parts[1]) - 1).ToString();
                    }
                }
            }
            return (int.Parse(time) - 1).ToString();
        }

        public static string ParseTime(int time)
        {
            int seconds = time % 60;
            string strSeconds = seconds >= 10 ? seconds.ToString() : "0" + seconds.ToString();
            int minutes = time / 60;
            string strMinutes = minutes >= 10 ? minutes.ToString() : "0" + minutes.ToString();
            return string.Concat(strMinutes, ':', strSeconds);
        }
    }
}
