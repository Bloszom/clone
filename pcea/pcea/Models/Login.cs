using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace pcea.Models
{
    public partial class Login
    {
        [Required(ErrorMessage = "required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }
}
