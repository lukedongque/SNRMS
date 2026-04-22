using System;
using System.Collections.Generic;
using System.Text;

namespace SNRMS.ViewModels
{
    public class AnalyticsBarItem
    {
        public string Label { get; }
        public int Value { get; }
        public bool IsHeader { get; }
        public string BarColor => IsHeader ? "#1B3A6B" : "#718096";
        public string LabelColor => IsHeader ? "#1B3A6B" : "#4A5568";
        public string LabelWeight => IsHeader ? "SemiBold" : "Normal";
        public int MaxValue { get; }

        public AnalyticsBarItem(string label, int value, bool isHeader = false, int maxValue = 20)
        {
            Label = label;
            Value = value;
            IsHeader = isHeader;
            MaxValue = maxValue;
        }
    }
}
