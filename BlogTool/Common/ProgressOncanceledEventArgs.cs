﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlogTool.Common
{
    public class ProgressOncanceledEventArgs : EventArgs
    {
        public double CurrentVal { get; set; }
        public double TotalVal { get; set; }
    }
}
