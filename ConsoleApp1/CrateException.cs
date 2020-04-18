using System;

namespace eq2crate
{
    public class CrateException:Exception
    {
        public readonly int severity;
        public CrateException():base("Generic Crate Exception!")
        {
            severity = 0;
        }
        public CrateException(int severe) : base("Generic Crate Exception with Severity")
        {
            severity = severe;
        }
        public CrateException(string reason_string) : base(reason_string)
        {
            severity = 0;
        }
        public CrateException(Exception this_exception) : base("Typed Crate Exception", this_exception)
        {
            severity = 0;
        }
        public CrateException(string reason_string, Exception this_exception) : base(reason_string, this_exception)
        {
            severity = 0;
        }
        public CrateException(string reason_string, int severe) : base(reason_string)
        {
            severity = severe;
        }
        public CrateException(int severe, Exception this_exception) : base("Typed Crate Exception with severity", this_exception)
        {
            severity = severe;
        }
        public CrateException(string reason_string, int severe, Exception this_exception) : base(reason_string, this_exception)
        {
            severity = severe;
        }
    }
}
