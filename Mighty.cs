using System;

using NUnit.Framework;

using Mighty;

namespace OrmSpTesting
{
    [TestFixture]
    public class Mighty
    {
        [Test]
        public void ProcedureWithNameDirectionArgs_Runs()
        {
            var db = new MightyOrm(ConnectionStrings.AdventureWorksFull);
            var result = db.ExecuteProcedure("my_add_proc",
                inParams: new { a = 1, b = 2 },
                returnParams: new { c = (int?)null });
            Assert.AreEqual(3, result.c);
        }

        [Test]
        public void OutputParamFromRawSql_IsBetter()
        {
            var db = new MightyOrm(ConnectionStrings.AdventureWorksFull);
            var result = db.ExecuteWithParams("SELECT @sum = @0 + @0 + @1",
                outParams: new { sum = (int?)null },
                args: new object[] { 1, 3 });
            Assert.AreEqual(5, result.sum);
        }

        [Test]
        public void OutputParamFromRawSql_IsBetter2()
        {
            var db = new MightyOrm(ConnectionStrings.AdventureWorksFull);
            var result = db.ExecuteWithParams("SELECT @sum = @a + @a + @b",
                outParams: new { sum = (int?)null },
                inParams: new { a = 1, b = 3 });
            Assert.AreEqual(5, result.sum);
        }

        [Test]
        public void OutputParamFromRawSql_IsMoreBetter()
        {
            var db = new MightyOrm(ConnectionStrings.AdventureWorksFull);
            var result = db.ExecuteWithParams("SELECT @MyOutParam = @0 + 13 WHERE 42 = @0",
                outParams: new { MyOutParam = (int?)null }, args: 44);
            Assert.IsNull(result.MyOutParam);
        }
    }
}
