using CommunityToolkit.Mvvm.ComponentModel;
using SNRMS.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SNRMS.ViewModels
{
    public partial class InstructorDashboardViewModel:ObservableObject
    {
        public readonly InstructorService _instructorService;
    }
}
