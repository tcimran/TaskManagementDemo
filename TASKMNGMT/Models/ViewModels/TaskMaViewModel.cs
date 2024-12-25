using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MvvmHelpers;
using OfficeOpenXml;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Threading.Tasks;
using System.Timers;
using TASKMNGMT.Models.HelperClasses;
using TASKMNGMT.ViewModels;
using Cell = DocumentFormat.OpenXml.Spreadsheet.Cell;
using Colors = Microsoft.Maui.Graphics.Colors;


namespace TASKMNGMT.Models.ViewModels
{
    public  partial class TaskMaViewModel:CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        #region Property
        private ProjectDetailsViewModel _projectDetailsViewModel;


        private bool _isPaused;
        public bool IsnewTask => !string.IsNullOrEmpty(Taskname) & Timer != "00:00:00" & !istimer;
        public bool enablestart;
        public bool istimer => enablestart;
        public bool ispause => !istimer;
        private bool _isEditable;
        public bool Isinputable => CurrentTask == "None" & enablestart == false;
        public bool isselected => selectedItem != null;
        public bool IsPagination => Tasks.Count > 5;

        private TimeSpan timeTakenForTask;
        private Stopwatch _stopwatch;
        private System.Timers.Timer _timer;
        private TimeSpan _taskStartTime;
        public TaskMang taskis;
        private Popup _popupWindow;

        public bool showentryfield => !IsEditable;

        private string _btnstatus;
        // Property to control the editability of the Entry control
        public bool IsEditable
        {
            get => _isEditable;
            set => SetProperty(ref _isEditable, value);
        }

        public string BtnStatus
        {
            get => _btnstatus;
            set => SetProperty(ref _btnstatus, value);
        }

        public ProjectDetailsViewModel projvm
        {
            get => _projectDetailsViewModel;
            set => SetProperty(ref _projectDetailsViewModel, value);
        } 
        #endregion

        #region ObservableProperties

        [ObservableProperty]
        private ObservableCollection<TaskMang> tasks;

        [ObservableProperty]
        private TaskMang selectedItem;

        private string endTime;

        [ObservableProperty]
        private string timer = "00:00:00";

        [ObservableProperty]
        private string taskTimer = "00";

        [ObservableProperty]
        private string currentTask = "None";

        [ObservableProperty]
        private string taskname;

        [ObservableProperty]
        private string startButtonText = "Start";

        [ObservableProperty]
        private bool isRefreshing;
        #endregion

        public TaskMaViewModel(ProjectDetailsViewModel projectDetailsViewModel)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            _projectDetailsViewModel = projectDetailsViewModel;
            _stopwatch = new Stopwatch();
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += OnTimerChanged;
            _isPaused = false;
            enablestart = true;
            BtnStatus = "Edits";
            tasks = new BulkObservableCollection<TaskMang>();
            //isRefreshing = false;
            selectedItem = null;
            //_popupWindow = new Popup();
            //LoadTasksFromExcelAsync();

