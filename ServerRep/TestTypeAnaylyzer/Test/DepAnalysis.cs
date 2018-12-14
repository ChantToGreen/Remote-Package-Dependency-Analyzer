using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeFile;
using TypeAnalysis;
using CodeAnalysis;
using Lexer;

namespace DependencyAnalysis
{
    class DepAnalysis
    {
        private List<string> files_ = null;
        public DepAnalysis()
        {
            files_ = new List<string>();
        }
        public void AddFile(string path)
        {
            string[] files = System.IO.Directory.GetFiles(path, "*.cs", System.IO.SearchOption.AllDirectories);
            foreach(string file in files)
                files_.Add(file);
        }
        public void collectTypeInfo(string path)
        {
            TypeAnalyzer analyzer = new TypeAnalyzer();
            analyzer.AddFileColeection(path);
            analyzer.AnalyzeFiles();
        }
        public void ConstructDependency()
        {
            foreach(string filepath in files_)
            {
                Semi semi = new Semi();
                if (!semi.open(filepath))
                {
                    Console.WriteLine("Cannot open file {0}", filepath);
                    continue;
                }
                string filename = TypeAnalyzer.Filename(filepath);
                //Console.WriteLine(" - Processing file {0}", filename);
                BuildDependencyAnalyzer depAnalyzer = new BuildDependencyAnalyzer(semi,filename);
                Parser depParser = depAnalyzer.build();
                //Console.WriteLine("Size of graph: {0}", graph_.Count);
                try
                {
                    while (semi.get().Count > 0)
                        depParser.parse(semi);
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
            }

        }
        static void Main()
        {
            DepAnalysis analyzer = new DepAnalysis();
            analyzer.AddFile("../../../TestTypeAnaylyzer");
            analyzer.collectTypeInfo("../../../TestTypeAnaylyzer");
            analyzer.ConstructDependency();
            Repository repo = Repository.getInstance();
            repo.WalkTheGraph();
            Console.ReadKey();
        }
    }
}
