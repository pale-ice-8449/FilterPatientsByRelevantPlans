using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using VMS.TPS.Common.Model.API;
using ConsoleX;
using FilterPatientsByRelevantPlans.Models;

// TODO: Replace the following version attributes by creating AssemblyInfo.cs. You can do this in the properties of the Visual Studio project.
[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyFileVersion("1.0.0.1")]
[assembly: AssemblyInformationalVersion("1.0")]

// TODO: Uncomment the following line if the script requires write access.
// [assembly: ESAPIScript(IsWriteable = true)]

namespace FilterPatientsByRelevantPlans
{
  class Program
  {
    private static DateTime _queryStartDate;
    private static DateTime _queryEndDate;

    private static List<string> _patientsNotAbleToOpen;
    private static List<RelevantPatientDataModel> _relavantPatientsInDateRange;

    [STAThread]
    static void Main(string[] args)
    {
      try
      {
        using (Application app = Application.CreateApplication())
        {
          Execute(app);
        }
      }
      catch (Exception e)
      {
        Console.Error.WriteLine(e.ToString());
      }
    }
    static void Execute(Application app)
    {
      // using ConsoleX from Rex Cardan in NuGet Package Manager...not required...just can make things simpler
      // when writing to the console
      ConsoleUI ui = new ConsoleUI();

      // exit option variable - app runs
      bool getMeOutOfHere = false;

      // get the desired time window to search from the user
      GetTimeRange(ui);

      // patient ids in the database
      IEnumerable<string> patientIds = app.PatientSummaries.Select(x => x.Id);

      // console application 
      while (!getMeOutOfHere)
      {
        // options for console app
        var options = new Dictionary<string, Action>();

        // add options w/ corresponding functions/tasks
        options.Add("Get Relevant Plan Information", () => GetRelevantPlanInformation(app, ui, patientIds, _queryStartDate, _queryEndDate));
        options.Add("View Patients That Could Not Be Opened", () => ShowUnopenablePatients(ui, _patientsNotAbleToOpen));
        // can add more options
        options.Add("Save Relevant Data To CSV", () => SaveToCSV(ui, _relavantPatientsInDateRange));
        options.Add("Show Cool And Meaningful Stats", () => ShowCoolAndMeaningfulStats(ui, _relavantPatientsInDateRange));

        // exit option
        options.Add("Exit", () => { app.Dispose(); getMeOutOfHere = true; });

        ui.GetResponseAndDoAction(options);
        ui.SkipLines(2);
      }
    }

    private static void ShowCoolAndMeaningfulStats(ConsoleUI ui, List<RelevantPatientDataModel> relavantPatientsInDateRange)
    {
      // gain insights into data...
    }

    private static void SaveToCSV(ConsoleUI ui, List<RelevantPatientDataModel> relavantPatientsInDateRange)
    {
      if (_relavantPatientsInDateRange.Count > 0)
      {
        // save your data....or do whatever
        try
        {

        }
        catch (Exception)
        {
          ui.SkipLines(2);
          ui.WriteError("Something went wrong while trying to save your data...");
          ui.SkipLines(2);
        }
      }
      else
      {
        ui.SkipLines(2);
        ui.Write("Sorry, there was not any relevant data to save");
        ui.SkipLines(2);
      }
    }

    private static void ShowUnopenablePatients(ConsoleUI ui, List<string> patientsNotAbleToOpen)
    {
      ui.Write("-------------------------------\n");
      ui.Write($"{patientsNotAbleToOpen.Count} patients could not be opened...");
      ui.SkipLines(2);
      foreach (var pid in _patientsNotAbleToOpen)
      {
        ui.Write(pid);
      }
      ui.SkipLines(2);
      ui.Write("-------------------------------");
      ui.SkipLines(2);
    }

    private static void GetRelevantPlanInformation(Application app, ConsoleUI ui, IEnumerable<string> patientIds, DateTime queryStartDate, DateTime queryEndDate)
    {
      // using the application and list of patient ids
      // you want to...
      // open patient by id
      // add to list if meets criteria

      // initiate new list of patients that couldn't be opened
      _patientsNotAbleToOpen = new List<string>();

      // open/closed state of app/patient
      bool patientIsOpen = false;

      // loop through the patient ids pulled from te summaries
      foreach (var pid in patientIds)
      {
        try
        {
          // may not be necessary but may help in case something happened before patient could be closed... not tested
          if (patientIsOpen)
          {
            app.ClosePatient();
          }

          // open
          Patient patient = app.OpenPatientById(pid);

          // patient is now open
          patientIsOpen = true;

          // empty list for relevant plans
          List<PlanSetup> relavantPlans = new List<PlanSetup>();

          // foreach course
          foreach (var course in patient.Courses)
          {
            // foreach plan in course
            foreach (var psetup in course.PlanSetups)
            {
              // if the plan has been treated and was created within the relevant date window
              if (psetup.IsTreated && psetup.CreationDateTime >= _queryStartDate && psetup.CreationDateTime <= _queryEndDate)
              {
                // add the plan to the patient's list of relevant plans
                relavantPlans.Add(psetup);
              }
            }

          }

          // if the patient has relevant plans
          if (relavantPlans.Count > 0)
          {
            // create and add a new relevant patient data model (this is not necessary...do what you need/want
            _relavantPatientsInDateRange.Add(
                  new RelevantPatientDataModel
                  {
                    Patient = patient,
                    RelevantPlans = relavantPlans
                  });

            // this is the data model made above -> can make it whatever you want and can be useful for manipulation later
            //public class RelevantPatientDataModel
            //{
            //  public Patient Patient { get; set; }
            //  public List<PlanSetup> RelevantPlans { get; set; }
            //}

          }

          // close patient
          app.ClosePatient();
          // patient is closed
          patientIsOpen = false;
        }
        catch (Exception)
        {
          _patientsNotAbleToOpen.Add(pid);
        }
      }
    }

    /// <summary>
    /// Gets the desired/relevant date range
    /// </summary>
    /// <param name="ui"></param>
    private static void GetTimeRange(ConsoleUI ui)
    {
      ui.WriteSectionHeader("----Date Range----", ConsoleColor.Gray);
      // get start date
      GetDateStringInput(ui, "start", "\n----Desired Query Start Date----", "Please provide a valid query start date string\ne.g., mm/dd or m/d or m/d/yy or mm/dd/yyyy");
      // get end date
      GetDateStringInput(ui, "end", "\n----Desired Query End Date----", "Please provide a valid query end date string\ne.g., mm/dd or m/d or m/d/yy or mm/dd/yyyy");
    }

    /// <summary>
    /// Gets the date string input from the user
    /// </summary>
    /// <param name="ui"></param>
    /// <param name="startOrEndDate"></param>
    /// <param name="prompt"></param>
    /// <param name="errorPrompt"></param>
    private static void GetDateStringInput(ConsoleUI ui, string startOrEndDate, string prompt, string errorPrompt)
    {
      try
      {
        if (startOrEndDate.ToUpper() == "START")
        {
          _queryStartDate = DateTime.Parse(ui.GetStringInput(prompt));
        }
        else if (startOrEndDate.ToUpper() == "END")
        {
          _queryEndDate = DateTime.Parse(ui.GetStringInput(prompt)).AddDays(1); // adding a day in case the time is 12am -> can test/verify
        }

      }
      catch (Exception)
      {
        ui.WriteError(errorPrompt);
        GetDateStringInput(ui, startOrEndDate, prompt, errorPrompt);
      }
    }

  }
}
