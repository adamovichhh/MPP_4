using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestGeneratorLib
{
    public class TestGenerator

    {
        private List<string> classFiles;
        private string destinationFolder;
        private ExecutionDataflowBlockOptions loadFileBlockOption;
        private ExecutionDataflowBlockOptions taskCountBlockOption;
        private ExecutionDataflowBlockOptions  writeFileBlockOption;

        public TestGenerator(List<string> _classFiles, string _destinationFolder, int maxFilesToLoad, int maxExecuteTasks, int maxFilesToWrite)
        {
            classFiles = _classFiles;
            destinationFolder = _destinationFolder;
            loadFileBlockOption = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxFilesToLoad };
            taskCountBlockOption = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxExecuteTasks };
             writeFileBlockOption = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxFilesToWrite };
        }


        public Task Generate()
        {
            var readFiles = new TransformBlock<string, FileInfo>(new Func<string, Task<FileInfo>>(ReadFiles), loadFileBlockOption);
            var generateTestClasses = new TransformBlock<FileInfo, List<FileInfo>>(new Func<FileInfo, Task<List<FileInfo>>>(GenerateTests), taskCountBlockOption);
            var writeToFile = new ActionBlock<List<FileInfo>>(async input => { await WriteToFile(input); },  writeFileBlockOption);

            var linkOptions = new DataflowLinkOptions() { PropagateCompletion = true };
            readFiles.LinkTo(generateTestClasses, linkOptions);
            generateTestClasses.LinkTo(writeToFile, linkOptions);

            foreach (var sourceFile in classFiles)
            {
                readFiles.Post(sourceFile);
            }
            readFiles.Complete();

            return writeToFile.Completion;
        }


        private async Task<FileInfo> ReadFiles(string sourceFile)
        {
            Console.WriteLine("Reading.....");
            string code;
            using (var reader = new StreamReader(new FileStream(sourceFile, FileMode.Open)))
            {
                code = await reader.ReadToEndAsync();
            }
            return new FileInfo(sourceFile, code);
        }


        private async Task WriteToFile(List<FileInfo> fileInfo)
        {
            Console.WriteLine("Writing.....");
            foreach (var fi in fileInfo)
            {
                using var writer = new StreamWriter(
                        new FileStream(Path.Combine(destinationFolder, fi.Name), FileMode.Create));
                await writer.WriteAsync(fi.Code);
            }

        }


        private async Task<List<FileInfo>> GenerateTests(FileInfo fi)
        {
            Console.WriteLine("Generating.....");
            return await GenerateCode(fi);
        }

        private async Task<List<FileInfo>> GenerateCode(FileInfo fi)
        {
            var root = await CSharpSyntaxTree.ParseText(fi.Code).GetRootAsync();
            return GenerateCodeFromTree(root);
        }


        private List<FileInfo> GenerateCodeFromTree(SyntaxNode root)
        {
            var usingDirectives = new List<UsingDirectiveSyntax>(root
                .DescendantNodes()
                .OfType<UsingDirectiveSyntax>());
            var namespaces = new List<NamespaceDeclarationSyntax>(root
                .DescendantNodes()
                .OfType<NamespaceDeclarationSyntax>());

            var nsInfo = new List<NamespaceData>();
            foreach (var ns in namespaces)
            {
                var innerClasses = ns.DescendantNodes().OfType<ClassDeclarationSyntax>();
                var innerNsClasses = new List<ClassData>();
                foreach (var innerNsClass in innerClasses)
                {
                    innerNsClasses.Add(new ClassData(innerNsClass.Identifier.ToString(),
                        GetMethods(innerNsClass)));
                }
                nsInfo.Add(new NamespaceData(ns.Name.ToString(), innerNsClasses));
            }
            return CodeGenerator.Generate(nsInfo, usingDirectives);
        }

        private List<MethodData> GetMethods(ClassDeclarationSyntax innerNsClass)
        {
            var methods = innerNsClass
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>();
            var result = new List<MethodData>();
            foreach (var method in methods)
            {
                result.Add(new MethodData(method.Identifier.ToString(),
                    method.ReturnType, method.ParameterList.Parameters.Select(param => new ParametrsData(param.Identifier.Value.ToString(), param.Type))
                .ToList()));
            }
            return result;
        }


    }
}
