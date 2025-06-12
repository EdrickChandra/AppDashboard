using Surveying.ViewModels;
using Windows.Devices.Sensors;

namespace Surveying.ViewModels
{
    /// <summary>
    /// ACTIVITY TEMPLATE SELECTOR
    /// Dynamically selects the appropriate DataTemplate based on ActivityType
    /// Enables single page to handle all activity types with different UI layouts
    /// </summary>
    public class ActivityTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CleaningTemplate { get; set; }
        public DataTemplate RepairTemplate { get; set; }
        public DataTemplate PeriodicTemplate { get; set; }
        public DataTemplate SurveyTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is ContainerActivityViewModel viewModel)
            {
                return viewModel.ActivityType switch
                {
                    ActivityType.Cleaning => CleaningTemplate,
                    ActivityType.Repair => RepairTemplate,
                    ActivityType.Periodic => PeriodicTemplate,
                    ActivityType.Survey => SurveyTemplate,
                    _ => CleaningTemplate // Default fallback
                };
            }

            return CleaningTemplate; // Default fallback
        }
    }
}