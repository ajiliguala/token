using System;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        // 定义源文件夹路径  
        string[] srcDirs = { "D:\\cloud二次开发视频讲座" };
        // 定义目标文件夹路径  
        string dstDir = "D:\\cloud二次开发视频讲座\\文档";

        // 询问用户要操作的文件类型  
        Console.Write("请输入要操作的文件扩展名（如：.docx, .doc, .pdf，输入多个扩展名时用逗号分隔）：");
        string input = Console.ReadLine();

        // 分割用户输入，获取多个扩展名  
        string[] extensions = input.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(x => x.TrimStart('.')) // 移除点号  
                                  .ToArray();

        // 遍历源文件夹，查找用户指定类型的文档      
        foreach (string srcDir in srcDirs)
        {
            var files = Directory.GetFiles(srcDir, "*.*", SearchOption.AllDirectories)
                                 .Where(f => extensions.Contains(Path.GetExtension(f)));

            foreach (string file in files)
            {
                // 构建目标文件路径      
                string fileName = Path.GetFileName(file);
                string dstFilePath = Path.Combine(dstDir, fileName);

                // 如果目标文件已存在，则替换它    
                if (File.Exists(dstFilePath))
                {
                    File.Replace(file, dstFilePath, null);
                }
                else
                {
                    // 移动文件到目标文件夹      
                    File.Copy(file, dstFilePath);
                }

                Console.WriteLine($"Moved '{file}' to '{dstFilePath}'");
            }
        }
    }
}