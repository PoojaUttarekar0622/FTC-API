using System;
using System.Collections.Generic;
using System.Text;

namespace Helper.Model
{
  public  class ParseStringToDate
    {
        public static string DateFormat = "dd MMM yyyy";

        public static string DateDifference(DateTime? fromDate, DateTime? toDate)
        {
            string Difference = "";
            if (fromDate != null && toDate != null)
            {
                TimeSpan span = (toDate.Value - fromDate.Value);
                if (span.Days > 0)
                {
                    Difference = span.Days + " day/(s)";
                }
                if (span.Hours > 0)
                {
                    Difference = Difference + span.Hours + " hours";
                }
                if (span.Minutes > 0)
                {
                    Difference = Difference + span.Minutes + " minutes";
                }
            }
            return Difference;
        }

       
        public static DateTime ParseStringtoDate(string strdt)
        {
            DateTime SODate = new DateTime();
            string strdate1 = strdt.Replace("00:00:00", "");
            string strdate2 = strdate1.Trim();
            string strdate = strdate2;// new string(strdate2.Where(c => char.IsLetter(c) || char.IsDigit(c) || c.Equals('/') || c.Equals(',') || c.Equals('-') || c.Equals(':')).ToArray());

            //  var strdate = Convert.ToDateTime(strdt).Date.ToString("dd/mm/yyyy");
            try
            {
                if (DateTime.TryParseExact(strdate, "DD/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture,
       System.Globalization.DateTimeStyles.None, out SODate))
                    return SODate;
                else if (DateTime.TryParseExact(strdate, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture,
             System.Globalization.DateTimeStyles.None, out SODate))
                    return SODate;
                else if (DateTime.TryParseExact(strdate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture,
           System.Globalization.DateTimeStyles.None, out SODate))
                    return SODate;
                else if (DateTime.TryParseExact(strdate, "MM-dd-yyyy", System.Globalization.CultureInfo.InvariantCulture,
       System.Globalization.DateTimeStyles.None, out SODate))
                    return SODate;
                else if (DateTime.TryParseExact(strdate, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture,
  System.Globalization.DateTimeStyles.None, out SODate))
                    return SODate;
                else if (DateTime.TryParseExact(strdate, "dd MMM yyyy", System.Globalization.CultureInfo.InvariantCulture,
         System.Globalization.DateTimeStyles.None, out SODate))
                    return SODate;
                else if (DateTime.TryParseExact(strdate, "dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture,
        System.Globalization.DateTimeStyles.None, out SODate))
                    return SODate;

                else return DateTime.Now;

            }
            catch (Exception)
            {

                return DateTime.Now;
            }

        }

        public static DateTime ParseStringtoDatetime(string strdt)
        {
            DateTime SODate = new DateTime();
           // string strdate1 = strdt.Replace("00:00:00", "");
           // string strdate2 = strdate1.Trim();
            string strdate = strdt;// new string(strdate2.Where(c => char.IsLetter(c) || char.IsDigit(c) || c.Equals('/') || c.Equals(',') || c.Equals('-') || c.Equals(':')).ToArray());

            //  var strdate = Convert.ToDateTime(strdt).Date.ToString("dd/mm/yyyy"); //11/24/2021 16:10:54
            try
            {
                if (DateTime.TryParseExact(strdate, "MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture,
       System.Globalization.DateTimeStyles.None, out SODate))
                    return SODate;

                if (DateTime.TryParseExact(strdate, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture,
     System.Globalization.DateTimeStyles.None, out SODate))
                    return SODate;


                else return DateTime.Now;

            }
            catch (Exception)
            {

                return DateTime.Now;
            }

        }

    }
}
