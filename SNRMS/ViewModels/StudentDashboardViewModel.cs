using CommunityToolkit.Mvvm.ComponentModel;
using SNRMS.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SNRMS.ViewModels
{
    public partial class StudentDashboardViewModel: ObservableObject
    {
        private readonly StudentService _instructorService;
    }
}
