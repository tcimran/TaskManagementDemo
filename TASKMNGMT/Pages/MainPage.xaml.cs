using TASKMNGMT.Models.ViewModels;
namespace TASKMNGMT.Pages;

public partial class MainPage : ContentPage
{
    private Entry projectEntry;
    private Picker projectPicker;
    public MainPage()
	{
		InitializeComponent();
        BindingContext = new MainPageViewModels(); // Your ViewModel
    }
    private void OnViewCellAppearing(object sender, EventArgs e)
    {
        var viewCell = sender as ViewCell;
        if (viewCell != null)
        {
            var gridContent = viewCell.FindByName<Grid>("gridcontent");
            if (gridContent != null)
            {
                // Create and bind the Entry (for project name)
                var projectEntry = new Entry
                {
                    IsEnabled = false, // Read-only
                    FontSize = 12
                };
                projectEntry.SetBinding(Entry.TextProperty, new Binding("SelectedProject.ProjectName", source: viewCell.BindingContext, mode: BindingMode.TwoWay));
                projectEntry.SetBinding(Entry.IsVisibleProperty, new Binding("taskvm.showentryfield", source: viewCell.BindingContext));

                // Create and bind the Picker (for selecting projects)
                var projectPicker = new Picker
                {
                    FontSize = 12
                };
                projectPicker.SetBinding(Picker.ItemsSourceProperty, new Binding("projectvm.Projects", source: BindingContext));
                projectPicker.SetBinding(Picker.SelectedItemProperty, new Binding("SelectedProject", source: viewCell.BindingContext, mode: BindingMode.TwoWay));
                projectPicker.ItemDisplayBinding = new Binding("ProjectName");
                projectPicker.SetBinding(Picker.IsEnabledProperty, new Binding("IsEditable", source: viewCell.BindingContext));

                // Add the controls to the gridcontent
                //gridContent.Add(projectEntry,5,0); // Add Entry to column 5
                gridContent.Add(projectPicker,5,0); // Add Picker to the same spot (replaces Entry when editable)
            }
        }
    }

    private void Switch_Toggled(object sender, ToggledEventArgs e)
    {
        var viewModel = BindingContext as MainPageViewModels;
        if (viewModel != null && viewModel.projectvm.ontogglechangedCommand != null)
        {
            if (viewModel.projectvm.ontogglechangedCommand.CanExecute(null))
            {
                viewModel.projectvm.ontogglechangedCommand.Execute(null);
            }
        }
    }

}
