using System;
using System.Collections.Generic;
using System.Globalization;

namespace TMLTestTask
{

  /// <summary>
  /// Календарь
  /// </summary>
  public class Calendar
  {

    private List<CalendarDay> days = new List<CalendarDay>();

    public Calendar () {
      GenerateScheduleRandom ();
    }

    // Получить день по индексу
    public CalendarDay this [int i] {
      get { 
        // Если запрашиваемый элемент находится рядом с концом списка, то добавим еще, чтобы было что прорисовывать
        if (i > days.Count-20) {
          days.Add (new CalendarDay (){ CurrentDay = days [days.Count-1].CurrentDay.AddDays(1) });
        }
        return days[i]; 
      }
    }

    // Получить день по дате
    public CalendarDay this [DateTime dateTime] {
      get { return days.Find(day => day.CurrentDay.Date == dateTime.Date.AddDays(1)); }
    }

    public int CountDays() {
      return days.Count;
    }

    // Генерация случайного расписания
    private void GenerateScheduleRandom(int countDays = 30) {
      Random random = new Random ();
      DateTime currentDay = DateTime.Today;
      for (int i = 0; i < countDays; i++) {
        currentDay = currentDay.AddDays (1);
        DateTime currentTime = currentDay.AddHours (7);
        List<Schedule> schedule = new List<Schedule> ();
        for (int j = 0; j < random.Next(2, 12); j++) {
          currentTime = currentTime.AddMinutes (random.Next (30, 90));
          schedule.Add (new Schedule(){ 
            Title = Schedule.ScheduleTypes[random.Next(Schedule.ScheduleTypes.Count)], 
            IsFree = random.NextDouble() >= 0.4, 
            Time = RoundTime(currentTime).TimeOfDay,
            IsApproved = false
          });
        }
        days.Add (new CalendarDay() { 
          CurrentDay = currentDay, 
          Schedules = schedule 
        });
      }
    }

    // Окугление времени до 15 минут
    private DateTime RoundTime(DateTime dateTime)
    {
      return new DateTime(dateTime.Year, dateTime.Month, 
        dateTime.Day, dateTime.Hour, (dateTime.Minute / 15) * 15, 0);
    }

  }

  /// <summary>
  /// Календарный день 
  /// </summary>
  public class CalendarDay
  {
    // День
    public DateTime CurrentDay { get; set; }
    // Является рабочим
    public bool IsWorkDay { get { return (CurrentDay.DayOfWeek != DayOfWeek.Sunday && CurrentDay.DayOfWeek != DayOfWeek.Saturday); } }
    // День недели
    public string DayOfWeekShort { 
      get { 
        CultureInfo culture = new CultureInfo("ru-RU");
        string[] dayShortNames = culture.DateTimeFormat.AbbreviatedDayNames;
        return dayShortNames[(int) CurrentDay.DayOfWeek]; 
      } 
    }
    // Расписание приема на этот день
    public List<Schedule> Schedules { get; set; }

    // Проверка - выбран ли пользователем хотябы один период из расписания
    public bool existApprovedDay() {
      return Schedules != null ? Schedules.Exists(schedule => schedule.IsApproved == true) : false;
    }

    // Сбрасывает выбор периода
    public void skipApprovedDay() {
      if (Schedules != null)
        Schedules.ForEach(schedule => schedule.IsApproved = false);
    }

  }


  /// <summary>
  /// Расписание приема на день
  /// </summary>
  public class Schedule
  {
    // Время приема
    public TimeSpan Time { get; set; }
    // Название услуги
    public String Title { get; set; }
    // Уже был занят
    public bool IsFree { get; set; }
    // Незанятый период уже выбран пользователем
    public bool IsApproved { get; set; }

    static public List<String> ScheduleTypes {
      get {
        return new List<String>() { "Окулист", "Стоматолог", "Терапевт", "Невролог", "Хирург", "Ортодонт", "ЛОР" };
      }
    }
  
  }


}