            // Load data asynchronously
            LoadData();

        }

        private async void LoadData()
        {
            //await LoadDataAsync();
            await DemoUploadAsync();
        }

        public async Task LoadDataAsync()
        {
            var filepath = "C:\\Users\\USER\\Documents\\TaskApp.xlsx";
            if (!File.Exists(filepath))
            {
                await _projectDetailsViewModel.EnsureExcelFile();
            }
            var tasks = await Task.Run(() => LoadTasksFromExcel(filepath));

            var bulkCollection = new BulkObservableCollection<TaskMang>();

            bulkCollection.AddRange(tasks);
            //Tasks = new ObservableRangeCollection<TaskMang>(bulkCollection);
        }

        private List<TaskMang> LoadTasksFromExcel(string filepath)
        {
            var tasks = new List<TaskMang>();

            try
            {
                using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filepath, false))
                {
                    WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                    Sheet taskSheet = workbookPart.Workbook.Sheets.Elements<Sheet>().FirstOrDefault(s => s.Name == "Task_Master");

                    if (taskSheet == null)
                    {
                        throw new Exception("Sheet 'Task_Master' not found");
                    }

                    WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(taskSheet.Id);
                    SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();

                    if (sheetData != null)
                    {
                        foreach (Row row in sheetData.Elements<Row>().Skip(1))
                        {
                            var task = new TaskMang
                            {
                                id = int.Parse(GetCellValue(workbookPart, row.Elements<Cell>().ElementAtOrDefault(0))),
                                TaskName = GetCellValue(workbookPart, row.Elements<Cell>().ElementAtOrDefault(1)),
                                TaskStartTime = GetCellValue(workbookPart, row.Elements<Cell>().ElementAtOrDefault(2)),
                                TaskEndTime = GetCellValue(workbookPart, row.Elements<Cell>().ElementAtOrDefault(3)),
                                TaskTimeTaken = GetCellValue(workbookPart, row.Elements<Cell>().ElementAtOrDefault(4)),
                                ProjectName = GetCellValue(workbookPart, row.Elements<Cell>().ElementAtOrDefault(5)),
                            };

                            task.SelectedProject = _projectDetailsViewModel.Projects.FirstOrDefault(p => p.ProjectName == task.ProjectName);
                            tasks.Add(task);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error loading tasks: {e.Message}");
            }

            return tasks;
        }


        public async Task DemoUploadAsync()
        {
            //var taskstoAdd = new List<TaskMang>();

            await LoadTasksFromExcel();
            //await Task.Run(() =>
            //{

            //    LoadTasksFromExcel();
            //});
            //await MainThread.InvokeOnMainThreadAsync(() =>
            //{
            //    foreach(TaskMang task in taskstoAdd)
            //    {
            //        Tasks.Add(task);
            //    }

            //});
        }













        #region CurrentWorkingMethods

        partial void OnSelectedItemChanged(TaskMang value)
        {
            OnPropertyChanged(nameof(isselected));
        }

        







        [RelayCommand]
        private async Task Refreash()
        {
            IsRefreshing = true;
            await LoadTasksFromExcel();
            var refreshtodos = new ObservableCollection<TaskMang>(Tasks);
            Tasks.Clear();

            foreach (var todo in refreshtodos)
            {
                Tasks.Add(new TaskMang
                {
                    id = todo.id,
                    TaskName = todo.TaskName,
                    TaskStartTime = todo.TaskStartTime,
                    TaskEndTime = todo.TaskEndTime,
                    TaskTimeTaken = todo.TaskTimeTaken,
                    ProjectName = todo.ProjectName,
                });
            }
            IsRefreshing = false;
        }

        private async Task LoadTasksFromExcel()
        {
            Tasks.Clear();
            var filepath = "C:\\Users\\USER\\Documents\\TaskApp.xlsx";

            if (!File.Exists(filepath))
            {
                await _projectDetailsViewModel.EnsureExcelFile();
            }

            var tasking = await Task.Run(() =>
            {
                var temptasking = new List<TaskMang>();

                try
                {
                    // Open the Excel file in OpenXML format.
                    using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filepath, false))
                    {
                        // Get the workbook part
                        WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

                        // Get the first worksheet (or by name "Task_Master")
                        Sheet taskSheet = workbookPart.Workbook.Sheets.Elements<Sheet>().FirstOrDefault(s => s.Name == "Task_Master");
                        if (taskSheet == null)
                        {
                            throw new Exception("Sheet 'Task_Master' not found");
                        }

                        // Get the worksheet part for that sheet
                        WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(taskSheet.Id);

                        // Access the data in the worksheet
                        SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();

                        if (sheetData != null)
                        {
                            // Loop through each row starting from the second row (to skip the header)
                            foreach (Row row in sheetData.Elements<Row>().Skip(1))
                            {
                                // Extract each cell value from the row
                                var task = new TaskMang
                                {
                                    id = int.Parse(GetCellValue(workbookPart, row.Elements<Cell>().ElementAtOrDefault(0))),
                                    TaskName = GetCellValue(workbookPart, row.Elements<Cell>().ElementAtOrDefault(1)),
                                    TaskStartTime = GetCellValue(workbookPart, row.Elements<Cell>().ElementAtOrDefault(2)),
                                    TaskEndTime = GetCellValue(workbookPart, row.Elements<Cell>().ElementAtOrDefault(3)),
                                    TaskTimeTaken = GetCellValue(workbookPart, row.Elements<Cell>().ElementAtOrDefault(4)),
                                    ProjectName = GetCellValue(workbookPart, row.Elements<Cell>().ElementAtOrDefault(5)),
                                };

                                // Match project from ViewModel and add to temporary list
                                task.SelectedProject = _projectDetailsViewModel.Projects.FirstOrDefault(p => p.ProjectName == task.ProjectName);
                                temptasking.Add(task);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Error loading tasks: {e.Message}");
                }

                return temptasking;
            });

            // Final update to the task list
            UpdateTaskList(tasking);
        }

        private string GetCellValue(WorkbookPart workbookPart, Cell cell)
        {
            if (cell == null)
                return null;

            string value = cell.InnerText;

            // If the cell represents a number or a date
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                var stringTable = workbookPart.SharedStringTablePart.SharedStringTable;
                value = stringTable.ElementAt(int.Parse(value)).InnerText;
            }

            return value;
        }

        private void UpdateTaskList(List<TaskMang> tasking)
        {
            foreach (var task in tasking)
            {
                // Application is on hold while adding to Task Observable list
                Tasks.Add(task); // Add to the observable collection
            }
        }


        

        private void OnTimerChanged(object? sender, ElapsedEventArgs e)
        {
            Timer = _stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
            if(!string.IsNullOrEmpty(CurrentTask) & CurrentTask != "None")
            {
                timeTakenForTask = _stopwatch.Elapsed - _taskStartTime;
                TaskTimer = timeTakenForTask.ToString(@"mm\:ss");
            }
            
        }

        [RelayCommand]
        private void StartTimer()
        {
            if (_isPaused)
            {
                _stopwatch.Start();
                _timer.Start();
                StartButtonText = "Start";
                _isPaused = false;
                enablestart = false;
            }
            else
            {
                _stopwatch.Start();
                _timer.Start();
                enablestart = false;
            }
            OnPropertyChanged(nameof(istimer));
            OnPropertyChanged(nameof(ispause));
            OnPropertyChanged(nameof(IsnewTask));
            OnPropertyChanged(nameof(Isinputable));

        }

        [RelayCommand]
        private void StopTimer()
        {
            ResetTimer();
            enablestart = true;
            
            OnPropertyChanged(nameof(IsnewTask));
            OnPropertyChanged(nameof(ispause));
            OnPropertyChanged(nameof(Isinputable));


        }

        [RelayCommand]
        private void PauseTimer()
        {
            _stopwatch.Stop();
            _timer.Stop();
            StartButtonText = "Resume";
            _isPaused = true;
            enablestart = true;
            OnPropertyChanged(nameof(istimer));
            OnPropertyChanged(nameof(ispause));
            OnPropertyChanged(nameof(IsnewTask));
        }

        [RelayCommand]
        private void ResetTimer()
        {
            _stopwatch.Reset();
            Timer = "00:00:00";
            CurrentTask = "None";
            TaskTimer = "00";
            enablestart = true;
            StartButtonText = "Start";
            OnPropertyChanged(nameof(istimer));
        }
        partial void OnTasknameChanged(string value)
        {
            OnPropertyChanged(nameof(IsnewTask));
        }


        [RelayCommand]
        private void AddTask()
        {
            CurrentTask = taskname;
            _taskStartTime = _stopwatch.Elapsed;
            Taskname = "";
            OnPropertyChanged(nameof(Isinputable));
        }
        [RelayCommand]
        private async void SaveTask()
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
            if(CurrentTask  == "None")
            {
               await Application.Current.MainPage.DisplayAlert("Failed", $"Task is None ==>{CurrentTask}", "OK");
                return;
            }
            try
            {
                string filePath = "C:\\Users\\USER\\Documents\\TaskApp.xlsx";
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets["Task_Master"];
                    if(worksheet == null)
                    {
                        worksheet = package.Workbook.Worksheets.Add("Task_Master");
                    }
                    int rowcount = worksheet.Dimension?.Rows ?? 0; // 1 will return as header is there
                    int newrow = rowcount + 1;
                    int starting_id = rowcount;
                    worksheet.Cells[newrow, 1].Value = starting_id;
                    worksheet.Cells[newrow, 2].Value = CurrentTask;
                    worksheet.Cells[newrow, 3].Value = _taskStartTime.ToString(@"hh\:mm\:ss");
                    worksheet.Cells[newrow, 4].Value = _stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
                    worksheet.Cells[newrow, 5].Value = timeTakenForTask.ToString(@"hh\:mm\:ss");
                    worksheet.Cells[newrow, 6].Value = _projectDetailsViewModel.SelectedProject?.ProjectName;
                    package.Save();

                    var newTask = new TaskMang
                    {
                        id = starting_id,
                        TaskName = CurrentTask,
                        TaskStartTime = _taskStartTime.ToString(@"hh\:mm\:ss"),
                        TaskEndTime  = _stopwatch.Elapsed.ToString(@"hh\:mm\:ss"),
                        TaskTimeTaken = timeTakenForTask.ToString(@"hh\:mm\:ss"),
                        ProjectName = _projectDetailsViewModel.SelectedProject?.ProjectName
                    };
                    newTask.SelectedProject = _projectDetailsViewModel.Projects.FirstOrDefault(p => p.ProjectName == _projectDetailsViewModel.SelectedProject?.ProjectName);
                    OnPropertyChanged(nameof(TaskMang.SelectedProject));
                    OnPropertyChanged(nameof(_projectDetailsViewModel.Projects));
                    OnPropertyChanged(nameof(newTask.IsEditable));
                    Tasks.Add(newTask);
                    text = $"{CurrentTask} is saved";

                }
            }
            catch (Exception ex)
            {
                text = "Error in saving";
            }

            var snackbar = Snackbar.Make(text, null, actionButtonText, duration, snackbarOptions);

            await snackbar.Show(cancellationTokenSource.Token);
            Taskname = "";
            Timer = "00:00:00";
            CurrentTask = "None";
            OnPropertyChanged(nameof(IsnewTask));
            OnPropertyChanged(nameof(Isinputable));
        }

        [RelayCommand]
        private void EditTask(TaskMang tasks)
        {
   
            if(tasks != null)
            {
                tasks.IsEditable = !tasks.IsEditable;
                IsEditable = tasks.IsEditable;
                OnPropertyChanged(nameof(IsEditable));  
                OnPropertyChanged(nameof(showentryfield));  
            }
            if (tasks.IsEditable)
            {
                BtnStatus = "Save";
                return;
            }
            else
            {
                BtnStatus = "Edit";
                UpdateTaskExcel(tasks);
            }
            OnPropertyChanged(nameof(BtnStatus));
        }

        private void UpdateTaskExcel(TaskMang Taskid)
        {
            try
            {
                string filePath = "C:\\Users\\USER\\Documents\\TaskApp.xlsx";
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets["Task_Master"];
                    if (worksheet == null)
                    {
                        Application.Current.MainPage.DisplayAlert("Failed", "WorkSheet not found", "OK");
                        return;
                    }
                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        int task = int.Parse(worksheet.Cells[row, 1].Text);
                        if (task == Taskid.id)
                        {
                            worksheet.Cells[row, 2].Value = Taskid.TaskName;
                            worksheet.Cells[row, 3].Value = Taskid.TaskStartTime;
                            worksheet.Cells[row, 4].Value = Taskid.TaskEndTime;
                            worksheet.Cells[row, 5].Value = Taskid.TaskTimeTaken;
                            worksheet.Cells[row, 6].Value = Taskid.SelectedProject?.ProjectName;
                            package.Save();
                            break;
                        }
                    }
                    Application.Current.MainPage.DisplayAlert("Success", "Task updated successfully.", "OK");
                }
            }
            catch (Exception ex)
            {
                Application.Current.MainPage.DisplayAlert("Failed", $"Task Error:{ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private void DeleteTask(TaskMang tasks)
        {
           if(tasks !=null)
            {
                DeleteTaskFromExcel(tasks.id);
                //LoadTasksFromExcelAsync();
            }
        }

        private void DeleteTaskFromExcel(int taskId)
        {
            string filePath = "C:\\Users\\USER\\Documents\\TaskApp.xlsx";
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets["Task_Master"];

                if (worksheet == null)
                {
                    return; // Task_Master sheet not found
                }

                // Find the row with the matching task ID and delete it
                int rowCount = worksheet.Dimension?.Rows ?? 0;
                int rowToDelete = -1;

                for (int row = 2; row <= rowCount; row++) // Start from row 2 (assuming headers in row 1)
                {
                    int excelTaskId = Convert.ToInt32(worksheet.Cells[row, 1].Value);
                    if (excelTaskId == taskId)
                    {
                        rowToDelete = row;
                        worksheet.DeleteRow(rowToDelete);
                        rowCount--; // Reduce row count as we have deleted a row
                        break;
                    }
                }

                if (rowToDelete != -1)
                {
                    // Now, update the IDs of the rows below the deleted row
                    for (int row =2; row <= rowCount; row++)
                    {
                        // Update the ID column (assuming it's in column 1)
                        worksheet.Cells[row, 1].Value = row - 1; // New ID should be previous row number
                    }

                    // Save the updated Excel file
                    package.Save();
                }
            }
        }
        #endregion

        //[RelayCommand]
        //private async void OpenTaskEditPopup()
        //{
        //    var popup = new EditTaskpopup
        //    {
        //        BindingContext = this,
        //    };
        //    Microsoft.Maui.Controls.Application.Current.MainPage.ShowPopup(popup);
        //    _popupWindow = popup;
        //}
    };
}
