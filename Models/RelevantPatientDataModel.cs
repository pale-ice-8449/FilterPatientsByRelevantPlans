using System.Collections.Generic;
using VMS.TPS.Common.Model.API;

namespace FilterPatientsByRelevantPlans.Models
{
  public class RelevantPatientDataModel
  {
    public Patient Patient { get; set; }
    public List<PlanSetup> RelevantPlans { get; set; }
  }
}
