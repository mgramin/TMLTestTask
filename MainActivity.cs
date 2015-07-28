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

  public static class Registry {
    public static DateTime SelectedDay {
      get;
      set;
    }
  }

  [Activity (Label = "TMLTestTask", MainLauncher = true, Icon = "@drawable/icon")]
  public class MainActivity : Activity
  {

    // RecyclerView instance that displays the photo album:
    RecyclerView mRecyclerView;
    // Layout manager that lays out each card in the RecyclerView:
    RecyclerView.LayoutManager mLayoutManager;
    CalendarAdapter mAdapter;
    // Photo album that is managed by the adapter:
    Calendar calendar;

    ListView listView;


    protected override void OnCreate (Bundle bundle)
    {
      base.OnCreate (bundle);
      SetContentView (Resource.Layout.Main);

      Registry.SelectedDay = DateTime.Today;

      calendar = new Calendar ();

      mLayoutManager = new LinearLayoutManager (this, LinearLayoutManager.Horizontal, false);

      mAdapter = new CalendarAdapter (calendar);
      mAdapter.ItemClick += OnItemClick;

      mRecyclerView = FindViewById<RecyclerView> (Resource.Id.recyclerView);
      mRecyclerView.SetLayoutManager (mLayoutManager);
      mRecyclerView.SetAdapter (mAdapter);

      InfiniteScrollListener _infiniteScrollListener = new InfiniteScrollListener(this.UpdateDataAdapter);
      mRecyclerView.SetOnScrollListener (_infiniteScrollListener);


      Button button = FindViewById<Button> (Resource.Id.myButton);

      button.Click += delegate {
        button.Text = string.Format ("clicks!");
      };

      CheckedTextView checkView = FindViewById<CheckedTextView> (Resource.Id.checkedField);
      /*checkView.Click += delegate {
        if (checkView.Checked)
          checkView.Checked = false;
        else
          checkView.Checked = true;
      };*/

      listView = FindViewById<ListView> (Resource.Id.scheduleListView);
      CustomListAdapter customListAdapter = new CustomListAdapter(this, calendar.days [0].Schedules);
      listView.Adapter = customListAdapter;
      //listView.ItemClick += OnListItemClick;

      //CheckedTextView checkedTextView = FindViewById<CheckedTextView> (Resource.Id.checkedField);
      //checkedTextView.Click += OnListItemClick;



    }

    void OnListItemClick(object sender, EventArgs e) {
    }

    private void UpdateDataAdapter()
    {
      int count = calendar.NumDays();
      Toast.MakeText(this, string.Format("{0} items", count), ToastLength.Short).Show();
      if (count > 0)
      {
        mAdapter.NotifyDataSetChanged();
      }
    }

    // Handler for the item click event:
    void OnItemClick (object sender, int position)
    {
      RecyclerView.Adapter adapter = sender as RecyclerView.Adapter;
      Registry.SelectedDay = DateTime.Today.AddDays(position);
      adapter.NotifyDataSetChanged ();
      listView.Adapter = new CustomListAdapter(this, calendar.days [position].Schedules);
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


  // Adapter to connect the data set (photo album) to the RecyclerView:
  public class CalendarAdapter : RecyclerView.Adapter
  {
    public event EventHandler<int> ItemClick;

    private Calendar calendar;

    public CalendarAdapter (Calendar calendar)
    {
      this.calendar = calendar;
    }

    // Create a new photo CardView (invoked by the layout manager):
    public override RecyclerView.ViewHolder OnCreateViewHolder (ViewGroup parent, int viewType)
    {
      View itemView = LayoutInflater.From (parent.Context).Inflate (Resource.Layout.CalendarDayCardView, parent, false);
      return new CalendarViewHolder (itemView, OnClick);
    }

    // Fill in the contents of the photo card (invoked by the layout manager):
    public override void OnBindViewHolder (RecyclerView.ViewHolder holder, int position)
    {
      CalendarViewHolder calendarViewHolder = holder as CalendarViewHolder;

      calendarViewHolder.MonthDay.Text = calendar [position].Day.Day.ToString ();
      calendarViewHolder.WeekDay.Text = calendar [position].DayOfWeekShort;

      if (Registry.SelectedDay.ToShortDateString () == DateTime.Now.AddDays (position).ToShortDateString ()) {
        calendarViewHolder.MonthDay.SetTextColor (Android.Graphics.Color.Indigo);
        calendarViewHolder.WeekDay.SetTextColor (Android.Graphics.Color.Indigo);
        calendarViewHolder.MonthDay.SetTextSize (Android.Util.ComplexUnitType.Dip, 22);
      } else if (!calendar [position].IsWorkDay) {
        calendarViewHolder.MonthDay.SetTextSize (Android.Util.ComplexUnitType.Dip, 16);
        calendarViewHolder.MonthDay.SetTextColor (Android.Graphics.Color.Red);
        calendarViewHolder.WeekDay.SetTextColor (Android.Graphics.Color.Red);
      }
      else {
        calendarViewHolder.MonthDay.SetTextSize (Android.Util.ComplexUnitType.Dip, 16);
        calendarViewHolder.MonthDay.SetTextColor (Android.Graphics.Color.Black);
        calendarViewHolder.WeekDay.SetTextColor (Android.Graphics.Color.Black);
      }
    }

    // Return the number of photos available in the photo album:
    public override int ItemCount
    {
      get { return calendar.NumDays(); }
    }

    void OnClick (int position)
    {
      if (ItemClick != null) {
        ItemClick (this, position);
      }
    }
  }


  public class InfiniteScrollListener : RecyclerView.OnScrollListener
  {
    private readonly Action moreItemsLoadedCallback;

    public InfiniteScrollListener(Action moreItemsLoadedCallback)
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


  public class CustomListAdapter : BaseAdapter<Schedule>
  {
  	Activity context;
    List<Schedule> list;

    public CustomListAdapter (Activity _context, List<Schedule> _list)
  		:base()
  	{
  		this.context = _context;
  		this.list = _list;
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
      //checkedTextView.Checked = true;
      TextView scheduleTitle = view.FindViewById<TextView> (Resource.Id.ScheduleTitle);
      scheduleTitle.Text = schedule.Title;
      Console.WriteLine (schedule.IsFree);
      if (!schedule.IsFree) {
        scheduleTitle.SetTextColor (Android.Graphics.Color.Gray);
        scheduleTime.SetTextColor (Android.Graphics.Color.Gray);
      } else {
        scheduleTitle.SetTextColor (Android.Graphics.Color.White);
        scheduleTime.SetTextColor (Android.Graphics.Color.White);
      }


  		return view;
  	}

  }


}