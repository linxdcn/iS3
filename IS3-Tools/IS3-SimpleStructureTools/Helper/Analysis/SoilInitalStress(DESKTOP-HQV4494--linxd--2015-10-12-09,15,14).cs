using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IS3.Core;

namespace IS3.SimpleStructureTools.Helper.Analysis
{
    public class SoilInitalStress
    {
        public enum ISMethod { OverburdenPressure, TerzaghiFormula };

        public class IStratum
        {
            public DGObject StratumObj { get; set; }
            public string Name { get; set; }
            public double Top { get; set; }
            public double Thickness { get; set; }
            public double Gama { get; set; }
            public double K0 { get; set; }
            public double c { get; set; }
            public double fai { get; set; }

            public IStratum Clone()
            {
                IStratum s = new IStratum();
                s.StratumObj = StratumObj;
                s.Name = Name;
                s.Top = Top;
                s.Thickness = Thickness;
                s.Gama = Gama;
                s.K0 = K0;
                s.c = c;
                s.fai = fai;
                return s;
            }

            public string FormatResult()
            {
                string str = "";
                str += string.Format("Strata name: {0}, Top: {1:0.00} m, Thickness: {2:0.00} m, " +
                    "Gama: {3:0.0} kN/m^3, K0: {4:0.00}, c:{5:0.00} kPa, φ:{6:0.00}° \n",
                    Name, Top, Thickness, Gama, K0, c, fai);
                return str;
            }
        }

        public class IResult
        {
            public double Pg;   // Segment dead load
            public double Pe1;  // Vertical ground pressure at top
            public double Pe2;  // Subgrade reaction at bottom
            public double Qe1;  // Horizontal ground pressure at top
            public double Qe2;  // Horizontal ground pressure at bottom
            public double Pw1;  // Vertical water pressure at top
            public double Pw2;  // Vertical water pressure at bottom
            public double Qw1;  // Horizontal water pressure at top
            public double Qw2;  // Horizontal water pressure at bottom

            // Water pressure at any given position
            //  sita: radial position relative to top of the tunnel, in radian
            //  Rc: radius of the tunnel
            public double Pw(double sita, double Rc)
            {
                return Pw1 + 10.0 * Rc * (1.0 - Math.Cos(sita));
            }
        }

        public double D { get; set; }       // tunnel diameter
        public double Thickness { get; set; }   // segment thickness
        public double SigmaT { get; set; }  // support pressure applied at the face ( note this is zero for a sprayed conreter lining)
        public double P0;                   // Surcharge
        public double WaterTable { get; set; }      // water table elevation
        public double SoilTop { get; set; }         // soil top elevation
        public double TunnelTop { get; set; }       // tunnel top elevation
        public ISMethod Method { get; set; }        // Initial stress calculation method
        public string StrResult { get; set; }
        public IResult result { get; set; }

        public List<IStratum> Strata;

        // Get stratum at the specified elevation
        public IStratum GetStratum(double z)
        {
            foreach (IStratum s in Strata)
            {
                if (s.Top >= z && (s.Top - s.Thickness) < z)
                    return s;
            }
            return null;
        }

        // Compute strata average property: Gama, cu
        //
        public static IStratum AverageProperty(List<IStratum> strata)
        {
            if (strata == null || strata.Count == 0)
                return null;

            IStratum r = new IStratum();
            r.Top = strata[0].Top;
            r.Name = "Average";

            double d1 = 0, d2 = 0, d3 = 0, d4 = 0;
            double t1 = 0, t2 = 0, t3 = 0, t4 = 0;
            double thickness = 0;
            foreach (IStratum s in strata)
            {
                thickness += s.Thickness;

                if (s.Gama != 0)
                {
                    t1 += s.Thickness;
                    d1 += s.Gama * s.Thickness;
                }
                if (s.c != 0)
                {
                    t2 += s.Thickness;
                    d2 += s.c * s.Thickness;
                }
                if (s.fai != 0)
                {
                    t3 += s.Thickness;
                    d3 += s.fai * s.Thickness;
                }
                if (s.K0 != 0)
                {
                    t4 += s.Thickness;
                    d4 += s.K0 * s.Thickness;
                }
            }
            d1 /= t1;
            d2 /= t2;
            d3 /= t3;
            d4 /= t4;

            r.Gama = d1;
            r.c = d2;
            r.fai = d3;
            r.K0 = d4;

            r.Thickness = thickness;

            return r;
        }

