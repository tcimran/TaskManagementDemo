using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TASKMNGMT.Models.HelperClasses
{
    public class TaskDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ReadOnlyTemplate { get; set; }
        public DataTemplate EditableTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var task = item as TaskMang; // Assuming TaskMang is your model
            if (task?.IsEditable == true)
            {
                return EditableTemplate;
            }
            return ReadOnlyTemplate;
        }
    }
}
