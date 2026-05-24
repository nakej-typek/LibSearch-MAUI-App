namespace MyDreamTimeTableApp
{
	
class Program
{
	static void Main()
	{
		var timetable = new Timetable();
		SeminarGroup[] pb111Sem =
		[
			new SeminarGroup(69, new List<string> {"Jožoslava Sabovská", "Vlado Diamantík"} )
		];
		SeminarGroup[] pb178Sem =
		[
			new SeminarGroup(67, new List<string> { "Nailil Abyxová" })
		];
		
		var pb178 = new Subject("PB178", "Gigachad Subject", 
			new TimeOnly(18,00), new TimeOnly(19, 50),
			pb178Sem, "Martin Macák (chápete, jakože jsem mu ve jméně prohodil první písmena)" );
		
		var pb111 = new Subject("PB111", "Hell on Earth",
			TimeOnly.MinValue, TimeOnly.MaxValue, pb111Sem, "Adolf Stalin");
		
		var tuesdaySubjects = new List<Subject> { pb178 };
		var thursdaySubjects = new List<Subject> { pb111 };
		timetable.SetDay(DayOfWeek.Tuesday, tuesdaySubjects);
		timetable.SetDay(DayOfWeek.Thursday, thursdaySubjects);

		//Console.WriteLine(timetable.GetDaySchedule(DayOfWeek.Monday));
		// Console.WriteLine(timetable.GetDaySchedule(DayOfWeek.Tuesday)[0].SubjectCode);
		// Console.WriteLine(timetable.GetDaySchedule(DayOfWeek.Tuesday)[0].SeminarGroups[0].Tutors[0]);
		//
		// Console.WriteLine(timetable.GetDaySchedule(DayOfWeek.Thursday)[0].SubjectCode);
		// Console.WriteLine(timetable.GetDaySchedule(DayOfWeek.Thursday)[0].SeminarGroups[0].Tutors[0]);
		
	}
}
}
