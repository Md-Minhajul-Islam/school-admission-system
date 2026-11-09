namespace SchoolAdmission.Application.Common;

public static class Ids
{
    public static string NewClassId() => "c_" + Guid.NewGuid().ToString("N");

    public static string NewHoldId() => "h_" + Guid.NewGuid().ToString("N");
}
