using MapsterMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service for automated tenant onboarding with all required initial data.
    /// </summary>
    public class TenantOnboardingService(IUnitOfWork uow, IUserService userService, IHttpClientFactory httpClientFactory,IConfiguration configuration) : ITenantOnboardingService
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IUserService _userService = userService;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IConfiguration _configuration = configuration;

        /// <summary>
        /// Onboards a new tenant with all required initial data.
        /// </summary>
        public async Task<TenantOnboardingResult> OnboardNewTenantAsync(TenantOnboardingRequest request)
        {
            var result = new TenantOnboardingResult();

            try
            {

                var tenant = await GetById(request.TenantId);
                if (tenant?.Data == null)
                {
                    result.Errors.Add("Tenant not found.");
                    return result;
                }

                var user = await _userService.GetById(request.AdminUserId);
                if (user?.Data == null || user.Data.TenantId != request.TenantId)
                {
                    result.Errors.Add("The specified admin user does not belong to this tenant.");
                    return result;
                }

                result.TenantId = request.TenantId;

                var eligibility = await ValidateTenantEligibilityAsync(request.TenantId);
                if (!eligibility.IsEligible)
                {
                    result.Errors.AddRange(eligibility.Errors);
                    result.Message = eligibility.Message;
                    return result;
                }

                var now = DateTime.UtcNow;


                var roles = new List<SecurityRole>
                     {
                        new() {
                            RoleName = "Super Admin",
                            RoleDesc = "Administrator role with full access",
                            TenantId = request.TenantId,
                            CreatedBy = "System",
                            CreatedByDateTime = now,
                            UpdatedByDateTime = now
                        },
                        new() {
                            RoleName = "Admin",
                            RoleDesc = "Standard administrator role",
                            TenantId = request.TenantId,
                            CreatedBy = "System",
                            CreatedByDateTime = now,
                            UpdatedByDateTime = now
                        },
                        new() {
                            RoleName = "User",
                            RoleDesc = "Standard user role",
                            TenantId = request.TenantId,
                            CreatedBy = "System",
                            CreatedByDateTime = now,
                            UpdatedByDateTime = now
                        }
                    };

                _uow.SecurityRoleRepository.AddRange(roles);
                await _uow.CompleteAsync();

                var adminRole = roles.First(g => g.RoleName!.Equals("Super Admin",StringComparison.OrdinalIgnoreCase));


                var userRole = new UserRole
                {
                    UserId = request.AdminUserId,
                    RoleId = adminRole.RoleId,
                    TenantId = request.TenantId,
                    CreatedBy = "System",
                    CreatedByDateTime = now,
                    UpdatedBy = "System",
                    UpdatedByDateTime = now
                };

                _uow.UserRoleRepository.Add(userRole);


                var permissions = await _uow.PermissionRepository.Query().ToListAsync();

                var rolePermissions = permissions.Select(p => new RolePermission
                {
                    PermissionId = p.PermissionId,
                    RoleId = adminRole.RoleId,
                    TenantId = request.TenantId,
                    UpdatedByDateTime = now
                }).ToList();

                _uow.RolePermissionRepository.AddRange(rolePermissions);


                var dataTypes = await _uow.DataTypeRepository.Query().ToListAsync();

                var numericType = dataTypes
                    .FirstOrDefault(x => x.DataTypeName!.Equals("Numeric", StringComparison.OrdinalIgnoreCase));

                var textType = dataTypes
                    .FirstOrDefault(x => x.DataTypeName!.Equals("Text", StringComparison.OrdinalIgnoreCase));

                var parameters = new List<Parameter>();

                if (numericType != null)
                {
                parameters.AddRange(
                [
                    CreateParameter("Age", numericType.DataTypeId, request.TenantId, now, "R001", "Customer's age does not satisfy eligibility rule"),
                    CreateParameter("Salary", numericType.DataTypeId, request.TenantId, now, "R002", "Customer's salary does not satisfy eligibility rule"),
                    CreateParameter("Score", numericType.DataTypeId, request.TenantId, now, "R003", "Customer's score does not satisfy eligibility rule"),
                    CreateParameter("ProbabilityOfDefault", numericType.DataTypeId, request.TenantId, now, "R006", "ProbabilityofDefault is Required")]);
                }

                if (textType != null)
                {
                    parameters.AddRange(
                    [
                CreateParameter("LoanNo", textType.DataTypeId, request.TenantId, now, "R004", "Loan number is required"),
                CreateParameter("NationalId", textType.DataTypeId, request.TenantId, now, "R005", "Customer's national ID does not satisfy eligibility rule")]);
                }

                if (parameters.Count != 0)
                {
                    _uow.ParameterRepository.AddRange(parameters);
                }


                var setting = new Setting
                {
                    IsMakerCheckerEnable = false,
                    TenantId = request.TenantId
                };

                _uow.SettingRepository.Add(setting);


                await _uow.CompleteAsync();

                result.Success = true;
                result.Message = "Tenant onboarded successfully.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Onboarding failed.";
                result.Errors.Add(ex.Message);
            }

            return result;
        }
        private static Parameter CreateParameter( string name, int dataTypeId, int tenantId, DateTime now, string rejectionCode, string rejectionMessage)
        {
            return new Parameter
            {
                ParameterName = name,
                DataTypeId = dataTypeId,
                TenantId = tenantId,
                Identifier = 2,
                CreatedBy = "System",
                CreatedByDateTime = now,
                UpdatedByDateTime = now,
                RejectionReason = rejectionMessage,
                RejectionReasonCode =rejectionCode,
                IsMandatory=true
            };
        }

        public async Task<ApiResponse<TenantModel>> GetById(int tenantId)
        {
            var version = _configuration["MIdentityAPI:Version"] ?? "1";
            var client = _httpClientFactory.CreateClient("MIdentityAPI");
            var response = await client.GetAsync($"api/v{version}/Tenants/{tenantId}");
            // Executes the query and returns the results as a list.
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<ApiResponse<TenantModel>>();

            // Enrich with groups from local database (filtered by tenantId for multi-tenant isolation)

            return data ?? new ApiResponse<TenantModel>();
        }


        /// <summary>
        /// Validates if a tenant is eligible for onboarding (checks if already subscribed/onboarded).
        /// </summary>
        /// <param name="tenantId">The tenant ID to validate.</param>
        /// <returns>A validation result indicating eligibility and any errors.</returns>
        private async Task<TenantEligibilityValidation> ValidateTenantEligibilityAsync(int tenantId)
        {
            var validation = new TenantEligibilityValidation
            {
                IsEligible = true,
                Message = "Tenant is eligible for onboarding"
            };

            try
            {
                // Check if any core onboarding structure already exists
                var isAlreadyOnboarded =
                    await _uow.SecurityRoleRepository.Query()
                        .AnyAsync(x => x.TenantId == tenantId);
             
                if (isAlreadyOnboarded)
                {
                    validation.IsEligible = false;
                    validation.Message = "Tenant is already onboarded.";
                    validation.Errors.Add("Core security configuration already exists for this tenant.");
                    return validation;
                }
            }
            catch (Exception ex)
            {
                validation.IsEligible = false;
                validation.Message = "Validation failed";
                validation.Errors.Add($"Error validating tenant eligibility: {ex.Message}");
            }

            return validation;
        }

        /// <summary>
        /// Validates that a tenant has all required setup data.
        /// </summary>
        public async Task<bool> ValidateTenantSetupAsync(int tenantId)
        {
            try
            {
                // Check if at least one security role exists
                var hasRoles = await _uow.SecurityRoleRepository.Query()
                    .AnyAsync(sg => sg.TenantId == tenantId);
                if (!hasRoles) return false;

                //// Check if at least one user exists
                //var hasUsers = await _uow.UserRepository.Query()
                //    .AnyAsync(u => u.TenantId == tenantId);
                //if (!hasUsers) return false;

                // Check if at least one category exists
                var hasCategories = await _uow.CategoryRepository.Query()
                    .AnyAsync(c => c.TenantId == tenantId);
                if (!hasCategories) return false;

                return true;
            }
            catch
            {
                return false;
            }
        }


    }
}
