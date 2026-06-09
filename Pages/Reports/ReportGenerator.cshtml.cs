using MediSphere.Business.Interfaces;
using MediSphere.Models;
using MediSphere.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace MediSphere.Pages.Reports
{
    [Authorize]
    public class ReportGeneratorModel : PageModel
    {
        private readonly IReportBusiness _reportBusiness;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public IEnumerable<PatientModel> Patients { get; set; } = Enumerable.Empty<PatientModel>();
        public IEnumerable<ReportTypeModel> ReportTypes { get; set; } = Enumerable.Empty<ReportTypeModel>();

        public ReportGeneratorModel(IReportBusiness reportBusiness, IStringLocalizer<SharedResources> localizer)
        {
            _reportBusiness = reportBusiness;
            _localizer = localizer;
        }

        [BindProperty]
        public ReportModel NewReportModel { get; set; } = new();

        [BindProperty]
        public string? ReportTypeName { get; set; }

        [BindProperty]
        public int? SelectedTemplateTypeId { get; set; }

        public ReportTypeModel? SelectedReportType { get; set; }

        public async Task<IActionResult> OnGetAsync(int reportId)
        {
            Patients = await _reportBusiness.GetPatientsForReportsAsync();
            ReportTypes = await _reportBusiness.GetReportTypesAsync();

            if (reportId != 0)
            {
                SelectedReportType = ReportTypes.FirstOrDefault(rt => rt.ReportTypeId == reportId);
                if (SelectedReportType != null)
                {
                    SelectedTemplateTypeId = SelectedReportType.ReportTypeId;
                    ReportTypeName = SelectedReportType.TemplateType;
                    NewReportModel.ReportDescription = SelectedReportType.TemplateType;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCreateReport()
        {
            if (!ModelState.IsValid)
            {
                await LoadFormDataAsync();
                return Page();
            }

            var reportTypeId = await _reportBusiness.ResolveReportTypeIdAsync(SelectedTemplateTypeId, ReportTypeName);

            var report = new ReportModel
            {
                ReportDescription = NewReportModel.ReportDescription,
                PatientId = NewReportModel.PatientId,
                InitialStaffName = NewReportModel.InitialStaffName,
                Status = NewReportModel.Status,
                ReportTypeId = reportTypeId
            };

            var result = await _reportBusiness.CreateReportAsync(report);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? _localizer["CouldNotCreateReport"]);
                await LoadFormDataAsync();
                return Page();
            }

            return RedirectToPage("/Reports/Index");
        }

        public async Task<IActionResult> OnPostCreateTemplateType()
        {
            if (string.IsNullOrWhiteSpace(ReportTypeName))
            {
                ModelState.AddModelError(string.Empty, _localizer["EnterReportTypeName"]);
                await LoadFormDataAsync();
                return Page();
            }

            var reportTypes = await _reportBusiness.GetReportTypesAsync();
            if (reportTypes.Any(t => string.Equals(t.TemplateType, ReportTypeName.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError(string.Empty, _localizer["ReportTypeExists"]);
                await LoadFormDataAsync();
                return Page();
            }

            var reporttype = new ReportTypeModel
            {
                TemplateType = ReportTypeName.Trim(),
                ReportTypeCreationTime = DateTime.Now
            };

            var result = await _reportBusiness.CreateReportTypeAsync(reporttype);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? _localizer["CouldNotCreateReportType"]);
                await LoadFormDataAsync();
                return Page();
            }

            return RedirectToPage("/Reports/ReportTypes");
        }

        private async Task LoadFormDataAsync()
        {
            Patients = await _reportBusiness.GetPatientsForReportsAsync();
            ReportTypes = await _reportBusiness.GetReportTypesAsync();
        }
    }
}
