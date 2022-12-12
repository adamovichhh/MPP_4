

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using TestGeneratorLib;

namespace TestProject
{
    public class Tests
    {
        private string classFolder;
        private string generatedClassFolder;
        string testClassesFolder;
        [SetUp]
        public async Task SetupAsync()
        {
            generatedClassFolder  = @"C:\\Users\\User\\Desktop\\Ycheba_5sem\\SPP\MPP4\\TestProject\\ResultTests";
            testClassesFolder =  @"C:\\Users\\User\\Desktop\\Ycheba_5sem\\SPP\\MPP4\\MPP4\\Files";
            var allTestClasses = new List<string>();
            foreach (string path in Directory.GetFiles(testClassesFolder, "*.cs"))
            {
                allTestClasses.Add(path);
            }
    
            TestGenerator generator = new(allTestClasses, generatedClassFolder, 3, 3, 3);
            await generator.Generate();
        }

        [Test]
        public void Dividing_To_OneClassFiles()
        {
            var generatedFiles = new List<string>();
            foreach (string Onefile in Directory.GetFiles(generatedClassFolder, "*.cs"))
            {
                generatedFiles.Add(Onefile);
            }
            var classFiles = new List<string>();
            foreach (string Onefile in Directory.GetFiles(testClassesFolder, "*.cs"))
            {
                classFiles.Add(Onefile);
            }
            Assert.AreEqual(generatedFiles.Count(), 6);
            Assert.AreEqual(classFiles.Count(), 4);
        }

        [Test]
        public void One_class_one_namespace()
        {
          
            var classCount = Directory.GetFiles(generatedClassFolder)
                .Count(file => file.Contains("Class1_"));
            Assert.AreEqual(classCount, 1);
        }

        [Test]
        public void two_class_two_namespace()
        {

            var class4Count = Directory.GetFiles(generatedClassFolder)
                 .Count(file => file.Contains("Class4_"));
            var class5Count = Directory.GetFiles(generatedClassFolder)
                .Count(file => file.Contains("Class5_"));

            Assert.AreEqual(class4Count, 1);
            Assert.AreEqual(class5Count, 1);
        }
    }
}