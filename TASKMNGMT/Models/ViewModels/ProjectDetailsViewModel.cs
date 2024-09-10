using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OfficeOpenXml;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using TASKMNGMT.Models;
using TASKMNGMT.Pages;

namespace TASKMNGMT.ViewModels
{
    public partial class ProjectDetailsViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<ProjectModels> projects;

        [ObservableProperty]
        private ProjectModels project;

        [ObservableProperty]
        private ProjectModels selectedProject;

        public bool IsProjectSelected => SelectedProject.ProjectName !="Default" ;

        private Popup _popupWindow;

        public ProjectDetailsViewModel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            Projects = new ObservableCollection<ProjectModels>
            {
                new ProjectModels { ProjectName = "Default", ProjectDescription = "Default Selection", IsCompleted = false },
            };

            Project = new ProjectModels();
            _popupWindow = new Popup();
            selectedProject = Projects.FirstOrDefault();
            EnsureExcelFile();
            LoadTasksFromExcel();
        }
        private void LoadTasksFromExcel()
        {
            var filepath = "C:\\Users\\USER\\Documents\\TaskApp.xlsx";
            if (!File.Exists(filepath))
            {
                EnsureExcelFile();
            }
            using (var package = new ExcelPackage(new FileInfo(filepath)))
            {
                var worksheet = package.Workbook.Worksheets["Project_Master"];
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    var pro = new ProjectModels
                    {
                        id = int.Parse(worksheet.Cells[row, 1].Text),
                        ProjectName = worksheet.Cells[row, 2].Text,
                        ProjectDescription = worksheet.Cells[row, 3].Text,
                        IsCompleted = bool.Parse(worksheet.Cells[row, 4].Text),
                    };
                    if (pro != null)
                    {
                        Projects.Add(pro);
                    }
                }

            }
        }
        public async Task<bool> EnsureExcelFile()
        {
            string filePath = "C:\\Users\\USER\\Documents\\TaskApp.xlsx";

            if (!File.Exists(filePath))
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var workbook = package.Workbook;
                    var taskmastersheet = workbook.Worksheets.Add("Task_Master");
                    taskmastersheet.Cells[1, 1].Value = "TASK_ID";
                    taskmastersheet.Cells[1, 2].Value = "TASK_NAME";
                    taskmastersheet.Cells[1, 3].Value = "START_TIME";
                    taskmastersheet.Cells[1, 4].Value = "END_TIME";
                    taskmastersheet.Cells[1, 5].Value = "TIME_TAKEN";
                    taskmastersheet.Cells[1, 6].Value = "PROJECT_NAME";

                    var todomastersheet = workbook.Worksheets.Add("Todo_Master");
                    todomastersheet.Cells[1, 1].Value = "TODO_ID";
                    todomastersheet.Cells[1, 2].Value = "TODO_NAME";
                    todomastersheet.Cells[1, 3].Value = "STATUS";
                    todomastersheet.Cells[1, 4].Value = "COMMENTS";
                    todomastersheet.Cells[1, 5].Value = "ADD_DATE";
                    todomastersheet.Cells[1, 6].Value = "END_DATE";
                    todomastersheet.Cells[1, 7].Value = "PROJECT_NAME";

                    var projectmaster = workbook.Worksheets.Add("Project_Master");
                    projectmaster.Cells[1, 1].Value = "PROJECT_ID";
                    projectmaster.Cells[1, 2].Value = "PROJECT_NAME";
                    projectmaster.Cells[1, 3].Value = "PROJECT_DESCRIPTION";
                    projectmaster.Cells[1, 4].Value = "PROJECT_STATUS";
                    projectmaster.Cells[1, 5].Value = "ADD_DATE";
                    projectmaster.Cells[1, 6].Value = "DONE_DATE";

                    package.Save();
                    return true;
                }

            }
            return false;
        }

        partial void OnSelectedProjectChanged(ProjectModels value)
        {
            OnPropertyChanged(nameof(IsProjectSelected));
        }

        [RelayCommand]
        private void OpenAddProjectPopup()
        {
            var popup = new AddProject
            {
                BindingContext = this
            };

            Application.Current.MainPage.ShowPopup(popup);
            _popupWindow = popup;
        }

        [RelayCommand]
        private async void AddProjectToList()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var snackbarOptions = new SnackbarOptions
            {
                BackgroundColor = Colors.Red,
                TextColor = Colors.Green,
                ActionButtonTextColor = Colors.Yellow,
                CornerRadius = new CornerRadius(10),
                Font = Microsoft.Maui.Font.SystemFontOfSize(14),
                ActionButtonFont = Microsoft.Maui.Font.SystemFontOfSize(14),
                CharacterSpacing = 0.5
            };
            TimeSpan duration = TimeSpan.FromSeconds(3);
            string text = "This is a Snackbar";
            string actionButtonText = "Click Here to Dismiss";
            try
            {
                string filePath = "C:\\Users\\USER\\Documents\\TaskApp.xlsx";
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets["Project_Master"];
                    if (worksheet == null)
                    {
                        worksheet = package.Workbook.Worksheets.Add("Project_Master");
                    }
                    if(project.IsCompleted == null)
                    {
                        project.IsCompleted = false;
                    }
                    int rowcount = worksheet.Dimension?.Rows ?? 0;
                    int newrow = rowcount + 1;
                    int starting_id = rowcount - 1;
                    worksheet.Cells[newrow, 1].Value = starting_id;
                    worksheet.Cells[newrow, 2].Value = project.ProjectName;
                    worksheet.Cells[newrow, 3].Value = project.ProjectDescription;
                    worksheet.Cells[newrow, 4].Value = project.IsCompleted;
                    package.Save();

                    var newproj = new ProjectModels
                    {
                        id = starting_id,
                        ProjectName = project.ProjectName,
                        ProjectDescription = project.ProjectDescription,
                        IsCompleted = project.IsCompleted,

                    };
                    Projects.Add(newproj);




                    text = $"{project.ProjectName} is saved";




                    //Action action = async () => Application.Current.MainPage.DisplayAlert("Title", "Message", "OK");

                }
            }
            catch (Exception ex)
            {
                text = "Error in saving";
            }

            var snackbar = Snackbar.Make(text, null, actionButtonText, duration, snackbarOptions);

            await snackbar.Show(cancellationTokenSource.Token);
            project.ProjectName = string.Empty;
            project.ProjectDescription = string.Empty;
            project.IsCompleted = false;
            if (_popupWindow != null)
            {
                _popupWindow.Close();
            }

        }

        [RelayCommand]
        private async void DeleteProject()
        {
            //projects.Remove(selectedProject);
            DeleteProjectFromExcl();
        }

        private void DeleteProjectFromExcl()
        {
            string filePath = "C:\\Users\\USER\\Documents\\TaskApp.xlsx";
            using(var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets["Project_Master"];
                if(worksheet == null)
                {
                    return;
                }
                int rowCount = worksheet.Dimension?.Rows ?? 0;
                int rowToDelete = -1;
                for (int row = 2; row <= rowCount; row++)
                {
                    int excelTaskId = Convert.ToInt32(worksheet.Cells[row, 1].Value);
                    if (excelTaskId == selectedProject!.id)
                    {
                        rowToDelete = row;
                        worksheet.DeleteRow(rowToDelete);
                        rowCount--;
                        break;
                    }
                }
                if (rowToDelete != -1)
                {
                    // Now, update the IDs of the rows below the deleted row
                    for (int row = 2; row <= rowCount; row++)
                    {
                        // Update the ID column (assuming it's in column 1)
                        worksheet.Cells[row, 1].Value = row - 1; // New ID should be previous row number
                    }

                    // Save the updated Excel file
                    package.Save();
                    Projects.Remove(selectedProject);
                }
            }
        }

        [RelayCommand]
        private void ontogglechanged()
        {
            string filePath = "C:\\Users\\USER\\Documents\\TaskApp.xlsx";

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets["Project_Master"];
                if (worksheet == null)
                {
                    return;
                }
                int rowCount = worksheet.Dimension?.Rows ?? 0;
                int rowToDelete = -1;
                for (int row = 2; row <= rowCount; row++)
                {
                    int excelTaskId = Convert.ToInt32(worksheet.Cells[row, 1].Value);
                    if (excelTaskId == selectedProject!.id)
                    {
                        worksheet.Cells[row, 4].Value = selectedProject.IsCompleted;
                        package.Save();
                        break;
                    }
                }
            }
        }
    }
}
