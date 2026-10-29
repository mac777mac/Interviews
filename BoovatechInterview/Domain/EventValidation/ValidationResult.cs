using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boovatech.Eventing.EventValidation
{
    public class ValidationResult
    {

        public bool IsOk { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static ValidationResult Error(string message)
        {
            return new ValidationResult
            {
                ErrorMessage = message,
            };
        }

        public static ValidationResult Ok = new ValidationResult
        {
            IsOk = true,
        };

    }
}
