using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;

using NUnit.Framework;

using PetaPoco;
using PetaPoco.Core;
using PetaPoco.Providers;
using PetaPoco.Utilities;


namespace OrmSpTesting
{
    public static class ObjectExtensions
    {
        public static DbParameter CreateParameter(
            this Database db,
            string name = null,
            object value = null,
            ParameterDirection direction = ParameterDirection.Input)
        {
            var param = db.Provider.GetFactory().CreateParameter();
            param.Direction = direction;
            if (name != null) param.ParameterName = name;
            if (value != null) param.Value = value;
            return param;
        }
    }

    [TestFixture]
    public class PetaPoco
    {
        [Test]
        public void ProcedureUsingHelperMethod_Runs()
        {
            using (var db = new Database(ConnectionStrings.AdventureWorks, "System.Data.SqlClient"))
            {
                var c = db.CreateParameter("c", 0, ParameterDirection.ReturnValue);
                db.ExecuteNonQueryProc("my_add_proc",
                    db.CreateParameter("a", 1), db.CreateParameter("b", 2), c);
                Assert.AreEqual(3, c.Value);
            }
        }

        [Test]
        public void ProcedureUsingSqlParameter_Runs()
        {
            using (var db = new Database(ConnectionStrings.AdventureWorks, "System.Data.SqlClient"))
            {
                var c = new SqlParameter("c", 0);
                c.Direction = ParameterDirection.ReturnValue;
                db.ExecuteNonQueryProc("my_add_proc",
                    new SqlParameter("a", 1), new SqlParameter("b", 2), c);
                Assert.AreEqual(3, c.Value);
            }
        }

        public class Address
        {
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string PostalCode { get; set; }
            public string Country { get; set; }

            public override string ToString()
            {
                return string.Format("{0},{1} {2}, {3}, {4}, {5}",
                    AddressLine1,
                    AddressLine2 == null ? "" : $" {AddressLine2},",
                    City,
                    State,
                    PostalCode,
                    Country);
            }
        }

        const string sql = @"SELECT a.AddressLine1, a.AddressLine2, a.City, p.Name AS [State], a.PostalCode, c.Name AS [Country]
FROM Person.[Address] a
INNER JOIN Person.StateProvince p
ON a.StateProvinceID = p.StateProvinceID
INNER JOIN Person.CountryRegion c
ON p.CountryRegionCode = c.CountryRegionCode
";

        [Test]
        public void QueryIgnoringParamName_Succeeds()
        {
            // City	StateProvinceID	PostalCode
            // Bothell 79  98011
            using (var db = new Database(ConnectionStrings.AdventureWorks, "System.Data.SqlClient"))
            {
                var c = new SqlParameter("c", 0);
                c.Direction = ParameterDirection.ReturnValue;
                int count = 0;
                foreach (var address in db.Query<Address>(
                    $"{sql}WHERE City = @0",
                    new SqlParameter("@City", "Bothell")))
                {
                    Debug.WriteLine(address);
                    count++;
                }
                Assert.AreEqual(26, count);
            }
        }

        [Test]
        public void QueryNullParamName_Succeeds()
        {
            // City	StateProvinceID	PostalCode
            // Bothell 79  98011
            using (var db = new Database(ConnectionStrings.AdventureWorks, "System.Data.SqlClient"))
            {
                var c = new SqlParameter();
                c.Value = "Bothell";
                int count = 0;
                foreach (var address in db.Query<Address>(
                    $"{sql}WHERE City = @0",
                    c))
                {
                    Debug.WriteLine(address);
                    count++;
                }
                Assert.AreEqual(26, count);
            }
        }

        [Test]
        public void QueryUsingParamName_Fails()
        {
            // City	StateProvinceID	PostalCode
            // Bothell 79  98011
            using (var db = new Database(ConnectionStrings.AdventureWorks, "System.Data.SqlClient"))
            {
                var c = new SqlParameter("c", 0);
                c.Direction = ParameterDirection.ReturnValue;
                Assert.Throws<ArgumentException>(() =>
                {
                    foreach (var address in db.Query<Address>(
                        $"{sql}WHERE a.City = @City",
                        new SqlParameter("@City", "Bothell")))
                    {
                        Debug.WriteLine(address);
                    }
                });
            }
        }

        [Test]
        public void OutputParamFromRawSql_Succeeds()
        {
            using (var db = new Database(ConnectionStrings.AdventureWorks, "System.Data.SqlClient"))
            {
                var i0 = db.CreateParameter("ignored", 1);
                var i1 = db.CreateParameter("ignored", 3);
                var o = db.CreateParameter("ignored", (int?)0, ParameterDirection.Output);
                var count = db.Query<dynamic>("SELECT @2 = @0 + @1",
                    i0, i1, o).ToList().Count;
                Assert.AreEqual(4, o.Value);
                Assert.AreEqual("@0", o.ParameterName);
            }
        }

        [Test]
        public void OutputFromRawSqlRepeatedParam_Fails()
        {
            using (var db = new Database(ConnectionStrings.AdventureWorks, "System.Data.SqlClient"))
            {
                var i0 = db.CreateParameter("ignored", 1);
                var i1 = db.CreateParameter("ignored", 3);
                var o = db.CreateParameter("ignored", (int?)0, ParameterDirection.Output);
                Assert.Throws<ArgumentException>(() => {
                    // this should be able to work, it works on Massive or Mighty and on Dapper
                    var count = db.Query<dynamic>("SELECT @2 = @0 + @0 + @1",
                        i0, i1, o).ToList().Count;
                });
            }
        }
    }
}
