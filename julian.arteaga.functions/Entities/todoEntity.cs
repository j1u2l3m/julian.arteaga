using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace julian.arteaga.functions.Entities
{
    public class todoEntity : TableEntity
    {


        public DateTime Createdtime { get; set; }

        public string TaskDescription { get; set; }

        public Boolean Iscompleted { get; set; }
    }
}
