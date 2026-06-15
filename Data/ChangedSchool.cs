using mapa_back.Models.DTO;

namespace mapa_back.Models
{
	public class ChangedSchool
	{
		public School SchoolBeforeChanges { get; set; }
		public School SchoolsAfterChanges { get; set; }

		public ChangedSchool(School schoolBeforeChanges, School schoolAfterChanges)
		{
			this.SchoolBeforeChanges = schoolBeforeChanges;
			this.SchoolsAfterChanges = schoolAfterChanges;
		}
	}
}
