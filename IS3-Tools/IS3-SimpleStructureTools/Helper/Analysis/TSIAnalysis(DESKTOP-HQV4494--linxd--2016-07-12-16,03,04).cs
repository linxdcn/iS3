using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IS3.ShieldTunnel;
using IS3.SimpleStructureTools.Helper.MathTools;

namespace IS3.SimpleStructureTools.Helper.Analysis
{
    public class RingTSI
    {
        public SegmentLining sl { get; set; }
        public double tsi { get; set; }
        public double s_ave { get; set; }
        public double s_diff { get; set; }
        public double c_ave { get; set; }
        public double leakage { get; set; }
        public double crack { get; set; }
        public double spall { get; set; }
    }

    public class SectionTSIInfo
    {
        public int startRingNo { get; set; }
        public int endRingNo { get; set; }
        public double startMile { get; set; }
        public double endMile { get; set; }
        public double tsi { get; set; }
        public double s_ave { get; set; }
        public double s_diff { get; set; }
        public double c_ave { get; set; }
        public double leakage { get; set; }
        public double crack { get; set; }
        public double spall { get; set; }
    }

    
    public class TSIAnalysis
    {
        private static int SECTION_LENGTH = 200;
        
        /// <summary>
        /// calculate TSI value of a tunnel section between two stations
        /// </summary>
        /// <param name="sls">all the segmentlining of a tunnel section</param>
        /// <returns>tsi value of each segment linings</returns>
        public static List<RingTSI> getTSIResult(List<SegmentLining> sls)
        {
            List<RingTSI> results = new List<RingTSI>();
            
            //order sls by id
            IEnumerable<SegmentLining> orderSL = null;
            orderSL = from items in sls orderby items.id select items;

            //seperate the tunnel into sections
            int sectionCount = sls.Count / SECTION_LENGTH;
            if ((sls.Count - sectionCount * SECTION_LENGTH) > (SECTION_LENGTH / 2))
                sectionCount++;

            List<SectionTSIInfo> infos = new List<SectionTSIInfo>();
            for (int i = 0; i < sectionCount; i++)
            {
                SectionTSIInfo info = new SectionTSIInfo();
                info.startRingNo = i * SECTION_LENGTH + 1;
                info.endRingNo = (i + 1) * SECTION_LENGTH;
                infos.Add(info);
            }
            infos.Last().endRingNo = sls.Count();

            foreach (SectionTSIInfo info in infos)
            {
                double startMile = 0;
                if (sls[info.startRingNo - 1].ConstructionRecord.MileageAsBuilt != null && sls[info.startRingNo - 1].ConstructionRecord.MileageAsBuilt > 0)
                    startMile = sls[info.startRingNo - 1].ConstructionRecord.MileageAsBuilt.Value;
                else if (sls[info.startRingNo - 1].StartMileage != null)
                    startMile = (double)sls[info.startRingNo - 1].StartMileage;
                else
                    return null;
                info.startMile = startMile;

                double endMile = 0;
                if (sls[info.endRingNo - 1].ConstructionRecord.MileageAsBuilt != null && sls[info.endRingNo - 1].ConstructionRecord.MileageAsBuilt > 0)
                    endMile = sls[info.endRingNo - 1].ConstructionRecord.MileageAsBuilt.Value;
                else if (sls[info.endRingNo - 1].StartMileage != null)
                    endMile = (double)sls[info.endRingNo - 1].StartMileage;
                else
                    return null;
                info.endMile = endMile;
            }

            //monitoring results
            List<SegmentLining> settRecords = new List<SegmentLining>();
            List<SegmentLining> convRecords = new List<SegmentLining>();
            List<SegmentLining> leakageRecords = new List<SegmentLining>();
            List<SegmentLining> crackRecords = new List<SegmentLining>();
            List<SegmentLining> spallRecords = new List<SegmentLining>();

            foreach (SegmentLining sl in orderSL)
            {
                if (sl.ConstructionRecord.SLSettlementRecords.SLSettlementItems.Count > 0)
                    settRecords.Add(sl);

                if (sl.ConstructionRecord.SLConvergenceRecords.SLConvergenceItems.Count > 0)
                    convRecords.Add(sl);

                if (sl.InspectionRecords.LeakageRecords.Count > 0)
                    leakageRecords.Add(sl);

                if (sl.InspectionRecords.SLCrackRecords.Count > 0)
                    crackRecords.Add(sl);

                if (sl.InspectionRecords.SLSpallRecords.Count > 0)
                    spallRecords.Add(sl);
            }

            GetSettlementResult(settRecords, infos);
            GetConvergenceResult(convRecords, infos);
            GetLeakageResult(leakageRecords, infos);
            GetCrackResult(crackRecords, infos);
            GetSpallResult(spallRecords, infos);

            return results;
        }

