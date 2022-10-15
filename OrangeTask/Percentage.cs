using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrangeTask
{
    public struct Percentage
    {
        public Percentage(decimal value) : this()
        {
            this.Value = value;
        }
        public Percentage(double value,double Total)
        {
            this.Value = (decimal)((value / Total) * 100);
        }
        public decimal Value { get; private set; }
        public double TotalValue(double TotalNumber) => (double)(((double)(Value / 100)) * TotalNumber);

        public static explicit operator Percentage(decimal d)
        {
            return new Percentage(d);
        }

        public static implicit operator decimal(Percentage p)
        {
            return p.Value;
        }

        public static Percentage Parse(string value)
        {
            return new Percentage(decimal.Parse(value));
        }
        public static bool TryParse(string value,out Percentage percentage,out string Reason)
        {
            percentage = new Percentage(0);
            try
            {
       
                decimal d = decimal.Parse(value); 
                if (d < 0)
                {
                    Reason = "Percentage cannot be less than zero";
                    return false;
                }
                if (d > 100)
                {
                    Reason = "Percentage cannot be more than 100";
                    return false;
                }
                percentage = new Percentage(d);
                Reason = "";
                return true;
            }
            catch (Exception ex)
            {
                Reason = ex.Message;
                return false;
            }
        }
        public override string ToString()
        {
            return string.Format("{0}%", this.Value);
        }
    }
}
