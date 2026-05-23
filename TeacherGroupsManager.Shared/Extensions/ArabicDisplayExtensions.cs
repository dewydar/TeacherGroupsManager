using TeacherGroupsManager.Core.Enums;

namespace TeacherGroupsManager.Shared.Extensions;

public static class ArabicDisplayExtensions
{
    public static string ToArabic(this GroupType value) => value == GroupType.Private ? "درس خاص" : "مجموعة عامة";
    public static string ToArabic(this LessonType value) => value == LessonType.Private ? "درس خاص" : "درس للمجموعة بالكامل";
    public static string ToArabic(this PaymentStatus value) => value switch
    {
        PaymentStatus.Paid => "مدفوع",
        PaymentStatus.PartiallyPaid => "مدفوع جزئي",
        _ => "غير مدفوع"
    };

    public static string DayToArabic(this DayOfWeek value) => value switch
    {
        DayOfWeek.Saturday => "السبت",
        DayOfWeek.Sunday => "الأحد",
        DayOfWeek.Monday => "الإثنين",
        DayOfWeek.Tuesday => "الثلاثاء",
        DayOfWeek.Wednesday => "الأربعاء",
        DayOfWeek.Thursday => "الخميس",
        _ => "الجمعة"
    };
}