        private static void GetSettlementResult(List<SegmentLining> settRecords, List<SectionTSIInfo> infos)
        {
            List<int> sequenceNo = new List<int>();             //环号
            List<double> mile = new List<double>();             //里程
            List<double> h0 = new List<double>();               //初始高程
            List<double> h_now = new List<double>();            //当此高程


            for (int num = 0; num < settRecords.Count; num++)
            {
                if (settRecords[num].ConstructionRecord.SLSettlementRecords.SLSettlementItems.Last().Elevation == null ||
                    settRecords[num].ConstructionRecord.SLSettlementRecords.SLSettlementItems.Last().InitialElev == null)
                    continue;

                if (settRecords[num].ConstructionRecord.MileageAsBuilt != null && settRecords[num].ConstructionRecord.MileageAsBuilt > 0)
                    mile.Add(settRecords[num].ConstructionRecord.MileageAsBuilt.Value);
                else if (settRecords[num].StartMileage != null)
                    mile.Add((double)settRecords[num].StartMileage);
                else
                    continue;

                double h0_temp = (double)settRecords[num].ConstructionRecord.SLSettlementRecords.SLSettlementItems.Last().InitialElev;
                double h_now_temp = (double)settRecords[num].ConstructionRecord.SLSettlementRecords.SLSettlementItems.Last().Elevation;

                sequenceNo.Add(num);
                h0.Add(h0_temp);
                h_now.Add(h_now_temp);
            }
            if (h0.Count() == 0 || h_now.Count() == 0)
                return;

            CalSettlement(mile, h0, h_now, infos);
        }
        private static void CalSettlement(List<double> mile, List<double> h0, List<double> h_now, List<SectionTSIInfo> infos)
        {
            //去除异常值
            HashSet<int> outlierIndexs = new HashSet<int>();
            List<double> temp_hd = new List<double>();
            for (int i = 0; i < h0.Count; i++)
                temp_hd.Add((h_now[i] - h0[i]));
            List<int> outliers = MathHelper.FindOutlierIndex(temp_hd);
            foreach (int i in outliers)
                outlierIndexs.Add(i);

            //有些沉降数据没有记录，但用初始记录代替，需剔除
            //若有沉降记录超过20mm，则认为所有监测点都有沉降
            bool no_zero = false;
            for (int i = 0; i < h_now.Count; i++)
            {
                if (Math.Abs(h_now[i] - h0[i]) * 1000 > 20)
                {
                    no_zero = true;
                    break;
                }
            }
            if (no_zero)
            {
                for (int i = 0; i < h_now.Count; i++)
                {
                    if ((h_now[i] - h0[i]) * 1000 == 0)
                    {
                        outlierIndexs.Add(i);
                    }
                }
            }

            //除去异常值后的沉降数据
            List<double> new_mile = new List<double>();
            List<double> new_h0 = new List<double>();
            List<double> new_h_now = new List<double>();
            for (int i = 0; i < h0.Count; i++)
            {
                if (outlierIndexs.Contains(i))
                    continue;
                new_mile.Add(mile[i]);
                new_h0.Add(h0[i]);
                new_h_now.Add(h_now[i]);
            }

            //计算最高点沉降
            int num = new_h0.Count;
            double sett_min = -1000000;
            for (int i = 0; i < num; i++)
            {
                double tmp = new_h_now[i] - new_h0[i];
                if (tmp > sett_min)
                    sett_min = tmp;
            }

            //计算沉降结果
            foreach (SectionTSIInfo info in infos)
            {
                //统计某一小段的沉降数据，首先对数据筛选
                double startM = info.startMile;
                double endM = info.endMile;
                //最终分析数据
                List<double> _mile = new List<double>();
                List<double> _h0 = new List<double>();
                List<double> _h_now = new List<double>();
                for (int i = 0; i < num; i++)
                {
                    if (new_mile[i] <= Math.Max(startM, endM) && new_mile[i] >= Math.Min(startM, endM))
                    {
                        _mile.Add(new_mile[i]);
                        _h0.Add(new_h0[i]);
                        _h_now.Add(new_h_now[i]);
                    }
                }

                int count = _h0.Count;

                //相对沉降，以最高点处为基准点
                List<double> temp_h = new List<double>();
                for (int i = 0; i < count; i++)
                    temp_h.Add(_h_now[i] - _h0[i] - sett_min);
                info.s_ave  = MathHelper.Mean(temp_h) * 1000;

                //差异沉降
                temp_h = new List<double>();
                for (int i = 1; i < count; i++)
                    temp_h.Add(Math.Abs((_h_now[i] - _h_now[i - 1]) / (_mile[i] - _mile[i - 1])));           
                info.s_diff = MathHelper.Mean(temp_h) * 1000;
            }
        }

