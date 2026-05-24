namespace MyDreamTimeTableApp;

public class Timetable
{
    private Dictionary<DayOfWeek, List<Subject>> _daySchedule = new Dictionary<DayOfWeek, List<Subject>>();
    
    public List<Subject> GetDaySchedule(DayOfWeek day)
    {
        return _daySchedule[day];
    }
    public void SetDay(DayOfWeek day, List<Subject> subjects)
    {
        _daySchedule[day] = subjects;
    }
}