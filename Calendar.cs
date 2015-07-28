using System;
using System.Collections.Generic;
using System.Globalization;

namespace TMLTestTask
{

  public class Calendar
  {
    private int count = 20;

    public List<CalendarDay> days = new List<CalendarDay>();

    public Calendar () {

      CultureInfo culture = new CultureInfo("ru-RU");
      string[] names = culture.DateTimeFormat.AbbreviatedDayNames;


      Random random = new Random ();
      DateTime currentDay = DateTime.Today;

      for (int i = 0; i < 30; i++) {
        currentDay = currentDay.AddDays (1);
        List<Schedule> schedule = new List<Schedule> ();
        for (int j = 0; j < 4; j++) {
          schedule.Add (new Schedule(){ 
            Title = Schedule.ScheduleTypes[random.Next(Schedule.ScheduleTypes.Count)], 
            IsFree = random.NextDouble() >= 0.3, 
            Time = currentDay.AddHours(7).AddMinutes(random.Next(480)).TimeOfDay 
          });
        }
        days.Add (new CalendarDay() { 
          Day = currentDay, 
          IsWorkDay = (currentDay.DayOfWeek != DayOfWeek.Sunday && currentDay.DayOfWeek != DayOfWeek.Saturday), 
          DayOfWeekShort = names[(int) currentDay.DayOfWeek],
          Schedules = schedule 
        });
      }
    }
          

    public CalendarDay this [int i] {
      get { return days[i]; }
    }

    public int NumDays() {
      return count;
    }
  }


  public class CalendarDay
  {
    public DateTime Day { get; set; }
    public bool IsWorkDay { get; set; }
    public string DayOfWeekShort { get; set; }
    public List<Schedule> Schedules { get; set; }
  }


  public class Schedule
  {
    static public List<String> ScheduleTypes {
      get {
        return new List<String>() { "Окулист", "Стоматолог", "Терапевт", "Невролог", "Хирург", "Ортодонт", "ЛОР" };
      }
    }

    public TimeSpan Time { get; set; }
    public String Title { get; set; }
    public bool IsFree { get; set; }
  }


}