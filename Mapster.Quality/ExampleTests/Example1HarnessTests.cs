using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mapster.Example.DomainModels;

namespace Mapster.Quality.ExampleTests
{
    [TestClass]
    public class Example1HarnessTests
    {
        [TestMethod]
        public void TestExample1_Get_ReturnsASingleOrderWithoutChildDetails()
        {
            // setup
            int id = 1;
            var logic = new Example.Example1();

            // call
            var actual = logic.Get(id);

            // assert
            Assert.AreEqual(id, actual.Id);
        }

        [TestMethod]
        public void TestExample1_GetEverything_ReturnsASingleOrderWithChildDetails()
        {
            // setup
            int id = 1;
            var logic = new Example.Example1();

            // call
            var actual = logic.GetEverything(id);

            // assert
            Assert.IsTrue(actual.LineItems.Count > 0);
        }

        [TestMethod]
        public void TestExample1_Search_ReturnsACollectionOfOrdersWithoutChildDetails()
        {
            // setup
            string searchTerms = "100";
            var logic = new Example.Example1();

            // call
            var actual = logic.Search(searchTerms);

            // assert
            Assert.IsTrue(actual.Count > 0);
        }
    }
}