        private static void GetConvergenceResult(List<SegmentLining> convRecords, List<SectionTSIInfo> infos)
        {
            List<int> RingNo = new List<int>();             //环号
            List<double> mile = new List<double>();             //里程
            List<double> d_now = new List<double>();
            List<double> devi = new List<double>();


            for (int num = 0; num < convRecords.Count; num++)
            {
                if (convRecords[num].ConstructionRecord.SLConvergenceRecords.SLConvergenceItems.Last().HorizontalRad == null ||
                    convRecords[num].ConstructionRecord.SLConvergenceRecords.SLConvergenceItems.Last().HorizontalDev == null)
                    continue;

                if (convRecords[num].ConstructionRecord.MileageAsBuilt != null && convRecords[num].ConstructionRecord.MileageAsBuilt > 0)
                    mile.Add(convRecords[num].ConstructionRecord.MileageAsBuilt.Value);
                else if (convRecords[num].StartMileage != null)
                    mile.Add((double)convRecords[num].StartMileage);
                else
                    continue;

                double d_now_temp = (double)convRecords[num].ConstructionRecord.SLConvergenceRecords.SLConvergenceItems.Last().HorizontalRad;
                double devi_temp = (double)convRecords[num].ConstructionRecord.SLConvergenceRecords.SLConvergenceItems.Last().HorizontalDev;

                RingNo.Add(convRecords[num].RingNo);
                d_now.Add(d_now_temp);
                devi.Add(devi_temp);
            }
            if (d_now.Count() == 0 || devi.Count() == 0)
                return;

            CalConvergence(RingNo, d_now, devi, infos);
        }
        private static void CalConvergence(List<int> RingNo, List<double> d_now, List<double> devi, List<SectionTSIInfo> infos)
        {
            //去除异常值
            HashSet<int> outlierIndexs = new HashSet<int>();

            //假设所有衬砌均有收敛，剔除devi为0的数据
            for (int i = 0; i < devi.Count; i++)
            {
                if (devi[i] == 0)
                {
                    outlierIndexs.Add(i);
                }
            }

            List<double> temp_cd = new List<double>();
            for (int i = 0; i < devi.Count; i++)
                temp_cd.Add(Math.Abs(devi[i]) / (d_now[i] - devi[i]) * 1000);
            List<int> outliers = MathHelper.FindOutlierIndex(temp_cd);
            foreach (int i in outliers)
                outlierIndexs.Add(i);

            //除去异常值后的收敛数据
            List<int> new_RingNo = new List<int>();
            List<double> new_d_last = new List<double>();
            List<double> new_d_now = new List<double>();
            List<double> new_devi = new List<double>();
            for (int i = 0; i < d_now.Count; i++)
            {
                if (outlierIndexs.Contains(i))
                    continue;
                new_RingNo.Add(RingNo[i]);
                new_d_now.Add(d_now[i]);
                new_devi.Add(devi[i]);
            }

            //计算收敛结果
            foreach (SectionTSIInfo info in infos)
            {
                //统计某一小段的收敛数据，首先对数据筛选
                int startRing = info.startRingNo;
                int endRing = info.endRingNo;
                int num = new_d_now.Count;
                //最终分析数据
                List<int> _RingNo = new List<int>();
                List<double> _d_now = new List<double>();
                List<double> _devi = new List<double>();
                for (int i = 0; i < num; i++)
                {
                    if (new_RingNo[i] <= Math.Max(startRing, endRing) && new_RingNo[i] >= Math.Min(startRing, endRing))
                    {
                        _RingNo.Add(new_RingNo[i]);
                        _d_now.Add(new_d_now[i]);
                        _devi.Add(new_devi[i]);
                    }
                }

                int count = _d_now.Count;

                //收敛率均值
                List<double> temp_c = new List<double>();
                for (int i = 0; i < count; i++)
                    temp_c.Add(Math.Abs(_devi[i]) / (_d_now[i] - _devi[i]) * 1000);
                info.c_ave  = MathHelper.Mean(temp_c);
            }
        }

