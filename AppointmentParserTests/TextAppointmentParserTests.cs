using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppointmentParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentParser.Tests
{
    [TestClass()]
    public class TextAppointmentParserTests
    {
        TextAppointmentParser parser = null;
        DateTime now = DateTime.Parse("1991/5/15 6:15:00");

        public void Validate(string text, string expectStart, string expectEnd, bool expectAllDay)
        {
            var app = parser.Parse(text);
            Assert.AreEqual(DateTime.Parse(expectStart), app.Start);
            Assert.AreEqual(DateTime.Parse(expectEnd), app.End);
            Assert.AreEqual(expectAllDay, app.AllDay);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            parser =  new TextAppointmentParser();
            Time.SetNowGetter(()=> { return now; });
            Date.SetNowGetter(()=> { return now; });
        }

        [TestMethod()]
        public void TextAppointmentParserTest_DateTime_()
        {
            Validate("2000/1/2 3:4",
                    "2000/1/2 3:4",
                    "2000/1/2 17:00",
                    false);
        }
        [TestMethod()]
        public void TextAppointmentParserTest_DateTime_DateTime()
        {
            Validate("2000/1/2 3:4 - 2000/11/22 4:5",
                    "2000/1/2 3:4",
                    "2000/11/22 4:5",
                    false);
        }

        [TestMethod()]
        public void TextAppointmentParserTest_Date()
        {
            Validate("2000/1/2",
                    "2000/1/2 8:30",
                    "2000/1/2 17:00",
                    true);
        }
        [TestMethod()]
        public void TextAppointmentParserTest_Date_Date()
        {
            Validate("2000/1/2 2/1",
                    "2000/1/2 8:30",
                    "2000/2/1 17:00",
                    true);
        }
        [TestMethod()]
        public void TextAppointmentParserTest_Time()
        {
            Validate("1:00",
                    "1991/5/15 1:00",
                    "1991/5/15 17:00",
                    false);
        }
        [TestMethod()]
        public void TextAppointmentParserTest_Time_OverDefaultEndTime()
        {
            Validate("18:00",
                    "1991/5/15 18:00",
                    "1991/5/15 18:00",
                    false);
        }
        [TestMethod()]
        public void TextAppointmentParserTest_Time_Time()
        {
            Validate("1:00 - 2:00",
                    "1991/5/15 1:00",
                    "1991/5/15 2:00",
                    false);
        }
        [TestMethod()]
        public void TextAppointmentParserTest_DateTime_Time()
        {
            Validate("2000/1/2 1:00 - 2:00",
                    "2000/1/2 1:00",
                    "2000/1/2 2:00",
                    false);
        }

        [TestMethod()]
        public void TextAppointmentParserTest_Date_DateTime()
        {
            Validate("2000/1/2 - 2000/1/3 1:00",
                    "2000/1/2 8:30",
                    "2000/1/3 1:00",
                    false);
        }
        [TestMethod()]
        public void TextAppointmentParserTest_Date_DateTime_StartTImeEarlierThanEndTime()
        {
            Validate("2000/1/2 - 2000/1/2 1:00",
                    "2000/1/2 8:30",
                    "2000/1/2 8:30",
                    false);
        }

        [TestMethod()]
        public void TextAppointmentParserTest_Time_DateTime()
        {
            Validate("1:00 - 2000/1/2 2:00",
                    "1991/5/15 1:00",
                    "2000/1/2 2:00",
                    false);
        }
        [TestMethod()]
        public void TextAppointmentParserTest_Time_Date()
        {
            Validate("1:00 - 2000/1/2",
                    "1991/5/15 1:00",
                    "2000/1/2 17:00",
                    false);
        }

        [TestMethod()]
        public void TextAppointmentParserTest_CompletionDateYear()
        {
            Validate("1/1",
                    "1992/1/1 8:30",
                    "1992/1/1 17:00",
                    true);
        }
        [TestMethod()]
        public void TextAppointmentParserTest_CompletionDateYearMonth()
        {
            Validate("1日",
                    "1991/6/1 8:30",
                    "1991/6/1 17:00",
                    true);
        }
        [TestMethod()]
        public void TextAppointmentParserTest_CompletionDateIncYear()
        {
            Date.SetNowGetter(() => { return DateTime.Parse("2000/12/2"); });
            Validate("1日",
                    "2001/1/1 8:30",
                    "2001/1/1 17:00",
                    true);
        }
        [TestMethod()]
        public void TextAppointmentParserTest_EndEarlierThanStart()
        {
            try
            {
                Validate("2000/1/2 2:00 - 2000/1/2 1:00",
                        "",
                        "",
                        false);
            }catch(ArgumentException ex)
            {
                return;
            }
            Assert.Fail();
        }

        [TestMethod()]
        public void TextAppointmentParserTest_OutOfRangeDateTime()
        {
            Validate("2000/13/2",
                    "1991/5/15 8:30",
                    "1991/5/15 17:00",
                    true);
        }
        [TestMethod()]
        public void TextAppointmentParserTest_InvalidDateTime()
        {
            try
            {
                Validate("2001/2/29",
                        "",
                        "",
                        false);
            }catch(ArgumentException ex)
            {
                return;
            }
            Assert.Fail();
        }

        [TestMethod()]
        public void TextAppointmentParserTest_ContainSpace()
        {
            Validate("２０００ 年 １ 月 ２ 日 ３ 時 ４分",
                    "2000/1/2 3:4",
                    "2000/1/2 17:00",
                    false);
        }
        [TestMethod()]
        public void TextAppointmentParserTest_ContainOnlySpaceBetweenNumber()
        {
            Validate("2000 / 1 / 2 3 / 4",
                    "2000/1/2 8:30",
                    "2000/3/4 17:00",
                    true);
        }
    }
}