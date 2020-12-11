using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mall.Web.Models
{
    public class AgreementModel
    {
        public Entities.AgreementInfo.AgreementTypes AgreementType { get; set; }
        public string AgreementContent { get; set; }
    }
}