        private static void GetLeakageResult(List<SegmentLining> leakageRecords, List<SectionTSIInfo> infos)
        {            
            foreach (SectionTSIInfo info in infos)
            {   
                Dictionary<DateTime, double> results = new Dictionary<DateTime, double>();

                foreach (SegmentLining sl in leakageRecords)
                {
                    if (sl.RingNo >= Math.Min(info.startRingNo, info.endRingNo) && sl.RingNo <= Math.Max(info.startRingNo, info.endRingNo))
                        continue;

                    foreach (LeakageRecordItem item in sl.InspectionRecords.LeakageRecords)
                    {
                        if (item.Date == null)
                            continue;

                        DateTime date = (DateTime)item.Date;
                        if (!results.ContainsKey(date))
                        {
                            results[date] = 0;
                        }
                        if (item.Area != 0 && item.Area != double.NaN)
                        {
                            results[date] += item.Area.Value;
                        }
                    }
                }

                IEnumerable<DateTime> dateCollection = results.Keys;
                List<DateTime> dateList = dateCollection.ToList();
                dateList.Sort();
                info.leakage = results[dateList.Last()] / Math.Abs(info.startRingNo - info.endRingNo) * 100;
            }         
        }
        private static void GetCrackResult(List<SegmentLining> crackRecords, List<SectionTSIInfo> infos)
        {
            foreach (SectionTSIInfo info in infos)
            {
                Dictionary<DateTime, double> results = new Dictionary<DateTime, double>();

                foreach (SegmentLining sl in crackRecords)
                {
                    if (sl.RingNo >= Math.Min(info.startRingNo, info.endRingNo) && sl.RingNo <= Math.Max(info.startRingNo, info.endRingNo))
                        continue;

                    foreach (SLCrackRecordItem item in sl.InspectionRecords.SLCrackRecords)
                    {
                        if (item.Date == null)
                            continue;

                        DateTime date = (DateTime)item.Date;
                        if (!results.ContainsKey(date))
                        {
                            results[date] = 0;
                        }
                        if (item.Length != 0 && item.Length != double.NaN)
                        {
                            results[date] += item.Length.Value;
                        }
                    }
                }

                IEnumerable<DateTime> dateCollection = results.Keys;
                List<DateTime> dateList = dateCollection.ToList();
                dateList.Sort();
                info.crack = results[dateList.Last()] / Math.Abs(info.startRingNo - info.endRingNo) * 100;
            }
        }
        private static void GetSpallResult(List<SegmentLining> spallRecords, List<SectionTSIInfo> infos)
        {
            foreach (SectionTSIInfo info in infos)
            {
                Dictionary<DateTime, double> results = new Dictionary<DateTime, double>();

                foreach (SegmentLining sl in spallRecords)
                {
                    if (sl.RingNo >= Math.Min(info.startRingNo, info.endRingNo) && sl.RingNo <= Math.Max(info.startRingNo, info.endRingNo))
                        continue;

                    foreach (SLSpallRecordItem item in sl.InspectionRecords.SLSpallRecords)
                    {
                        if (item.Date == null)
                            continue;

                        DateTime date = (DateTime)item.Date;
                        if (!results.ContainsKey(date))
                        {
                            results[date] = 0;
                        }
                        if (item.Area != 0 && item.Area != double.NaN)
                        {
                            results[date] += item.Area.Value;
                        }
                    }
                }

                IEnumerable<DateTime> dateCollection = results.Keys;
                List<DateTime> dateList = dateCollection.ToList();
                dateList.Sort();
                info.leakage = results[dateList.Last()] / Math.Abs(info.startRingNo - info.endRingNo) * 100;
            }
        }
    }
}
