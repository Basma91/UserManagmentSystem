using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagmentSystem.ViewModels
{
    public class RoleFormViewModel
    {
        [Required,StringLength(100)]
        public string Name { get; set; }
    }
}
