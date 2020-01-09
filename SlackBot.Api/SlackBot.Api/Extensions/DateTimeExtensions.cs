using System;
using System.Linq;

namespace SlackBot.Api.Extensions
{
    public static class DateTimeExtensions
    {
        public static readonly string Holidays = @"
2019/12/30,
2019/12/31,
2020/1/1,元日
2020/1/2,
2020/1/3,
2020/1/13,成人の日
2020/2/11,建国記念の日
2020/2/23,天皇誕生日
2020/2/24,休日
2020/3/20,春分の日
2020/4/29,昭和の日
2020/5/3,憲法記念日
2020/5/4,みどりの日
2020/5/5,こどもの日
2020/5/6,休日
2020/7/23,海の日
2020/7/24,スポーツの日
2020/8/10,山の日
2020/9/21,敬老の日
2020/9/22,秋分の日
2020/11/3,文化の日
2020/11/23,勤労感謝の日";

        public static bool IsHoliday(this DateTime dateTime)
        {
            return Holidays.Split("\n")
                .Where(x => x.Contains(","))
                .Select(x => x.Split(",").First())
                .Any(x =>
                {
                    if (DateTime.TryParseExact(x, "yyyy/MM/dd", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out var d))
                    {
                        return d == dateTime;
                    }

                    return false;
                });
        }
    }
}
