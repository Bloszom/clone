using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace pcea.Models
{
    public class WorkflowDetails
    {
        [Key]
        public long RecId { get; set; }
        public string ProcessId { get; set; }
        public string RoleId { get; set; }
        public string InputRoleId { get; set; }
        public string OutputRoleId { get; set; }

    }
}
