using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

using TASKMNGMT.Pages;


namespace TASKMNGMT.Models.ViewModels
{
    public partial class TODO_ViewModel: ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<ToDo> toDos;

        [ObservableProperty]
        private ObservableCollection<string> statuslist;

        [ObservableProperty]
        private string selectedStatusList;

        [ObservableProperty]
        private ToDo toDo;

        [ObservableProperty]
        private string buttonstatus;

        [ObservableProperty]
        private ToDo selectedItem;

        [ObservableProperty]
        private bool isRefreshing;
        


        private Popup _popupWindow;

        public bool isselected => selectedItem != null;

        public bool IsPagination => ToDos.Count > 5;

        public TODO_ViewModel()
        {
            ToDos = new ObservableCollection<ToDo>();
            ToDo= new ToDo();
            isRefreshing = false;
            selectedItem = null;
            Statuslist = new ObservableCollection<string>
            {
                "Done",
                "Pending",
                "On Hold",
                "Rejected",
                "Other"
            };
            _popupWindow = new Popup();
        }

        partial void OnSelectedItemChanged(ToDo value)
        {
            OnPropertyChanged(nameof(isselected));
        }

       

        [RelayCommand]
        private void RefreashCommand()
        {
            IsRefreshing = true;
            var refreshtodos = new ObservableCollection<ToDo>(toDos);

            toDos.Clear();
            foreach (var todo in refreshtodos)
            {
                ToDos.Add(new ToDo
                {
                    id = todo.id,
                    ToDoTask = todo.ToDoTask,
                    ToDoComments = todo.ToDoComments,
                    Status = todo.Status,
                    AddDate = todo.AddDate,
                    EditDate = todo.EditDate,
                });
            }
            IsRefreshing = false;
        }

        [RelayCommand]
        private void OpenAddTodoPopup()
        {
            buttonstatus = "Save";
            var popup = new AddTODO
            {
                BindingContext = this
            };

            Microsoft.Maui.Controls.Application.Current.MainPage.ShowPopup(popup);
            _popupWindow = popup;
        }

        [RelayCommand]
        private async void AddToDoToList()
        {
            //Add to Project Master Sheet
            //Create Project Sheet with NAME_TODO_MASTER and NAME_TASKMASTER
            //Create Headers on both TODOID	TODONAME	STATUS	COMMENTS	ADD_DATE	END_DATE ,
            //TASKID	TASKNAME	START_DATE	START_TIME	END_TIME	TIME_TAKEN(MIN)


            if (!string.IsNullOrWhiteSpace(ToDo.ToDoTask) && !string.IsNullOrWhiteSpace(ToDo.ToDoComments))
            {
                if(buttonstatus == "Save")
                {
                    ToDos.Add(new ToDo
                    {
                        id = ToDos.Count() + 1,
                        ToDoTask = ToDo.ToDoTask,
                        ToDoComments = ToDo.ToDoComments,
                        Status = selectedStatusList,
                        AddDate = DateTime.Now.ToString("dd-MM-yyyy")
                    });

                    // Optionally clear the inputs after saving
                    toDo.ToDoTask = string.Empty;
                    toDo.ToDoComments = string.Empty;
                }
                else if (buttonstatus == "Update")
                {
                    var item = ToDos.FirstOrDefault(t =>t.id == toDo.id);

                    if(item != null) 
                    {
                        item.ToDoTask = toDo.ToDoTask;
                        item.ToDoComments = toDo.ToDoComments;
                        item.Status = selectedStatusList;
                        item.AddDate = selectedItem.AddDate;
                        item.EditDate = DateTime.Now.ToString("dd-MM-yyyy");
                        var index = ToDos.IndexOf(item);
                        ToDos[index] = item;
                    }
                    ToDo.ToDoTask = "";
                    ToDo.ToDoComments = "";
                    selectedStatusList = "Pending";

                    RefreashCommand();
                }
                // Refresh the DataGrid
                OnPropertyChanged(nameof(IsPagination));


            }
            // Close the popup
            if (_popupWindow != null)
            {
                _popupWindow.Close();
            }
        }

        [RelayCommand]
        private async void EditToDoItem()
        {
            if (SelectedItem != null)
            {
                buttonstatus = "Update";
                toDo.id = selectedItem.id;
                toDo.ToDoTask = selectedItem.ToDoTask;
                toDo.ToDoComments = selectedItem.ToDoComments;
                SelectedStatusList = selectedItem.Status;
                var popup = new AddTODO
                {
                    BindingContext = this
                };


                Microsoft.Maui.Controls.Application.Current.MainPage.ShowPopup(popup);
                _popupWindow = popup;
            }
        }

        [RelayCommand]
        private async void DeleteToDoItem()
        {
            if (SelectedItem != null)
            {
                toDos.Remove(selectedItem);
                selectedItem = null;
                OnPropertyChanged(nameof(IsPagination));
            }

        }
    };
}
