using _FrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace pceaLibrary
{
    public class Report : _Database
    {
        public Report()
        {

        }

        public DataTable ExecuteDataTable(string cmdText)
        {
            DataTable dataTable = new DataTable();

            using (SqlCommand cmd = new SqlCommand())
            {

                cmd.CommandText = cmdText;
                dataTable = ExecuteDataTable(cmd);

                if (dataTable == null)
                {
                    FriendlyErrorMessage = "Error getting system configuration.  Contact Administrator.";
                }
            };

            return dataTable;
        }
    }
}
