namespace MyDreamTimeTableApp;

public class Subject
{
    private string _subjectCode;
    private string _subjectName;
    private string _teacher;
    private TimeOnly _startTime;
    private TimeOnly _endTime;
    private TimeSpan _duration;
    private SeminarGroup[] _seminarGroups;

    public string SubjectCode => _subjectCode;

    public string SubjectName => _subjectName;

    public string Teacher => _teacher;

    public TimeOnly StartTime => _startTime;

    public TimeOnly EndTime => _endTime;

    public TimeSpan Duration => _duration;

    public SeminarGroup[] SeminarGroups => _seminarGroups;
    
    public Subject(string subjectCode, string subjectName, 
        TimeOnly startTime, TimeOnly endTime, SeminarGroup[] seminarGroups, string teacher)
    {
        this._subjectCode = subjectCode;
        this._subjectName = subjectName;
        this._startTime = startTime;
        this._endTime = endTime;
        this._duration = endTime - startTime;
        this._seminarGroups = seminarGroups;
        this._teacher = teacher;
    }
    
    
}
