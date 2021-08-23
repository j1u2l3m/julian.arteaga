using System;
using System.Collections.Generic;
using System.Text;

namespace julian.arteaga.common.Models.Responses
{
    public class Response
    {

        public Boolean IaSuccess { get; set; }

        public string message { get; set; }

        public object Result { get; set; }
    }
}
