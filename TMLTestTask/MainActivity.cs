using System;

using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace TMLTestTask
{

  [Activity (Label = "TMLTestTask", MainLauncher = true, Icon = "@drawable/icon")]
  public class MainActivity : Activity
  {

    Calendar calendar;

    RecyclerView calendarView;
    CalendarAdapter calendarAdapter;
    RecyclerView.LayoutManager calendarLayoutManager;

    ListView scheduleView;
    ScheduleAdapter scheduleAdapter;

    Button acceptButton;

    protected override void OnCreate (Bundle bundle)
    {
      base.OnCreate (bundle);
      SetContentView (Resource.Layout.Main);

      // Дата в календаре по умолчанию - текущая дата
      Registry.SelectedDay = DateTime.Today;

      calendar = new Calendar ();

      calendarLayoutManager = new LinearLayoutManager (this, LinearLayoutManager.Horizontal, false);

      // Настройка календаря
      calendarAdapter = new CalendarAdapter (calendar);
      calendarAdapter.ItemClick += OnCalendarClick;
      calendarView = FindViewById<RecyclerView> (Resource.Id.recyclerView);
      calendarView.SetLayoutManager (calendarLayoutManager);
      calendarView.SetAdapter (calendarAdapter);
      EndlessScrollListener endlessScrollListener = new EndlessScrollListener(this.UpdateCalendarAdapter);
      calendarView.SetOnScrollListener (endlessScrollListener);

      // Настройка списка с расписанием
      CheckedTextView approvedCheckView = FindViewById<CheckedTextView> (Resource.Id.checkedField);
      scheduleView = FindViewById<ListView> (Resource.Id.scheduleListView);
      scheduleAdapter = new ScheduleAdapter(this, calendar[Registry.SelectedDay].Schedules);
      scheduleView.Adapter = scheduleAdapter;
      scheduleView.ItemClick += OnScheduleClick;

      // Кнопка записи на прием
      acceptButton = FindViewById<Button> (Resource.Id.myButton);
      acceptButton.Enabled = false;
    }

    private void UpdateCalendarAdapter()
    {
        calendarAdapter.NotifyDataSetChanged();
    }

    void OnCalendarClick (object sender, int position)
    {
      RecyclerView.Adapter adapter = sender as RecyclerView.Adapter;
      Registry.SelectedDay = DateTime.Today.AddDays(position);
      // Отображаем пустое расписание, если закончилось сгенерированное
      if (calendar [position].Schedules != null) {
        scheduleView.Adapter = new ScheduleAdapter (this, calendar [position].Schedules);
      } else {
        scheduleView.Adapter = new ScheduleAdapter (this, new List<Schedule> ());
      }
      // доступность кнопки в зависимости от выбора периода на день
      acceptButton.Enabled = calendar[position].existApprovedDay();
      adapter.NotifyDataSetChanged ();
    }

    void OnScheduleClick(object sender, AdapterView.ItemClickEventArgs e)
    {
      Schedule schedule = calendar [Registry.SelectedDay].Schedules[e.Position];
      // Если выбранное расписание свободно, то сбрасываем пользовательский выбор (если уже был) и устанавливаем флаг
      if (schedule.IsFree) {
        calendar [Registry.SelectedDay].skipApprovedDay();
        schedule.IsApproved = !schedule.IsApproved;
      } else {
        Toast.MakeText (this, "Интервал занят.", ToastLength.Short).Show ();
      }
      scheduleView.Adapter = new ScheduleAdapter(this, calendar[Registry.SelectedDay].Schedules);
      acceptButton.Enabled = calendar[Registry.SelectedDay].existApprovedDay();
    }

  }


  public class CalendarViewHolder : RecyclerView.ViewHolder
  {
    public TextView MonthDay { get; private set; }
    public TextView WeekDay { get; private set; }

    public CalendarViewHolder (View itemView, Action<int> listener) : base (itemView)
    {
      MonthDay = itemView.FindViewById<TextView> (Resource.Id.monthDayView);
      WeekDay = itemView.FindViewById<TextView> (Resource.Id.weekDayView);
      itemView.Click += (sender, e) => listener (base.Position);
    }
  }


  /// <summary>
  /// Обработчик для контрола календаря
  /// </summary>
  public class CalendarAdapter : RecyclerView.Adapter
  {
    public event EventHandler<int> ItemClick;

    private Calendar calendar;

    public CalendarAdapter (Calendar calendar)
    {
      this.calendar = calendar;
    }

    public override RecyclerView.ViewHolder OnCreateViewHolder (ViewGroup parent, int viewType)
    {
      View itemView = LayoutInflater.From (parent.Context).Inflate (Resource.Layout.CalendarDayCardView, parent, false);
      return new CalendarViewHolder (itemView, OnClick);
    }

    // Определяем способ отрисовки элементов в календаре
    public override void OnBindViewHolder (RecyclerView.ViewHolder holder, int position)
    {
      CalendarViewHolder calendarViewHolder = holder as CalendarViewHolder;

      calendarViewHolder.MonthDay.Text = calendar [position].CurrentDay.Day.ToString ();
      calendarViewHolder.WeekDay.Text = calendar [position].DayOfWeekShort;

      if (Registry.SelectedDay.ToShortDateString () == DateTime.Now.AddDays (position).ToShortDateString ()) {
        calendarViewHolder.MonthDay.SetTextColor (Android.Graphics.Color.DeepSkyBlue);
        calendarViewHolder.WeekDay.SetTextColor (Android.Graphics.Color.DeepSkyBlue);
        calendarViewHolder.MonthDay.SetTextSize (Android.Util.ComplexUnitType.Dip, 22);
      } else if (!calendar [position].IsWorkDay) {
        calendarViewHolder.MonthDay.SetTextSize (Android.Util.ComplexUnitType.Dip, 16);
        calendarViewHolder.MonthDay.SetTextColor (Android.Graphics.Color.Tomato);
        calendarViewHolder.WeekDay.SetTextColor (Android.Graphics.Color.Tomato);
      }
      else {
        calendarViewHolder.MonthDay.SetTextSize (Android.Util.ComplexUnitType.Dip, 16);
        calendarViewHolder.MonthDay.SetTextColor (Android.Graphics.Color.White);
        calendarViewHolder.WeekDay.SetTextColor (Android.Graphics.Color.White);
      }
    }

    public override int ItemCount
    {
      get { return calendar.CountDays(); }
    }

    void OnClick (int position)
    {
      if (ItemClick != null) {
        ItemClick (this, position);
      }
    }
  }


  /// <summary>
  /// Реализация бесконечного скрола по календарю
  /// TODO правда пока только в одну сторону ...
  /// </summary>
  public class EndlessScrollListener : RecyclerView.OnScrollListener
  {
    private readonly Action moreItemsLoadedCallback;

    public EndlessScrollListener(Action moreItemsLoadedCallback)
    {
      this.moreItemsLoadedCallback = moreItemsLoadedCallback;
    }

    public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
    {
      if (moreItemsLoadedCallback != null)
      {
        moreItemsLoadedCallback();
      }
    }
  }


  /// <summary>
  /// Обработчик для контрола расписания
  /// </summary>
  public class ScheduleAdapter : BaseAdapter<Schedule>
  {
  	Activity context;
    List<Schedule> list;

    public ScheduleAdapter (Activity context, List<Schedule> list) :base()
  	{
  		this.context = context;
  		this.list = list;
  	}

  	public override int Count {
  		get { return list.Count; }
  	}

  	public override long GetItemId (int position)
  	{
  		return position;
  	}

    public override Schedule this[int index] {
  		get { return list [index]; }
  	}

    // Определяем способ отрисовки элементов в расписании
    public override View GetView (int position, View convertView, ViewGroup parent)
  	{
  		View view = convertView;
      if (view == null) {
        view = context.LayoutInflater.Inflate (Resource.Layout.ScheduleViewRow, parent, false);
      }
      Schedule schedule = this [position];

      TextView scheduleTime = view.FindViewById<TextView>(Resource.Id.ScheduleTime);
      scheduleTime.Text = schedule.Time.ToString (@"hh\:mm");

      CheckedTextView checkedTextView = view.FindViewById<CheckedTextView> (Resource.Id.checkedField);

      TextView scheduleTitle = view.FindViewById<TextView> (Resource.Id.ScheduleTitle);
      scheduleTitle.Text = schedule.Title;
      if (!schedule.IsFree) {
        scheduleTitle.SetTextColor (Android.Graphics.Color.Gray);
        scheduleTime.SetTextColor (Android.Graphics.Color.Gray);
        checkedTextView.Visibility = ViewStates.Visible;
        checkedTextView.Checked = false;
      } else {
        scheduleTitle.SetTextColor (Android.Graphics.Color.White);
        scheduleTime.SetTextColor (Android.Graphics.Color.White);
        checkedTextView.Visibility = ViewStates.Invisible;
      }

      if (schedule.IsApproved) {
        checkedTextView.Visibility = ViewStates.Visible;
        checkedTextView.Checked = true;
      } else {
        checkedTextView.Checked = false;
      }

  		return view;
  	}

  }


  /// <summary>
  /// "Репозиторий"
  /// </summary>
  public static class Registry {
    // Выбранная пользователем дата
    public static DateTime SelectedDay { get; set; }
  }


}