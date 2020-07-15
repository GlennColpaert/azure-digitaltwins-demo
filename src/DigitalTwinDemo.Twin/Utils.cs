using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalTwinDemo.Twin
{
    public static class Utils
    {
        public static bool IgnoreErrors(Action operation)
        {
            if (operation == null)
                return false;
            try
            {
                operation.Invoke();
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