        // Soil pressure at a given point at z using Terzaghi's formula
        // Tunnel top elevation: z
        // Water table: w
        // Radius of tunnel: R
        // Surcharge: P0
        //
        public static double TerzaghiPressure(IStratum s, double w, double z,
            double R, double P0, ref string str)
        {
            double H = s.Top - z;
            double Hw = w - z;
            double B1 = R / Math.Tan(Math.PI / 8.0 + s.fai * Math.PI / 180.0 / 4.0);
            str += string.Format("B1=R/tan(π/8+φ/4)={0:0.00}/tan(22.5°+{1:0.00}°/4)={2:0.00} m\n",
                R, s.fai, B1);

            double K0TanFai = s.K0 * Math.Tan(s.fai * Math.PI / 180.0);
            str += string.Format("K0*Tan(φ)={0:0.00}*Tan({1:0.00}°)={2:0.00}\n",
                s.K0, s.fai, K0TanFai);
            double K0TanFaiHB1 = K0TanFai * H / B1;
            str += string.Format("K0*Tan(φ)*H/B1={0:0.00}*{1:0.00}/{2:0.00}={3:0.00}\n",
                K0TanFai, H, B1, K0TanFaiHB1);
            double h0 = (B1 - s.c / s.Gama) * (1 - Math.Exp(-1.0 * K0TanFaiHB1)) / K0TanFai
                + P0 * Math.Exp(-1.0 * K0TanFaiHB1) / s.Gama;
            str += string.Format("h0=(B1-c/γ)*(1-exp(-K0*Tan(φ)*H/B1))/K0*Tan(φ)+P0*exp(-K0*Tan(φ)*H/B1)/γ" +
                "=({0:0.00}-{1:0.00}/{2:0.00})*(1-exp(-{3:0.00}))/{4:0.00}+{5:0.00}*exp(-{6:0.00})/{7:0.00}" +
                "={8:0.00} m\n",
                B1, s.c, s.Gama, K0TanFaiHB1, K0TanFai, P0, K0TanFaiHB1, s.Gama, h0);

            double p = 0;
            if (Hw <= 0)
            {
                p = s.Gama * h0;
                str += string.Format("Hw={0:0.00}<=0, Pe1=γ*h0={1:0.00}*{2:0.00}={3:0.00} kPa\n",
                    Hw, s.Gama, h0, p);
            }
            else if (h0 <= Hw)
            {
                p = (s.Gama - 10.0) * h0;
                str += string.Format("h0={0:0.00} <= Hw={1:0.00}, Pe1=γ'*h0={2:0.00-γw}*{3:0.00}={4:0.00} kPa\n",
                    h0, Hw, s.Gama, h0, p);
            }
            else
            {
                p = s.Gama * (h0 - Hw) + (s.Gama - 10.0) * Hw;
                str += string.Format("h0={0:0.00}>Hw={1:0.00}, ", h0, Hw);
                str += string.Format("Pe1=γ*(h0-Hw)+γ'*Hw=" +
                    "{0:0.00}*({1:0.00}-{2:0.00})+({3:0.00}-γw)*{4:0.00}={5:0.00} kPa\n",
                    s.Gama, h0, Hw, s.Gama, Hw, p);
            }

            return p;
        }

        // Ground pressure at a given point at z
        // Water table: w
        // Surcharge: P0
        //
        public static double GroundPressure(List<IStratum> strata,
            double w, double z, double P0, ref string str)
        {
            double p = 0.0;
            foreach (IStratum s in strata)
            {
                p += GroundPressure(s, w, z, ref str);
            }
            p += P0;
            return p;
        }

        // Ground pressure of a stratum over a given point at z.
        // Water table: w
        //
        public static double GroundPressure(IStratum s, double w, double z,
            ref string str)
        {
            if (s.Top <= z)
                return 0;

            // thickness above the given point z
            double t = s.Thickness;
            if (s.Top - t < z)
                t = s.Top - z;

            // hi: thickness above the ground water table
            // hj: thickness below the ground water table
            double Hi, Hj, p;
            double Gama = s.Gama;
            if (Gama == 0)
                Gama = 18.0;

            if (s.Top - t > w)
            {
                Hi = t;
                Hj = 0;
                p = Hi * Gama;
                str += string.Format("{0}:γHi={1:0.0}*{2:0.00}={3:0.00} kPa\n",
                    s.Name, Gama, Hi, p);
            }
            else if (s.Top < w)
            {
                Hi = 0;
                Hj = t;
                p = Hj * (Gama - 10.0);
                str += string.Format("{0}:γ'Hj=({1:0.0}-γw)*{2:0.00}={3:0.00} kPa\n",
                    s.Name, Gama, Hj, p);
            }
            else
            {
                Hi = s.Top - w;
                Hj = t - Hi;
                p = Hi * Gama + Hj * (Gama - 10.0);
                str += string.Format("{0}:γHi+γ'Hj={1:0.0}*{2:0.00}+({3:0.0}-γw)*{4:0.00}" +
                    "={5:0.00} kPa\n",
                    s.Name, Gama, Hi, Gama, Hj, p);
            }

            return p;
        }

