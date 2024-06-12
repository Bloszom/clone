using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using _FrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace pceaLibrary
{
    public class TaskMgt: _Mail
    {
        private readonly IHostingEnvironment hosting;

        public TaskMgt(IHostingEnvironment hosting)
        {
            this.hosting = hosting;
        }
        public bool CommitSubmissionToWorkflow(string ProcessId, string FormId, long EntryId, string OperatorId, string OperatorName)
        {
            return true;
        }





    }
}
