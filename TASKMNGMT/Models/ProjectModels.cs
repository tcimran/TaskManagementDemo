using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TASKMNGMT.Models
{
    public class ProjectModels
    {
        public int? id {  get; set; }
        public string? ProjectName { get; set; }
        public string? ProjectDescription { get; set; }
        public bool IsCompleted { get; set; }
    }
}
