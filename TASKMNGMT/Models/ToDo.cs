using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TASKMNGMT.Models
{
    public class ToDo
    {
        public int id { get; set; }
        public string ToDoTask { get; set; }
        public string ToDoComments { get; set; }
        public string Status { get; set; }
        public string? AddDate { get; set; }
        public string? EditDate { get; set; }


    }
}
