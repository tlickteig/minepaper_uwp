using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinePaper.Classes
{
    public class Constants
    {
        public const string REMOTE_IMAGES_FOLDER = "https://minepaper.net/wallpapers";

        public const string REMOTE_IMAGE_LIST_ENDPOINT = "https://minepaper.net/api/allImages.php";

        public const string CONFIG_FILE_NAME = "config.json";

        public const int MAX_IMAGES = 500;

        public const int MAX_TRIES = 5;

        public const int MIN_IMAGES_TO_DOWNLOAD = 50;

        public const int MAX_IMAGES_TO_DOWNLOAD = 100;
    }
}