        public IResult CalculateIS()
        {
            string str = "";

            // Divide strata to that of above and below TunnelTop elevation
            //
            List<IStratum> strataAboveTop = new List<IStratum>();
            List<IStratum> strataBelowTop = new List<IStratum>();
            foreach (IStratum s in Strata)
            {
                if (s.Top - s.Thickness >= TunnelTop)
                    strataAboveTop.Add(s);
                else if (s.Top <= TunnelTop)
                    strataBelowTop.Add(s);
                else
                {
                    IStratum sAbove = s.Clone();
                    IStratum sBelow = s.Clone();
                    sAbove.Thickness = s.Top - TunnelTop;
                    sBelow.Top = TunnelTop;
                    sBelow.Thickness = s.Thickness - sAbove.Thickness;
                    strataAboveTop.Add(sAbove);
                    strataBelowTop.Add(sBelow);
                }
            }

            double H = SoilTop - TunnelTop;
            double Hw = WaterTable - TunnelTop;
            double R = D / 2.0;
            str += string.Format("H = SoilTop - TunnelTop = {0:0.00}-{1:0.00} = {2:0.00} m\n",
                SoilTop, TunnelTop, H);
            str += string.Format("Hw = WaterTable - TunnelTop = {0:0.00}-{1:0.00} = {2:0.00} m\n",
                WaterTable, TunnelTop, Hw);
            str += string.Format("R = {0:0.00} m, t = {1:0.00} m\n",
                R, Thickness);

            IResult r = new IResult();
            r.Pg = 25.0 * Thickness * Math.PI;
            r.Pw1 = 10.0 * Hw;
            r.Pw2 = 10.0 * (Hw + D);
            r.Qw1 = 10.0 * (Hw + Thickness / 2.0);
            r.Qw2 = 10.0 * (Hw + D - Thickness / 2.0);
            str += string.Format("Pg = π*γc*t = 3.14*25.0*{0:0.00} = {1:0.00} kPa\n",
                Thickness, r.Pg);
            str += string.Format("Pw1 = γw*Hw = 10.0*{0:0.00} = {1:0.00} kPa\n",
                Hw, r.Pw1);
            str += string.Format("Pw2 = γw*(Hw+D) = 10.0*({0:0.00}+{1:0.00}) = {2:0.00} kPa\n",
                Hw, D, r.Pw2);
            str += string.Format("Qw1 = γw*(Hw+t/2) = 10.0*({0:0.00}+{1:0.00}/2) = {2:0.00} kPa\n",
                Hw, Thickness, r.Qw1);
            str += string.Format("Qw2 = γw*(Hw+D-t/2) = 10.0*({0:0.00}+{1:0.00}-{2:0.00}/2) = {3:0.00} kPa\n",
                Hw, D, Thickness, r.Qw2);

            double Pe1 = 0.0;
            if (Method == ISMethod.OverburdenPressure)
            {
                str += string.Format("Calculate Pe1 using overburden pressure.\n", Pe1);
                Pe1 = GroundPressure(strataAboveTop, WaterTable, TunnelTop, P0, ref str);
                str += string.Format("Pe1 = P0+ΣγiHi+ΣγjHj = {0:0.00} kPa\n", Pe1);
            }
            else if (Method == ISMethod.TerzaghiFormula)
            {
                str += string.Format("Calculate Pe1 using Terzaghi's formula.\n", Pe1);
                str += string.Format("Averaged soil properties above tunnel: ", Pe1);
                foreach (IStratum s in strataAboveTop)
                {
                    str += s.Name;
                    str += ",";
                }
                str += "\n";
                IStratum sAverage = AverageProperty(strataAboveTop);
                str += sAverage.FormatResult();
                Pe1 = TerzaghiPressure(sAverage, WaterTable, TunnelTop, R, P0, ref str);
            }

            double Pe2 = Pe1;
            foreach (IStratum s in strataBelowTop)
            {
                Pe2 += GroundPressure(s, WaterTable, TunnelTop - D, ref str);
            }
            str += string.Format("Pe2' = Pe1+γ*D = {0:0.00} kPa\n", Pe2);

            IStratum s1 = GetStratum(TunnelTop);
            IStratum s2 = Strata[Strata.Count - 1];

            double Qe1, Qe2;
            double Gama_S1, Gama_S2;
            if (Hw > TunnelTop)
            {
                Gama_S1 = s1.Gama - 10.0;
                Gama_S2 = s2.Gama - 10.0;
                Qe1 = s1.K0 * (Pe1 + Gama_S1 * Thickness / 2.0);
                Qe2 = s2.K0 * (Pe2 - Gama_S2 * Thickness / 2.0);
                str += string.Format("Qe1 = K0*(Pe1+γ'*t/2.0) = " +
                    "{0:0.00}*({1:0.00}+{2:0.0}*{3:0.00}/2.0) = {4:0.00} kPa\n",
                    s1.K0, Pe1, Gama_S1, Thickness, Qe1);
                str += string.Format("Qe2 = K0*(Pe2'-γ'*t/2.0) = " +
                    "{0:0.00}*({1:0.00}-{2:0.0}*{3:0.00}/2.0) = {4:0.00} kPa\n",
                    s1.K0, Pe2, Gama_S2, Thickness, Qe2);
            }
            else if (Hw > TunnelTop - D)
            {
                Gama_S1 = s1.Gama;
                Gama_S2 = s2.Gama - 10.0;
                Qe1 = s1.K0 * (Pe1 + Gama_S1 * Thickness / 2.0);
                Qe2 = s2.K0 * (Pe2 - Gama_S2 * Thickness / 2.0);
                str += string.Format("Qe1 = K0*(Pe1+γ*t/2.0) = " +
                    "{0:0.00}*({1:0.00}+{2:0.0}*{3:0.00}/2.0) = {4:0.00} kPa\n",
                    s1.K0, Pe1, Gama_S1, Thickness, Qe1);
                str += string.Format("Qe2 = K0*(Pe2'-γ'*t/2.0) = " +
                    "{0:0.00}*({1:0.00}-{2:0.0}*{3:0.00}/2.0) = {4:0.00} kPa\n",
                    s1.K0, Pe2, Gama_S2, Thickness, Qe2);
            }
            else
            {
                Gama_S1 = s1.Gama;
                Gama_S2 = s2.Gama;
                Qe1 = s1.K0 * (Pe1 + Gama_S1 * Thickness / 2.0);
                Qe2 = s2.K0 * (Pe2 - Gama_S2 * Thickness / 2.0);
                str += string.Format("Qe1 = K0*(Pe1+γ*t/2.0) = " +
                    "{0:0.00}*({1:0.00}+{2:0.0}*{3:0.00}/2.0) = {4:0.00} kPa\n",
                    s1.K0, Pe1, Gama_S1, Thickness, Qe1);
                str += string.Format("Qe2 = K0*(Pe2'-γ*t/2.0) = " +
                    "{0:0.00}*({1:0.00}-{2:0.0}*{3:0.00}/2.0) = {4:0.00} kPa\n",
                    s2.K0, Pe2, Gama_S2, Thickness, Qe2);
            }

            // Interesting here: two versions of Pe2, the difference lies in how to caluculate the buoyancy.
            Pe2 = Pe1 + r.Pg - Math.PI / 2.0 * R * 10.0;
            str += string.Format("Pe2 (version 1) = Pe1+Pg-π/2*R*γw = " +
                "{0:0.00}+{1:0.00}-3.14/2*{2:0.00}*10.0 = {3:0.00} kPa\n",
                Pe1, r.Pg, R, Pe2);
            Pe2 = Pe1 + r.Pg + r.Pw1 - r.Pw2;
            str += string.Format("Pe2 (version 2) = Pe1+Pg+Pw1-Pw2 = " +
                "{0:0.00}+{1:0.00}+{2:0.00}-{3:0.00} = {4:0.00} kPa\n",
                Pe1, r.Pg, r.Pw1, r.Pw2, Pe2);

            if (Pe2 < 0)
            {
                Pe1 += Math.Abs(Pe2);
                Pe2 = 0;
                str += string.Format("Pe2 < 0, Pe1 = Pe1+Abs(Pe2) = {0:0.00} kPa, Pe2 = 0 kPa\n",
                    Pe1);
            }

            r.Pe1 = Pe1;
            r.Pe2 = Pe2;
            r.Qe1 = Qe1;
            r.Qe2 = Qe2;

            StrResult = str;

            return r;
        }
    }
}
