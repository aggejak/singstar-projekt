public class TargetNote
{
    public float startTime;
    public float endTime;
    public float targetMidi;

    public TargetNote(float startTime, float endTime, float targetMidi)
    {
        this.startTime = startTime;
        this.endTime = endTime;
        this.targetMidi = targetMidi;
    }
}

public class TargetNoteGroup
{
    public readonly TargetNote[] Notes;

    public float StartTime => Notes[0].startTime;
    public float EndTime => Notes[Notes.Length - 1].endTime;

    public float DisplayStartTime { get; }
    public float DisplayEndTime { get; }

    // Standard: samma visningstider som tonerna.
    public TargetNoteGroup(params TargetNote[] notes)
    {
        Notes = notes;
        DisplayStartTime = StartTime;
        DisplayEndTime = EndTime;
    }

    // Valfri tidigare visningsstart.
    public TargetNoteGroup(float displayStartTime, params TargetNote[] notes)
    {
        Notes = notes;
        DisplayStartTime = System.Math.Min(displayStartTime, StartTime);
        DisplayEndTime = EndTime;
    }

    // Egen start och eget slut för fönstret.
    public TargetNoteGroup(
        float displayStartTime,
        float displayEndTime,
        params TargetNote[] notes)
    {
        Notes = notes;
        DisplayStartTime = System.Math.Min(displayStartTime, StartTime);
        DisplayEndTime = System.Math.Max(displayEndTime, EndTime);
    }
}

public static class SongNotes
{
    // MIDI-tonnummer
    private const float G3 = 55f;
    private const float A3 = 57f;
    private const float B3 = 59f;
    private const float C4 = 60f;
    private const float D4 = 62f;
    private const float E4 = 64f;
    private const float Fs4 = 66f;
    private const float G4 = 67f;
    private const float B4 = 71f;

    // Tider i sekunder från ljudfilens början.
    // Varje grupp visas ensam från sin första tons start till sin sista tons slut.
    public static readonly TargetNoteGroup[] Groups =
    {
        // Första refrängen
        new TargetNoteGroup(
            11.00f, //fönster start
            18.00f,
            new TargetNote(12.24f, 13.78f, B3),  // Stad
            new TargetNote(13.88f, 15.46f, C4),  // i
            new TargetNote(15.56f, 17.20f, D4)   // ljus
        ),
        new TargetNoteGroup(
            17.20f,
            new TargetNote(18.00f, 18.34f, E4),  // i
            new TargetNote(18.44f, 18.90f, Fs4), // ett
            new TargetNote(19.00f, 20.57f, G4),  // land
            new TargetNote(20.67f, 21.40f, C4),  // u-
            new TargetNote(21.50f, 22.23f, B3),  // -tan
            new TargetNote(22.33f, 24.00f, A3)   // namn
        ),
        // Paus 24.83–25.67
        new TargetNoteGroup(
            24.00f,
            new TargetNote(25.67f, 27.23f, B3),  // Ge
            new TargetNote(27.33f, 28.90f, Fs4), // mig
            new TargetNote(29.00f, 30.50f, G4)   // liv
        ),
        new TargetNoteGroup(
            30.50f,
            new TargetNote(31.08f, 31.40f, G4),  // där
            new TargetNote(31.50f, 31.82f, E4),  // all-
            new TargetNote(31.92f, 32.23f, C4),  // -ting
            new TargetNote(32.33f, 33.90f, B3),  // föds
            new TargetNote(34.00f, 35.57f, A3),  // på
            new TargetNote(35.67f, 37.26f, G3)   // nytt
        ),
        // Andra refrängen
        new TargetNoteGroup(
            37.26f,
            new TargetNote(39.00f, 40.58f, B3),  // Stad
            new TargetNote(40.68f, 42.26f, C4),  // i
            new TargetNote(42.36f, 44.10f, D4)   // ljus
        ),
        new TargetNoteGroup(
            44.2f,
            new TargetNote(44.88f, 45.20f, E4),   // i
            new TargetNote(45.30f, 45.62f, Fs4), // ett
            new TargetNote(45.72f, 47.30f, G4),  // land
            new TargetNote(47.40f, 48.14f, C4),  // u-
            new TargetNote(48.24f, 48.98f, B3),  // -tan
            new TargetNote(49.08f, 50.62f, A3)   // namn
        ),
        // Paus 51.60–52.44
        new TargetNoteGroup(
            50.62f,
            new TargetNote(52.44f, 53.18f, B3),  // Ge
            new TargetNote(53.68f, 54.72f, B4),  // GEEE
            new TargetNote(54.92f, 55.70f, Fs4), // mig
            new TargetNote(55.88f, 57.25f, G4)   // liv
        ),
        new TargetNoteGroup(
            57.25f,
            new TargetNote(57.90f, 58.22f, G4),  // där
            new TargetNote(58.32f, 58.64f, E4),  // all-
            new TargetNote(58.74f, 59.02f, C4),  // -ting
            new TargetNote(59.12f, 59.90f, D4),  // föds
            new TargetNote(60.84f, 62.42f, Fs4), // på
            new TargetNote(62.52f, 67.28f, G4)   // nytt
        )
    };

    // Samma noter används av poängräkningen; tider och toner definieras bara ovan.
    public static readonly TargetNote[] Notes = FlattenGroups();

    private static TargetNote[] FlattenGroups()
    {
        var notes = new System.Collections.Generic.List<TargetNote>();
        foreach (TargetNoteGroup group in Groups)
        {
            notes.AddRange(group.Notes);
        }
        return notes.ToArray();
    }

    public static TargetNoteGroup GetCurrentGroup(float songTime)
    {
        foreach (TargetNoteGroup group in Groups)
        {
            if (songTime >= group.StartTime && songTime < group.EndTime)
            {
                return group;
            }
        }
        return null;
    }

    public static TargetNote GetCurrentNote(float songTime)
    {
        foreach (TargetNote note in Notes)
        {
            if (songTime >= note.startTime && songTime < note.endTime)
            {
                return note;
            }
        }
        return null;
    }
    public static TargetNoteGroup GetDisplayGroup(float songTime)
    {
        // Klipp aldrig bort en grupp som fortfarande sjungs.
        TargetNoteGroup activeGroup = GetCurrentGroup(songTime);

        if (activeGroup != null)
            return activeGroup;

        // Om visningstider överlappar prioriteras den senare gruppen.
        for (int i = Groups.Length - 1; i >= 0; i--)
        {
            TargetNoteGroup group = Groups[i];

            if (songTime >= group.DisplayStartTime &&
                songTime < group.DisplayEndTime)
            {
                return group;
            }
        }

        return null;
    }
}
