using MapsterMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Provides services for managing security groups.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="SecurityGroupService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class SecurityGroupService(IUnitOfWork uow, IMapper mapper) : ISecurityGroupService
    {
        /// <summary>
        /// The unit of work instance for database operations.
        /// </summary>
        private readonly IUnitOfWork _uow = uow;

        /// <summary>
        /// The AutoMapper instance for object mapping.
        /// </summary>
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Adds a new security group to the database.
        /// </summary>
        /// <param name="securityGroupModel">The SecurityGroupModel containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(SecurityGroupUpdateModel securityGroupModel)
        {
            securityGroupModel.CreatedByDateTime = DateTime.Now;
            // Sets the update timestamp to current UTC time
            securityGroupModel.UpdatedByDateTime = DateTime.UtcNow;
            // Maps the incoming model to SecurityGroup entity and adds to repository
            _uow.SecurityGroupRepository.Add(_mapper.Map<SecurityGroup>(securityGroupModel));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Gets all security groups.
        /// </summary>
        /// <returns>A list of SecurityGroupModel representing all security groups.</returns>
        public List<SecurityGroupModel> GetAll(int tenantId)
        {
            // Retrieves all security groups from the repository
            var securityGroups = _uow.SecurityGroupRepository.GetAllByTenantId(tenantId);
            // Maps the security groups to SecurityGroupModel objects
            return _mapper.Map<List<SecurityGroupModel>>(securityGroups);
        }

        /// <summary>
        /// Gets a security group by its ID.
        /// </summary>
        /// <param name="id">The security group ID to retrieve.</param>
        /// <returns>The SecurityGroupModel for the specified ID.</returns>
        public SecurityGroupModel GetById(int id,int tenantId)
        {
            // Retrieves the specific security group by ID
            var securityGroup = _uow.SecurityGroupRepository.Query().Where(s=>s.GroupId==id && s.TenantId==tenantId) ?? throw new KeyNotFoundException("security Group not found");
            // Maps the security group to SecurityGroupModel object
            return _mapper.Map<SecurityGroupModel>(securityGroup);
        }

        /// <summary>
        /// Removes a security group by its ID.
        /// </summary>
        /// <param name="id">The security group ID to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Remove(int id)
        {
            try
            {
                // Retrieves the security group by ID
                var item = _uow.SecurityGroupRepository.GetById(id);
                // Removes the security group from the repository
                _uow.SecurityGroupRepository.Remove(item);
                // Commits the changes to the database
                await _uow.CompleteAsync();
            }
            catch (Exception ex)
            {
                // Throws exception with error message if operation fails
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing security group.
        /// </summary>
        /// <param name="securityGroupModel">The SecurityGroupModel containing updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(SecurityGroupUpdateModel securityGroupModel)
        {
            // Retrieves the existing security group by ID
            var item = _uow.SecurityGroupRepository.GetById(securityGroupModel.GroupId);
            // Sets the update timestamp to current UTC time
            securityGroupModel.UpdatedByDateTime = DateTime.UtcNow;
            var createdBy = item.CreatedBy;
            var createdByDateTime = item.CreatedByDateTime;
            // Updates the security group with mapped data from the model
            _uow.SecurityGroupRepository.Update(_mapper.Map<SecurityGroupUpdateModel, SecurityGroup>(securityGroupModel, item));

            item.CreatedBy = createdBy;
            item.CreatedByDateTime = createdByDateTime;
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes multiple security groups by their IDs.
        /// </summary>
        /// <param name="ids">A list of security group IDs to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task MultipleDelete(List<int> ids)
        {
            // Validates that all provided IDs exist
            foreach (var id in ids)
            {
                // Checks if the security group exists in the database
                var hasvalue = await _uow.SecurityGroupRepository.Query().AnyAsync(item => item.GroupId == id);
                // Throws exception if any ID is not found
                if (hasvalue == false)
                {
                    throw new Exception($"these  id's: {id} is not present. please provide valid id. ");
                }
            }

            // Deletes all validated security groups
            foreach (var id in ids)
            {
                // Retrieves each security group by ID
                var manageitem = _uow.SecurityGroupRepository.GetById(id);
                // Removes the security group if found
                if (manageitem != null)
                {
                    _uow.SecurityGroupRepository.Remove(manageitem);
                }
            }

            // Commits all deletion changes to the database
            await _uow.CompleteAsync();
        }
    }
}
