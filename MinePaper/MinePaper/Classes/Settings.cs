using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinePaper.Classes
{
    public class Settings
    {
        public bool IsLockScreenRotating { get; set; }

        public bool IsDesktopRotating { get; set; }

        public string CurrentDesktopImage { get; set; }

        public string CurrentLockScreenImage { get; set; }

        public List<string> AvailableImages { get; set; }

        public int LockScreenAutoRotateMinutes { get; set; }

        public int DesktopAutoRotateMinutes { get; set; }

        public DateTime LockScreenLastRotatedTime { get; set; }

        public DateTime DesktopLastRotatedTime { get; set; }

        public DateTime LastImageSyncedTime { get; set; }
    }
}
