using System.IO;
namespace AlpacaSpy
{
    public class FileBrowserService
    {
        public IEnumerable<string> GetDirectories(string path)
        {
            try
            {
                return Directory.GetDirectories(path);
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        public IEnumerable<string> GetFiles(string path)
        {
            try
            {
                return Directory.GetFiles(path);
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }
    }

}
