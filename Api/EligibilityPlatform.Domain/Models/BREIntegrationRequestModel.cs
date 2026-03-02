using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Domain.Models
{

   
    public class BREIntegrationAlignmentRequest
    {
        public Dictionary<int, object>? KeyValues { get; set; }
    }
    public class BREIntegrationRequest
    {
        // Basic Information
        public string? NationalIdNumber { get; set; }             // Freelancer National ID
        public int? CustomerAge { get; set; }                     // Freelancer Age
        public decimal? MaxSalaryAllowed { get; set; }           // Freelancer Salary
        public string? SpecialNeedStatus { get; set; }           // Special Need Status
        public string? CityInNationalId { get; set; }            // City in National ID
        public string? SocialSecurityStatus { get; set; }        // Social Securiety Status
        public string? EmploymentSectorCode { get; set; }        // Work Entity
        public string? EmploymentSectorName { get; set; }        // Sub work entity
        public string? WorkEntity { get; set; }                  // Work Entity
        public string? SubWorkEntity { get; set; }               // Sub work entity
        public string? DrivingLicense { get; set; }              // Driving License
        public string? IsSdbEmployee { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string? InternalObligationDetails { get; set; }
        public string? InternalPdoDetails { get; set; }
        public string? Blacklist { get; set; }               // Blacklist

        // Corporate Guarantees
        public List<CorporateGuarantee>? CustomerCorporateGuarantees { get; set; } // (No direct match)

        // MOZN Integration Inputs
        public DateTime? DateOfBirth { get; set; }               // (No direct match)
        public decimal? SalaryAllowance { get; set; }            // Freelancer Salary
        public string? Gender { get; set; }                      // (No direct match)
        public string? Profession { get; set; }                  // Freelancer Activity
        public string? EducationalDegree { get; set; }           // (No direct match)
        public string? EducationalMajor { get; set; }            // (No direct match)
        public string? MaritalStatus { get; set; }               // (No direct match)
        public string? SubSectorName { get; set; }               // (No direct match)
        public string? BankRegion { get; set; }                  // Bank Details
        public string? SPLRegion { get; set; }                   // (No direct match)
        public string? SIMAHDetails { get; set; }                // SIMAHDetails
        public string? EmploymentStatus { get; set; }
        public string? FreelancerCategory { get; set; }
        // Employment Status
    }

    public class CorporateGuarantee
    {
        public string? AgreementCode { get; set; }
        public string? ProductCode { get; set; }
        public decimal? Limit { get; set; }
    }

    // Response DTOs
    public class BREIntegrationResponse
    {
        public string? RequestId { get; set; }
        public int CustomerScore { get; set; }
        public int ProbabilityOfDefault { get; set; }
        public List<EligibleProduct> EligibleProducts { get; set; } = [];
        public List<NonEligibleProduct> NonEligibleProducts { get; set; } = [];
        public long ProcessingTimeMs { get; set; }
        public DateTime Timestamp { get; set; }
    }
    public class BREIntegrationResponses
    {
        public string? RequestId { get; set; }
        public int CustomerScore { get; set; }
        public int ProbabilityOfDefault { get; set; }
        public List<EligibleProducts> EligibleProducts { get; set; } = [];
        public List<NonEligibleProduct> NonEligibleProducts { get; set; } = [];
        public long ProcessingTimeMs { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Message { get; set; }
        public List<string>? MandatoryParameters { get; set; }
    }

    public class EligibleProducts
    {
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public decimal MaxFinancingPercentage { get; set; }
        //public decimal FinancingCap { get; set; }
        public decimal ProductCapAmount { get; set; }
        //public int ProbabilityOfDefault { get; set; }
        //public decimal MaximumProductCapPercentage { get; set; }
        public decimal EligibleAmount { get; set; }
        public bool IsEligible { get; set; } = true;
    }

    public class EligibleProduct
    {
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public decimal MaxFinancingPercentage { get; set; }
        //public decimal FinancingCap { get; set; }
        public decimal ProductCapAmount { get; set; }
        //public int ProbabilityOfDefault { get; set; }
        //public decimal MaximumProductCapPercentage { get; set; }
        public decimal EligibleAmount { get; set; }
        public decimal? LimitAmount { get; set; } = 0;
        public bool IsEligible { get; set; } = true;
    }

    public class NonEligibleProduct
    {
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        //public List<string>? Data { get; set; }
        public List<RejectionReason> RejectionReasons { get; set; } = [];

    }
    public class RejectionReason
    {
        public string? Code { get; set; }
        public string? Description { get; set; }
    }
    // External API DTOs
    public class ExternalDataResponse
    {
        public MOZNResponse? MOZN { get; set; }
        public FLIPResponse? FLIP { get; set; }
        public YaqeenResponse? Yaqeen { get; set; }
        public FutureWorksResponse? FutureWorks { get; set; }
        public SIMAHResponse? SIMAH { get; set; }
        public WaseetResponse? Waseet { get; set; }
    }

    // MOZN Models
    public class MOZNRequest
    {
        public string? NationalId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public decimal SalaryAllowance { get; set; }
        public string? Gender { get; set; }
        public string? Profession { get; set; }
        public string? EducationalDegree { get; set; }
        public string? EducationalMajor { get; set; }
        public string? MaritalStatus { get; set; }
        public string? SocialSecurityStatus { get; set; }
        public string? SpecialNeedStatus { get; set; }
        public string? SubSectorName { get; set; }
        public string? BankRegion { get; set; }
        public string? SPLRegion { get; set; }
        public string? FreelanceCategory { get; set; }
        //public string? SIMAHDetails { get; set; }
        public double? SimahDefaultBalance { get; set; }
        public string? SimahCourtCase { get; set; }
        public double? SIMAHTotalMonthlyInstallment { get; set; }

    }

    public class MOZNResponse
    {
        public string? Status { get; set; }
        public decimal CustomerScore { get; set; }
        public decimal ProbabilityOfDefault { get; set; }
        public string? Id { get; set; }
    }

    // FLIP Models
    public class FLIPResponse
    {
        public string? Status { get; set; }
        public ErrorDetail? Error { get; set; }
        public List<FLIPData>? Data { get; set; }
    }

    public class FLIPData
    {
        public int Id { get; set; }
        public string? TransactionID { get; set; }
        public string? JsonContent { get; set; }
        public DateTime IngestedAt { get; set; }
        public string? NationalId { get; set; }
        public string? PlatformName { get; set; }
        public string? Source { get; set; }

        // New fields from FLIP API
        public int TotalNumberOfHoursWorked { get; set; }
        public string? Cyclicality { get; set; }
        public bool ActiveOnPlatform { get; set; }
        public string? Segment { get; set; }
    }

    // Yageen Models
    public class YaqeenResponse
    {
        public string? Status { get; set; }
        public YageenCitizenInfo? CitizenInfo { get; set; }
        public ErrorDetail? Error { get; set; }
        public List<Validation>? Validations { get; set; }
    }

    public class YageenCitizenInfo
    {
        public string? SaudiId { get; set; }
        public string? Firstname { get; set; }
        public string? Fathername { get; set; }
        public string? Grandfathername { get; set; }
        public string? SubtribeName { get; set; }
        public string? Familyname { get; set; }
        public string? FullName { get; set; }
        public string? Gender { get; set; }
        public string? DateOfBirthH { get; set; }
        public string? PlaceOfBirth { get; set; }
        public string? LastSocialEventDate { get; set; }
        public string? LastSocialEventGregDate { get; set; }
        public string? SocialStatusCode { get; set; }
        public string? OccupationCode { get; set; }
        public string? LifeStatus { get; set; }
        public string? IdIssueDate { get; set; }
        public string? Idexpirydate { get; set; }
        public decimal TrafficViolationAmount { get; set; }
        public int UpdateidIssueDate { get; set; }
        public string? DeathDate { get; set; }
        public string? SocialStatusDescription { get; set; }
    }

    // FutureWorks Models
    public class FutureWorksResponse
    {
        public string? Status { get; set; }
        public ErrorDetail? Error { get; set; }
        public FutureWorksData? Data { get; set; }
    }

    public class FutureWorksData
    {
        public string? EnglishName { get; set; }
        public string? NationalId { get; set; }
        public string? ArabicName { get; set; }
        public string? NationalIdExpiryDate { get; set; }
        public List<Certificate>? Certificate { get; set; }
        public string? Gender { get; set; }
    }

    public class Certificate
    {
        public string? ExpiryDate { get; set; }
        public string? CertificateNumber { get; set; }
        public string? Status { get; set; }
        public string? RevokedAt { get; set; }
        public string? IssueDate { get; set; }
        public string? CanceledAt { get; set; }
        public Speciality? Speciality { get; set; }
        public string? CertificateIndustry { get; set; }
        public string? Activity { get; set; }
        public string? CertificateValidity { get; set; }
    }

    public class Speciality
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? NameEn { get; set; }
        public CategoryFutureWorks? Category { get; set; }
    }

    public class CategoryFutureWorks
    {
        public string? NameEn { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
    }

    // SIMAH Models
    public class SIMAHRequest
    {
        public string? BirthDate { get; set; }
        public string? Businesstype { get; set; }
        public string? EmployerName { get; set; }
        public string? EmployerType { get; set; }
        public string? FamilyNameAr { get; set; }
        public string? FamilyNameEn { get; set; }
        public string? FirstNameAr { get; set; }
        public string? FirstNameEn { get; set; }
        public string? Gender { get; set; }
        public string? IDExpiryDate { get; set; }
        public string? IDNumber { get; set; }
        public string? MaritalStatus { get; set; }
        public string? MemberID { get; set; }
        public string? MemberRefNumber { get; set; }
        public string? MobileNumber { get; set; }
        public string? Nationality { get; set; }
        public string? Occupation { get; set; }
        public decimal TotalSalary { get; set; }
        public object? ExtensionData { get; set; }
    }
    public class SIMAHResponse
    {
        public bool Success { get; set; }
        public decimal DefaultBalance { get; set; }
        public string? DefaultSector { get; set; }
        public bool IsCourtCasesExists { get; set; }
        public bool IsDefault { get; set; }
        public string? MemberRefNumber { get; set; }
        public decimal Salary { get; set; }
        public string? Status { get; set; }
        public decimal TotalMonthlyIns { get; set; }

        // Additional properties for Obligation Details, PDO, Court Orders, Bounced Cheques
        public string? ObligationDetails { get; set; }
        public string? PDODetails { get; set; }
        public string? CourtOrder { get; set; }
        public string? BouncedCheques { get; set; }
    }

    // Supporting classes for the additional data
    public class ObligationDetail
    {
        public string? InstitutionName { get; set; }
        public string? ProductType { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal MonthlyInstallment { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class PDODetail
    {
        public string? InstitutionName { get; set; }
        public string? PDOType { get; set; }
        public decimal Amount { get; set; }
        public DateTime? Date { get; set; }
        public string? Status { get; set; }
    }

    public class CourtOrder
    {
        public string? CaseNumber { get; set; }
        public string? CourtName { get; set; }
        public string? CaseType { get; set; }
        public decimal Amount { get; set; }
        public DateTime? CaseDate { get; set; }
        public string? Status { get; set; }
    }

    public class BouncedCheque
    {
        public string? ChequeNumber { get; set; }
        public string? BankName { get; set; }
        public decimal Amount { get; set; }
        public DateTime? BounceDate { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }
    }

    // Waseet Models
    public class WaseetResponse
    {
        public string? Status { get; set; }
        public List<WaseetProduct>? Products { get; set; }
        public List<CorporateGuaranteeResponse>? CorporateGuarantees { get; set; }
        public List<BankEmployee>? BankEmployees { get; set; }
        public List<InternalObligation>? InternalObligations { get; set; }
        public bool? IsBlacklisted { get; set; }
    }

    public class WaseetProduct
    {
        public string? FreelanceProductCode { get; set; }
        public string? FreelanceProductName { get; set; }
        public string? FreelanceSubProductCode { get; set; }
        public string? FreelanceSubProductName { get; set; }
    }

    public class CorporateGuaranteeResponse
    {
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public List<string>? NationalIds { get; set; }
        public DateTime AgreementStartDate { get; set; }
        public DateTime AgreementExpiryDate { get; set; }
        public decimal AgreementAmount { get; set; }
    }

    public class BankEmployee
    {
        public string? IdNumber { get; set; }
        public DateTime EmployeeSince { get; set; }
        public string? Position { get; set; }
        public string? UnitId { get; set; }
        public string? UnitName { get; set; }
    }

    public class InternalObligation
    {
        public string? ObligationType { get; set; }
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Status { get; set; }
    }

    // Supporting DTOs
    public class ScoringResult
    {
        public int CustomerScore { get; set; }
        public int ProbabilityOfDefault { get; set; }
    }

    public class EligibilityData
    {
        public BREIntegrationRequest RequestData { get; set; } = new();
        public ExternalDataResponse ExternalData { get; set; } = new();
        public int CustomerScore { get; set; }
        public int ProbabilityOfDefault { get; set; }
        public string? RequestId { get; set; }
        public Dictionary<int, object> KeyValues { get; set; } = [];
    }

    public class ErrorDetail
    {
        public string? ErrorCode { get; set; }
        public string? ErrorDescription { get; set; }
        public string? SubError { get; set; }
        public List<Validation>? Validations { get; set; }
    }

    public class Validation
    {
        public string? PropertyName { get; set; }
        public List<string>? ValidationMessage { get; set; }
    }
    public class CustomerCorporateGuarantee
    {
        public string? AgreementCode { get; set; }
        public string? ProductCode { get; set; }
        public decimal? Limit { get; set; }
    }
}
