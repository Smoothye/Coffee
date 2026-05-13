using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void AdditionTest()
        {
            int result = 2 + 2;

            Assert.AreEqual(4, result);
        }
    }
}