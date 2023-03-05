using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace dluhy6_0.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string ErrorMessage { get; set; }

        public bool ShowErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public string Message { get; set; }
    }
}
