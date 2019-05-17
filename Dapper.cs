using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

using NUnit.Framework;

using Dapper;
using System.Data;

namespace OrmSpTesting
{
    [TestFixture]
    public class Dapper
    {
        [Test]
        public void ProcedureWithNameDirectionArgs_Runs()
        {
            using (var cnn = new SqlConnection(ConnectionStrings.AdventureWorks))
            {
                var p = new DynamicParameters();
                p.Add("@a", 1);
                p.Add("@b", 2);
                // param type is ignored for return params
                p.Add("@c", direction: ParameterDirection.ReturnValue);
                cnn.Query("my_add_proc", p, commandType: CommandType.StoredProcedure);
                int c = p.Get<int>("@c");
                Assert.AreEqual(3, c);
            }
        }

        [Test]
        public void OutputParamFromRawSql_IsBetter()
        {
            using (var cnn = new SqlConnection(ConnectionStrings.AdventureWorks))
            {
                var p = new DynamicParameters();
                p.Add("@0", 1);
                p.Add("@1", 3, null, ParameterDirection.Input); // just to check
                // aaah! it's not getting the type from (int?)null - Mighty can, and does! :)
                // (because the anonymous object really has a property of that type)
                p.Add("@sum", (int?)null, DbType.Int32, ParameterDirection.Output);
                cnn.Query("SELECT @sum = @0 + @0 + @1", p);
                Assert.AreEqual(5, p.Get<int?>("@sum"));
            }
        }

        [Test]
        public void IntNullOutputParam_CantSetItsOwnType()
        {
            using (var cnn = new SqlConnection(ConnectionStrings.AdventureWorks))
            {
                var p = new DynamicParameters();
                p.Add("@0", 1);
                p.Add("@1", 3);
                // aaah! it's not getting the type from (int?)null - Mighty can, and does! :)
                // (because the anonymous object really has a property of that type)
                p.Add("@sum", (int?)null, null, ParameterDirection.Output);
                var ex = Assert.Throws<InvalidOperationException>(() => {
                    cnn.Query("SELECT @sum = @0 + @0 + @1", p);
                });
                Assert.True(ex.Message.Contains("invalid size of 0"));
            }
        }

        [Test]
        public void OutputParamFromRawSql_IsBetter2()
        {
            using (var cnn = new SqlConnection(ConnectionStrings.AdventureWorks))
            {
                var p = new DynamicParameters();
                p.Add("@a", 1);
                p.Add("@b", 3);
                // as above
                p.Add("@sum", (int?)null, DbType.Int32, ParameterDirection.Output);
                cnn.Query("SELECT @sum = @a + @a + @b", p);
                Assert.AreEqual(5, p.Get<int?>("@sum"));
            }
        }

        [Test]
        public void OutputParamFromRawSql_IsMoreBetter()
        {
            using (var cnn = new SqlConnection(ConnectionStrings.AdventureWorks))
            {
                var p = new DynamicParameters();
                p.Add("@0", 43, DbType.Int32);
                p.Add("@MyOutParam", (int?)null, DbType.Int32, ParameterDirection.Output);
                cnn.Query("SELECT @MyOutParam = @0 + 13 WHERE 42 = @0", p);
                Assert.IsNull(p.Get<int?>("@MyOutParam"));
            }
        }
    }
}
