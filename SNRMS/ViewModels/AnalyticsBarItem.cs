using System;
using System.Collections.Generic;
using System.Text;

namespace SNRMS.ViewModels
{
    public class AnalyticsBarItem
    {
        public string Label { get; }
        public int Value { get; }
        public AnalyticsBarItem(string label, int value) { Label = label; Value = value; }
    }
}
