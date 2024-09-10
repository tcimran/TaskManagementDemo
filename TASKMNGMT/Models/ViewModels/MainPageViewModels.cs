using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TASKMNGMT.ViewModels;

namespace TASKMNGMT.Models.ViewModels
{
    public partial class MainPageViewModels:ObservableObject
    {
        [ObservableProperty]
        private bool isToDoListVisible;


        public bool isTaskListVisible => !isToDoListVisible;

        private string _btnColor = "#512BD4";
        public string BtnColor
        {
            get => _btnColor;
            set => SetProperty(ref _btnColor, value);
        }

        private string _btnText = "To-Do";

        public string BtnText
        {
            get => _btnText;
            set => SetProperty(ref _btnText, value);
        }


        public ProjectDetailsViewModel projectvm { get; set; }
        public TODO_ViewModel todovm { get; set; }
        public TaskMaViewModel taskvm { get; set; }

        public MainPageViewModels()
        {
            projectvm = new ProjectDetailsViewModel();
            todovm = new TODO_ViewModel();
            taskvm = new TaskMaViewModel(projectvm);
            isToDoListVisible = false;
        }

        [RelayCommand]
        private void ToggleToDoList()
        {
            IsToDoListVisible = !IsToDoListVisible;
            
        }

        partial void OnIsToDoListVisibleChanged(bool value)
        {
            if(isTaskListVisible)
            {
                BtnColor = "#512BD4";
                BtnText = "To-Do";
            }
            else
            {
                BtnColor = "#ADD8E6";
                BtnText = "Task List";
            }
            OnPropertyChanged(nameof(isTaskListVisible));

        }

    }
}
