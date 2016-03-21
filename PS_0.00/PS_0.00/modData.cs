using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS_0._00
{
    class modData
    {
        private string aC, fT, pP;
        private char[] tG = new char[2];
        private double mM, mA;

        // unused but available public string pA, cF, lC, tR, kW, dR;

        public string AC
        {
            get
            {
                return this.aC;
            }
            set
            {
                aC = value;
            }
        }
        public string FT
        {
            get
            {
                return this.fT;
            }
            set
            {
                fT = value;
            }
        }
        public string PP
        {
            get
            {
                return this.pP;
            }
            set
            {
                pP = value;
            }
        }
        public char[] TG
        {
            get
            {
                return this.tG;
            }
            set
            {
                tG = value;
            }
        }
        public double MM
        {
            get
            {
                return this.mM;
            }
            set
            {
                mM = value;
            }
        }
        public double MA
        {
            get
            {
                return this.mA;
            }
            set
            {
                mA = value;
            }
        }

        //public string ToString()
        //{
        //    string thingToPrint = "AC=" + AC + " FT " + FT + " MM " + MM;

        //    return thingToPrint;
        //}
    }
}
