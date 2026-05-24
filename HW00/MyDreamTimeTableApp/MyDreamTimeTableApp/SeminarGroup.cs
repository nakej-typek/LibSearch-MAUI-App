namespace MyDreamTimeTableApp;

public class SeminarGroup
{
    private int _groupnumber;
    private List<string> _tutors;

    public int Groupnumber => _groupnumber;

    public List<string> Tutors => _tutors;

    public SeminarGroup(int groupnumber, List<string> tutors)
    {
        _groupnumber = groupnumber;
        _tutors = tutors;
    }
}