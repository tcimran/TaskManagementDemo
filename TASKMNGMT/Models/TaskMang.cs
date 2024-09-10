using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TASKMNGMT.Models
{
    public class TaskMang: ObservableObject
    {
        public int id {  get; set; }
        public string TaskName { get; set; }
        public string TaskStartTime { get; set; }
        public string TaskEndTime { get; set; } 
        public string TaskTimeTaken { get; set; }

        private bool _isEditable;
        public bool IsEditable
        {
            get => _isEditable;
            set => SetProperty(ref _isEditable, value);
        }
        private ProjectModels? _selectedProject;
        public ProjectModels? SelectedProject
        {
            get => _selectedProject;
            set
            {
                SetProperty(ref _selectedProject, value);
                if (_selectedProject != null)
                {
                    // Update the ProjectName when the project is selected
                    ProjectName = _selectedProject.ProjectName;

                }
            }
        }

        private string? _projectName;
        public string? ProjectName
        {
            get => _projectName;
            set => SetProperty(ref _projectName, value);
        }
    }
}
