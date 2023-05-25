using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinePaper.Classes
{
    public class Settings
    {
        public bool IsAutoRotating { get; set; }

        public string CurrentImage { get; set; }

        public List<string> AvailableImages { get; set; }

        public int AutoRotateMinutes { get; set; }

        public DateTime LastRotatedTime { get; set; }

        public DateTime LastImageSyncedTime { get; set; }
    }
}
