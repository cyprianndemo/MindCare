using MindCare.Models;

namespace MindCare.ViewModel
{
    public class ResourceReadViewModel
    {
        public MentalHealthResource Resource { get; set; }
        public IEnumerable<MentalHealthResource> RelatedResources { get; set; }
        public IEnumerable<MentalHealthExercise> RelatedExercises { get; set; }
        public IEnumerable<ProfessionalResource> ProfessionalResources { get; set; }
    }
}
