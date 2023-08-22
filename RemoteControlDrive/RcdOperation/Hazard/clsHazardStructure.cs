using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RcdOperation.Hazard
{
    public class Setting
    {

        public string PanaWritePath { get; set; }

        public PanaWrite PanaWrite { get; set; }

        public string TMCWritePath { get; set; }

        public int RunTimeout { get; set; }

        public int ResultTimeout { get; set; }
    }

    public class PanaWrite
    {
        public bool Permition_For_Detection { get; set; }

        public string BodyNo { get; set; }

        public string CarType { get; set; }

        public string CamIP { get; set; }

        public string CamNo { get; set; }

        public PanaWrite()
        {

        }

        //private string False = "False";
        private string None = "None";

        public void SetInitialValue()
        {
            Permition_For_Detection = false;
            BodyNo = None;
            CarType = None;
            CamIP = None;
            CamNo = None;
        }
    }

    public class DetectionWrite
    {
        public bool Detection_run_condition { get; set; }

        public string Result { get; set; }

        public DetectionWrite() { }
    }
}
