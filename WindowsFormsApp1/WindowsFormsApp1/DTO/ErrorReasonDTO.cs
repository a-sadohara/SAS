using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.DTO
{
    public class ErrorReasonDTO
    {
        string strErrorReason = string.Empty;

        public void setStrErrorReason(string strReason)
        {
            strErrorReason = strReason;
        }

        public string getStrErrorReason()
        {
            return strErrorReason;
        }
    }
}
