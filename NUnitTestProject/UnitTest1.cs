using google_dialog;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NUnitTestProject
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Test1()
        {
            await DBIntegration.LoadDatabase();
            await DBIntegration.PopulateTables();
        }
    }
}