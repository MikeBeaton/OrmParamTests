using System;
using System.Collections.Generic;
using System.Text;

namespace OrmSpTesting
{
    public static class ConnectionStrings
    {
        public const string AdventureWorks = "data source=sqlserver.test.local;initial catalog=AdventureWorks;User Id=mightytests;Password=testpassword;persist security info=False;packet size=4096";
        public const string AdventureWorksFull = "data source=sqlserver.test.local;initial catalog=AdventureWorks;User Id=mightytests;Password=testpassword;persist security info=False;packet size=4096;providerName=System.Data.SqlClient";
    }
}
