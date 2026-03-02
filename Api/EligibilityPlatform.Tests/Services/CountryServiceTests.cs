using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MapsterMapper;
using MEligibilityPlatform.Application.Services;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Moq;
using Xunit;

namespace EligibilityPlatform.Tests.Services
{
    public class CountryServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CountryService _countryService;

        public CountryServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _countryService = new CountryService(_mockUow.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Add_ShouldAddCountryAndComplete_WhenCalled()
        {
            // Arrange
            var model = new CountryModel { CountryId = 1, CountryName = "Test Country" };
            var entity = new Country { CountryId = 1, CountryName = "Test Country" };

            _mockMapper.Setup(m => m.Map<Country>(model)).Returns(entity);
            _mockUow.Setup(u => u.CountryRepository.Add(entity, false));
            _mockUow.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

            // Act
            await _countryService.Add(model);

            // Assert
            _mockUow.Verify(u => u.CountryRepository.Add(entity, false), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
            Assert.NotEqual(default(DateTime), model.UpdatedByDateTime);
        }

        [Fact]
        public async Task Delete_ShouldRemoveCountryAndComplete_WhenProviderIdIsValid()
        {
            // Arrange
            var id = 1;
            var entity = new Country { CountryId = id, CountryName = "Test Country" };

            _mockUow.Setup(u => u.CountryRepository.GetById(id)).Returns(entity);
            _mockUow.Setup(u => u.CountryRepository.Remove(entity));
            _mockUow.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

            // Act
            await _countryService.Delete(id);

            // Assert
            _mockUow.Verify(u => u.CountryRepository.GetById(id), Times.Once);
            _mockUow.Verify(u => u.CountryRepository.Remove(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public void GetAll_ShouldReturnMappedCountries_WhenCalled()
        {
            // Arrange
            var entities = new List<Country> { new Country { CountryId = 1, CountryName = "C1" } };
            var models = new List<CountryModel> { new CountryModel { CountryId = 1, CountryName = "C1" } };

            _mockUow.Setup(u => u.CountryRepository.GetAll()).Returns(entities);
            _mockMapper.Setup(m => m.Map<List<CountryModel>>(entities)).Returns(models);

            // Act
            var result = _countryService.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("C1", result[0].CountryName);
        }

        [Fact]
        public void GetById_ShouldReturnMappedCountry_WhenProviderIdIsValid()
        {
            // Arrange
            var id = 1;
            var entity = new Country { CountryId = id, CountryName = "C1" };
            var model = new CountryModel { CountryId = id, CountryName = "C1" };

            _mockUow.Setup(u => u.CountryRepository.GetById(id)).Returns(entity);
            _mockMapper.Setup(m => m.Map<CountryModel>(entity)).Returns(model);

            // Act
            var result = _countryService.GetById(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("C1", result.CountryName);
        }

        [Fact]
        public async Task MultipleDelete_ShouldRemoveValidCountriesAndComplete()
        {
            // Arrange
            var ids = new List<int> { 1, 2, 3 };
            var entity1 = new Country { CountryId = 1 };
            Country entity2 = null; // simulate missing
            var entity3 = new Country { CountryId = 3 };

            _mockUow.Setup(u => u.CountryRepository.GetById(1)).Returns(entity1);
            _mockUow.Setup(u => u.CountryRepository.GetById(2)).Returns(entity2);
            _mockUow.Setup(u => u.CountryRepository.GetById(3)).Returns(entity3);

            // Act
            await _countryService.MultipleDelete(ids);

            // Assert
            _mockUow.Verify(u => u.CountryRepository.Remove(entity1), Times.Once);
            _mockUow.Verify(u => u.CountryRepository.Remove(It.IsAny<Country>()), Times.Exactly(2));
            _mockUow.Verify(u => u.CountryRepository.Remove(entity3), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldUpdateCountryAndComplete_WhenCalled()
        {
            // Arrange
            var model = new CountryModel { CountryId = 1, CountryName = "Updated" };
            var existingEntity = new Country { CountryId = 1, CountryName = "Old" };
            var mappedEntity = new Country { CountryId = 1, CountryName = "Updated" };

            _mockUow.Setup(u => u.CountryRepository.GetById(model.CountryId)).Returns(existingEntity);
            _mockMapper.Setup(m => m.Map<CountryModel, Country>(model, existingEntity)).Returns(mappedEntity);
            _mockUow.Setup(u => u.CountryRepository.Update(mappedEntity));
            _mockUow.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

            // Act
            await _countryService.Update(model);

            // Assert
            _mockUow.Verify(u => u.CountryRepository.Update(mappedEntity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
            Assert.NotEqual(default(DateTime), model.UpdatedByDateTime);
        }
    }
}
