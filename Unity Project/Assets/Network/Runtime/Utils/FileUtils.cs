using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Nave.Network
{
    public class FileUtils
    {
        private const string LOG_TAG = "FileUtils";

        public static byte[] ReadFile(string fullpath)
        {
            byte[] buffer = null;
            if (File.Exists(fullpath))
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(fullpath, FileMode.Open, FileAccess.Read);
                    buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                }
                catch (Exception e)
                {
                    Debuger.LogError("ReadFile() Path:{0}, Error:{1}", fullpath, e.Message);
                }
                finally
                {
                    fs?.Close();
                }
            }
            else
            {
                Debuger.LogError(LOG_TAG, "ReadFile() File is Not Exist: {0}", fullpath);
            }
            return buffer;
        }

        public static string[] ReadFileLines(string fullpath)
        {
            List<string> listLines = new List<string>();
            if (File.Exists(fullpath))
            {
                StreamReader fs = null;
                
                try
                {
                    fs = new StreamReader(fullpath);
                    while (fs.Peek() > 0) {
                        listLines.Add(fs.ReadLine());
                    }
                }
                catch (Exception e)
                {
                    Debuger.LogError(LOG_TAG, "ReadFileLines() Path:{0}, Error:{1}", fullpath, e.Message);
                }
                finally
                {
                    fs?.Close();
                }
            }
            else
            {
                Debuger.LogError(LOG_TAG, "ReadFileLines() File is Not Exist: {0}", fullpath);
            }
            return listLines.ToArray();
        }

        public static string ReadString(string fullpath)
        {
            byte[] buffer = ReadFile(fullpath);
            if (buffer != null)
            {
                return Encoding.UTF8.GetString(buffer);
            }
            return "";
        }

        public static string ReadStringASCII(string fullpath)
        {
            byte[] buffer = ReadFile(fullpath);
            if (buffer != null)
            {
                return Encoding.ASCII.GetString(buffer);
            }
            return "";
        }

        public static int SaveFile(string fullpath, byte[] content)
        {
            if (content == null)
            {
                content = new byte[0];
            }

            string dir = PathUtils.GetParentDir(fullpath);

            if (!Directory.Exists(dir))
            {
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch (Exception e)
                {
                    Debuger.LogError(LOG_TAG, "SaveFile() CreateDirectory Error! Dir:{0}, Error:{1}", dir, e.Message);
                    return -1;
                }

            }

            FileStream fs = null;
            try
            {
                fs = new FileStream(fullpath, FileMode.Create, FileAccess.Write);
                fs.Write(content, 0, content.Length);
            }
            catch (Exception e)
            {
                Debuger.LogError(LOG_TAG, "SaveFile() Path:{0}, Error:{1}", fullpath, e.Message);
                fs.Close();
                return -1;
            }
            finally
            {
                fs?.Close();
            }

            return content.Length;
        }

        public static int SaveFile(string fullpath, string content)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            return SaveFile(fullpath, buffer);
        }

        public static string[] GetAllFilesAtDirectory(string dirRoot, string[] ingoreExts = null)
        {
            List<string> arrays = new List<string>();
            string[] files = Directory.GetFiles(dirRoot);
            string[] dirs = Directory.GetDirectories(dirRoot);

            for (int i = 0; i < files.Length; i++)
            {
                string ext = Path.GetExtension(files[i]);
                if (ingoreExts != null && ingoreExts.Contains(ext)) continue;
                arrays.Add(files[i].Replace('\\', '/'));
            }

            for (int i = 0; i < dirs.Length; i++)
            {
                arrays.AddRange(GetAllFilesAtDirectory(dirs[i], ingoreExts));
            }
            return arrays.ToArray();
        }
    }
}

