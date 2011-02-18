﻿//using System;
//using System.Collections.Generic;
//using System.Linq;

namespace Vts.MonteCarlo.TallyActions
{
    public abstract class HistoryTallyBase : TallyBase
    {        
        protected AbsorptionWeightingType _awt;

        public HistoryTallyBase(ITissue tissue): base(tissue)
        {
            _awt = tissue.AbsorptionWeightingType;

            SetAbsorbAction(_awt);
        }

        protected abstract void SetAbsorbAction(AbsorptionWeightingType awt);
        //protected ITissue _tissue;
        //protected IList<OpticalProperties> _ops;

        //public HistoryTallyBase(ITissue tissue)
        //    : base(tissue.AbsorptionWeightingType)
        //{
        //    _tissue = tissue;
        //    _ops = tissue.Regions.Select(r => r.RegionOP).ToArray();
        //}
    }
}