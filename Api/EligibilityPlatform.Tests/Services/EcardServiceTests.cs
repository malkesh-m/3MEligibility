using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapsterMapper;
using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Application.Services;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Moq;
using OfficeOpenXml;
using Xunit;
using EligibilityPlatform.Tests.Helpers;

namespace EligibilityPlatform.Tests.Services
{
    public class EcardServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IEruleService> _mockEruleService;
        private readonly Mock<IEruleMasterService> _mockEruleMasterService;
        private readonly Mock<IExportService> _mockExportService;
        private readonly Mock<IEcardRepository> _mockEcardRepo;
        private readonly Mock<IPcardRepository> _mockPcardRepo;
        private readonly Mock<IEruleMasterRepository> _mockEruleMasterRepo;
        private readonly Mock<IEruleRepository> _mockEruleRepo;
        private readonly EcardService _service;

        public EcardServiceTests()
        {
            _mockUow = new();
            _mockMapper = new();
            _mockEruleService = new();
            _mockEruleMasterService = new();
            _mockExportService = new();
            _mockEcardRepo = new();
            _mockPcardRepo = new();
            _mockEruleMasterRepo = new();
            _mockEruleRepo = new();

            _mockEcardRepo.Setup(r => r.Query()).Returns(new List<Ecard>().BuildMock());
            _mockPcardRepo.Setup(r => r.Query()).Returns(new List<Pcard>().BuildMock());
            _mockEruleMasterRepo.Setup(r => r.Query()).Returns(new List<EruleMaster>().BuildMock());
            _mockEruleRepo.Setup(r => r.Query()).Returns(new List<Erule>().BuildMock());

            _mockUow.Setup(u => u.EcardRepository).Returns(_mockEcardRepo.Object);
            _mockUow.Setup(u => u.PcardRepository).Returns(_mockPcardRepo.Object);
            _mockUow.Setup(u => u.EruleMasterRepository).Returns(_mockEruleMasterRepo.Object);
            _mockUow.Setup(u => u.EruleRepository).Returns(_mockEruleRepo.Object);

            _service = new(
                _mockUow.Object,
                _mockMapper.Object,
                _mockEruleService.Object,
                _mockEruleMasterService.Object,
                _mockExportService.Object);
        }

        [Fact]
        public async Task Add_ExistingName_ShouldThrowException()
        {
            List<Ecard> data = [new() { TenantId = 1, EcardName = "E1" }];
            _mockEcardRepo.Setup(u => u.Query()).Returns(data.BuildMock());

            var model = new EcardAddUpdateModel { EcardName = "E1" };

            await Assert.ThrowsAsync<Exception>(() => _service.Add(1, model));
        }

        [Fact]
        public async Task Add_ValidModel_ShouldAddAndComplete()
        {
            var data = new List<Ecard>().BuildMock();
            _mockEcardRepo.Setup(u => u.Query()).Returns(data);

            var model = new EcardAddUpdateModel { EcardName = "E1" };
            var entity = new Ecard { EcardName = "E1" };
            _mockMapper.Setup(m => m.Map<Ecard>(model)).Returns(entity);

            await _service.Add(1, model);

            _mockEcardRepo.Verify(u => u.Add(entity, It.IsAny<bool>()), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldThrow_WhenNameAlreadyExists()
        {
            var existing = new List<Ecard>
            {
                new() { TenantId = 1, EcardId = 1, EcardName = "EC1" },
                new() { TenantId = 1, EcardId = 2, EcardName = "EC2" }
            }.BuildMock();

            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(existing);

            var model = new EcardUpdateModel { TenantId = 1, EcardId = 1, EcardName = "EC2" };

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.Update(model));
            Assert.Equal("Ecard with this name already exists", ex.Message);
        }

        [Fact]
        public async Task Delete_IsInUseByPcard_ShouldReturnErrorMessage()
        {
            var pcardData = new List<Pcard> { new() { TenantId = 1, Expression = "1 AND 2" } }.BuildMock();
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(pcardData);

            var result = await _service.Delete(1, 1);

            Assert.Contains("cannot be deleted because it is currently being used", result);
        }

        [Fact]
        public async Task Delete_NotInUse_ShouldDeleteAndComplete()
        {
            var pcardData = new List<Pcard>().BuildMock();
            var ecardData = new List<Ecard> { new() { EcardId = 1, TenantId = 1 } }.BuildMock();

            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(pcardData);
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(ecardData);

            var result = await _service.Delete(1, 1);

            Assert.Equal("Deleted successfully.", result);
            _mockUow.Verify(u => u.EcardRepository.Remove(It.IsAny<Ecard>()), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_ShouldReturnModels()
        {
            var data = new List<Ecard> { new() { TenantId = 1, EcardId = 1 } }.BuildMock();
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(data);

            var expected = new List<EcardListModel> { new() { EcardId = 1 } };
            _mockMapper.Setup(m => m.Map<List<EcardListModel>>(It.IsAny<List<Ecard>>())).Returns(expected);

            var result = await _service.GetAll(1);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetAll_ShouldFilterByTenant()
        {
            var data = new List<Ecard>
            {
                new() { TenantId = 1, EcardId = 1 },
                new() { TenantId = 2, EcardId = 2 }
            }.BuildMock();
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(data);

            List<Ecard>? captured = null;
            _mockMapper.Setup(m => m.Map<List<EcardListModel>>(It.IsAny<object>()))
                .Callback<object>(obj => captured = obj as List<Ecard>)
                .Returns([new EcardListModel { EcardId = 1 }]);

            var result = await _service.GetAll(1);

            Assert.NotNull(result);
            Assert.NotNull(captured);
            Assert.All(captured!, c => Assert.Equal(1, c.TenantId));
        }
        [Fact]
        public async Task ImportECard_ValidExcel_ShouldReturnSuccessMessage()
        {
            var master = new EruleMaster { Id = 1, EruleName = "R1", TenantId = 1 };
            var child = new Erule { EruleId = 100, EruleMasterId = 1, Version = 1 };
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { master }.AsQueryable().BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { child }.AsQueryable().BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard>().AsQueryable().BuildMock());

            using var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var ws = package.Workbook.Worksheets.Add("Cards");
                ws.Cells[1, 1].Value = "CardName";
                ws.Cells[1, 2].Value = "CardDescription";
                ws.Cells[1, 3].Value = "ExpressionShown";
                ws.Cells[2, 1].Value = "C1";
                ws.Cells[2, 2].Value = "D1";
                ws.Cells[2, 3].Value = "R1";
                package.Save();
            }
            stream.Position = 0;

            _mockMapper.Setup(m => m.Map<Ecard>(It.IsAny<EcardListModel>())).Returns(new Ecard());

            var result = await _service.ImportECard(1, stream, "user");

            Assert.Contains("1 Created successfully", result);
            _mockUow.Verify(u => u.EcardRepository.Add(It.IsAny<Ecard>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task DownloadTemplate_ShouldReturnByteArray()
        {
            _mockEruleMasterService.Setup(s => s.GetAll(1)).ReturnsAsync(new List<EruleMasterListModel>());

            var result = await _service.DownloadTemplate(1);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task ImportECard_EmptyExcel_ShouldReturnErrorMessage()
        {
            using var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var ws = package.Workbook.Worksheets.Add("Cards");
                // Adding something to row 1 to ensure Dimension is not null
                ws.Cells[1, 1].Value = "CardName";
                ws.Cells[1, 2].Value = "CardDescription";
                ws.Cells[1, 3].Value = "ExpressionShown";
                // Row 2 is empty/missing, so GetRowCount should return 0
                package.Save();
            }
            stream.Position = 0;

            var result = await _service.ImportECard(1, stream, "user");
            Assert.Equal("Uploaded File Is Empty", result);
        }

        [Fact]
        public async Task ImportECard_InvalidExpression_ShouldSkip()
        {
            _mockEruleMasterRepo.Setup(u => u.Query()).Returns(new List<EruleMaster>().AsQueryable().BuildMock());

            using var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var ws = package.Workbook.Worksheets.Add("Cards");
                ws.Cells[1, 1].Value = "CardName";
                ws.Cells[1, 2].Value = "CardDescription";
                ws.Cells[1, 3].Value = "ExpressionShown";
                ws.Cells[2, 1].Value = "C1";
                ws.Cells[2, 2].Value = "Desc";
                ws.Cells[2, 3].Value = "NonExistentRule";
                package.Save();
            }
            stream.Position = 0;

            var result = await _service.ImportECard(1, stream, "user");
            Assert.Contains("invalid rows skipped", result);
        }

        [Fact]
        public async Task RemoveMultiple_ShouldBulkDelete()
        {
            var cards = new List<Ecard> { new Ecard { EcardId = 1, EcardName = "C1", TenantId = 1 } };
            _mockEcardRepo.Setup(u => u.GetById(1)).Returns(cards[0]);
            _mockPcardRepo.Setup(u => u.Query()).Returns(new List<Pcard>().AsQueryable().BuildMock());

            var result = await _service.RemoveMultiple(1, new List<int> { 1 });
            Assert.Contains("ECards Deleted successfully", result);
            _mockEcardRepo.Verify(u => u.Remove(It.IsAny<Ecard>()), Times.Once);
        }

        [Fact]
        public async Task Update_ValidModel_ShouldUpdate()
        {
            var card = new Ecard { EcardId = 1, EcardName = "Old", TenantId = 1 };
            _mockEcardRepo.Setup(u => u.Query()).Returns(new List<Ecard> { card }.AsQueryable().BuildMock());

            var model = new EcardUpdateModel { EcardId = 1, EcardName = "New", TenantId = 1 };
            _mockMapper.Setup(m => m.Map<EcardUpdateModel, Ecard>(model, card))
                .Callback<EcardUpdateModel, Ecard>((src, dest) => dest.EcardName = src.EcardName ?? "")
                .Returns(card);

            await _service.Update(model);

            Assert.Equal("New", card.EcardName);
            _mockEcardRepo.Verify(u => u.Update(card), Times.Once);
        }

        [Fact]
        public async Task ExportECard_WithSearch_ShouldReturnStream()
        {
            var cards = new List<Ecard> 
            { 
                new Ecard { EcardId = 1, TenantId = 1, EcardName = "SearchTest" } 
            }.AsQueryable().BuildMock();

            _mockEcardRepo.Setup(u => u.Query()).Returns(cards);
            _mockMapper.Setup(m => m.Map<List<EcardModelDescription>>(It.IsAny<object>()))
                .Returns(new List<EcardModelDescription> { new EcardModelDescription { EcardId = 1, Expression = "EXP" } });

            _mockExportService.Setup(s => s.ExportToExcel(It.IsAny<List<EcardModelDescription>>(), "Ecards", It.IsAny<string[]>()))
                .ReturnsAsync(new MemoryStream());

            var request = new ExportRequestModel { SearchTerm = "search" };
            var result = await _service.ExportECard(1, request);

            Assert.NotNull(result);
            _mockExportService.Verify(s => s.ExportToExcel(It.Is<List<EcardModelDescription>>(l => l.Count == 1), "Ecards", It.IsAny<string[]>()), Times.Once);
        }

        [Fact]
        public async Task RemoveMultiple_SomeInUse_ShouldNotDeleteAll()
        {
            var cards = new List<Ecard> 
            { 
                new Ecard { EcardId = 1, EcardName = "InUse", TenantId = 1 },
                new Ecard { EcardId = 2, EcardName = "Free", TenantId = 1 }
            };
            _mockEcardRepo.Setup(u => u.GetById(1)).Returns(cards[0]);
            _mockEcardRepo.Setup(u => u.GetById(2)).Returns(cards[1]);

            var pCards = new List<Pcard> { new Pcard { Expression = "1", TenantId = 1 } }.AsQueryable().BuildMock();
            _mockPcardRepo.Setup(u => u.Query()).Returns(pCards);

            var result = await _service.RemoveMultiple(1, new List<int> { 1, 2 });
            
            Assert.Contains("could not be deleted", result);
            Assert.Contains("InUse", result);
            _mockEcardRepo.Verify(u => u.Remove(It.Is<Ecard>(c => c.EcardId == 1)), Times.Never);
            _mockEcardRepo.Verify(u => u.Remove(It.Is<Ecard>(c => c.EcardId == 2)), Times.Once);
        }

        [Fact]
        public async Task ImportECard_DuplicateAndInvalidRows_ShouldIncrementCounters()
        {
            var existing = new List<Ecard> { new Ecard { TenantId = 1, EcardName = "Dupe", EcardDesc = "Desc", Expression = "( 1 )" } }.AsQueryable().BuildMock();
            _mockEcardRepo.Setup(u => u.Query()).Returns(existing);
            var master = new EruleMaster { Id = 1, EruleName = "R1", TenantId = 1 };
            _mockEruleMasterRepo.Setup(u => u.Query()).Returns(new List<EruleMaster> { master }.AsQueryable().BuildMock());
            _mockEcardRepo.Setup(u => u.Add(It.IsAny<Ecard>(), It.IsAny<bool>()));
            
            // Mock Erule for BuildExpressionFromShown (queries EruleRepository)
            var rule = new Erule { EruleId = 1, EruleMasterId = 1, IsPublished = true, TenantId = 1, Expression = "E", EruleMaster = master };
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule }.AsQueryable().BuildMock());
            
            _mockMapper.Setup(m => m.Map<Ecard>(It.IsAny<EcardListModel>())).Returns(new Ecard());

            using var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var ws = package.Workbook.Worksheets.Add("Cards");
                ws.Cells[1, 1].Value = "CardName"; ws.Cells[1, 2].Value = "CardDescription"; ws.Cells[1, 3].Value = "ExpressionShown";
                // Row 2: Duplicate
                ws.Cells[2, 1].Value = "Dupe"; ws.Cells[2, 2].Value = "Desc"; ws.Cells[2, 3].Value = "R1";
                // Row 3: Invalid (missing rule)
                ws.Cells[3, 1].Value = "New"; ws.Cells[3, 2].Value = "Desc"; ws.Cells[3, 3].Value = "MissingRule";
                // Row 4: Empty Name
                ws.Cells[4, 1].Value = ""; ws.Cells[4, 2].Value = "Desc"; ws.Cells[4, 3].Value = "R1";
                package.Save();
            }
            stream.Position = 0;

            var result = await _service.ImportECard(1, stream, "user");
            Assert.Contains("1 duplicates skipped", result);
            Assert.Contains("2 invalid rows skipped", result);
        }

        [Fact]
        public async Task Add_DuplicateName_ShouldThrowException()
        {
            var model = new EcardAddUpdateModel { EcardName = "Existing", TenantId = 1 };
            _mockEcardRepo.Setup(u => u.Query()).Returns(new List<Ecard> { new Ecard { EcardName = "Existing", TenantId = 1 } }.AsQueryable().BuildMock());

            await Assert.ThrowsAsync<Exception>(() => _service.Add(1, model));
        }

        [Fact]
        public async Task Update_DuplicateName_ShouldThrowException()
        {
            var model = new EcardUpdateModel { EcardId = 1, EcardName = "Existing", TenantId = 1 };
            _mockEcardRepo.Setup(u => u.Query()).Returns(new List<Ecard> { new Ecard { EcardId = 2, EcardName = "Existing", TenantId = 1 } }.AsQueryable().BuildMock());

            await Assert.ThrowsAsync<Exception>(() => _service.Update(model));
        }

        [Fact]
        public async Task Delete_RepoThrows_ShouldReturnMessage()
        {
            _mockPcardRepo.Setup(u => u.Query()).Returns(new List<Pcard>().AsQueryable().BuildMock());
            _mockEcardRepo.Setup(u => u.Query()).Throws(new Exception("Repo Error"));

            var result = await _service.Delete(1, 1);
            Assert.Equal("Repo Error", result);
        }

        [Fact]
        public async Task ImportECard_EmptyFile_ShouldReturnError()
        {
            using var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var ws = package.Workbook.Worksheets.Add("Empty");
                ws.Cells[1, 1].Value = "Header";
                package.Save();
            }
            stream.Position = 0;

            var result = await _service.ImportECard(1, stream, "user");
            Assert.Equal("Uploaded File Is Empty", result);
        }

        [Fact]
        public async Task DownloadTemplate_ShouldReturnBytes()
        {
            _mockEruleMasterService.Setup(s => s.GetAll(It.IsAny<int>())).ReturnsAsync(new List<EruleMasterListModel>());
            var result = await _service.DownloadTemplate(1);
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void GetById_NotFound_ShouldThrowException()
        {
            _mockEcardRepo.Setup(u => u.Query()).Returns(new List<Ecard>().AsQueryable().BuildMock());
            Assert.Throws<InvalidOperationException>(() => _service.GetById(1, 999));
        }

        [Fact]
        public async Task ExportECard_WithSelection_ShouldReturnStream()
        {
            var request = new ExportRequestModel { SelectedIds = new List<int> { 1 } };
            _mockEcardRepo.Setup(u => u.Query()).Returns(new List<Ecard> { new Ecard { EcardId = 1, TenantId = 1, EcardName = "N", Expression = "E" } }.AsQueryable().BuildMock());
            _mockExportService.Setup(s => s.ExportToExcel(It.IsAny<List<EcardModelDescription>>(), It.IsAny<string>(), It.IsAny<string[]>())).ReturnsAsync(new MemoryStream());

            var result = await _service.ExportECard(1, request);
            Assert.NotNull(result);
        }
        [Fact]
        public async Task ImportECard_InvalidExpression_ShouldIncrementCounter()
        {
            _mockEruleMasterRepo.Setup(u => u.Query()).Returns(new List<EruleMaster>().AsQueryable().BuildMock()); // Causes expression to be null
            using var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var ws = package.Workbook.Worksheets.Add("Cards");
                ws.Cells[1, 1].Value = "CardName*"; ws.Cells[1, 2].Value = "CardDescription"; ws.Cells[1, 3].Value = "ExpressionShown";
                ws.Cells[2, 1].Value = "C1"; ws.Cells[2, 2].Value = "D1"; ws.Cells[2, 3].Value = "MissingRule";
                package.Save();
            }
            stream.Position = 0;

            var result = await _service.ImportECard(1, stream, "user");
            Assert.Contains("1 invalid rows skipped", result);
        }
        [Fact]
        public async Task ImportECard_ComplexExpression_ShouldSucceed()
        {
            var master1 = new EruleMaster { Id = 1, EruleName = "R1", TenantId = 1 };
            var master2 = new EruleMaster { Id = 2, EruleName = "R2", TenantId = 1 };
            _mockEruleMasterRepo.Setup(u => u.Query()).Returns(new List<EruleMaster> { master1, master2 }.AsQueryable().BuildMock());
            _mockEruleRepo.Setup(u => u.Query()).Returns(new List<Erule> { new Erule { EruleId = 101, EruleMasterId = 1 }, new Erule { EruleId = 102, EruleMasterId = 2 } }.AsQueryable().BuildMock());
            _mockEcardRepo.Setup(u => u.Query()).Returns(new List<Ecard>().AsQueryable().BuildMock());

            using var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var ws = package.Workbook.Worksheets.Add("Cards");
                ws.Cells[1, 1].Value = "CardName*"; ws.Cells[1, 2].Value = "CardDescription"; ws.Cells[1, 3].Value = "ExpressionShown";
                ws.Cells[2, 1].Value = "C1"; ws.Cells[2, 2].Value = "D1"; ws.Cells[2, 3].Value = "(R1 AND R2)";
                package.Save();
            }
            stream.Position = 0;

            _mockMapper.Setup(m => m.Map<Ecard>(It.IsAny<EcardListModel>())).Returns(new Ecard());

            var result = await _service.ImportECard(1, stream, "user");
            Assert.Contains("1 Created successfully", result);
        }

        [Fact]
        public async Task DownloadTemplate_WithRules_ShouldReturnBytes()
        {
            var rules = new List<EruleMasterListModel> { new() { EruleId = 1, EruleName = "R1" } };
            _mockEruleMasterService.Setup(s => s.GetAll(1)).ReturnsAsync(rules);

            var result = await _service.DownloadTemplate(1);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task RemoveMultiple_NoIds_ShouldReturnNoEcardMessage()
        {
            var result = await _service.RemoveMultiple(1, new List<int>());
            Assert.Equal("No ECards were deleted.", result);
        }
        [Fact]
        public async Task RemoveMultiple_NonExistentId_ShouldSkip()
        {
            _mockEcardRepo.Setup(u => u.GetById(999)).Returns((Ecard?)null);
            _mockPcardRepo.Setup(u => u.Query()).Returns(new List<Pcard>().AsQueryable().BuildMock());

            var result = await _service.RemoveMultiple(1, [999]);

            Assert.Equal("No ECards were deleted.", result);
        }

        [Fact]
        public async Task ImportECard_MissingRules_ShouldSkip()
        {
            var master = new EruleMaster { Id = 1, EruleName = "R1", TenantId = 1 };
            _mockEruleMasterRepo.Setup(u => u.Query()).Returns(new List<EruleMaster> { master }.AsQueryable().BuildMock());
            // Trying to import (R1 AND R2) but only R1 exists

            using var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var ws = package.Workbook.Worksheets.Add("Cards");
                ws.Cells[1, 1].Value = "CardName*"; ws.Cells[1, 2].Value = "CardDescription"; ws.Cells[1, 3].Value = "ExpressionShown";
                ws.Cells[2, 1].Value = "C1"; ws.Cells[2, 2].Value = "D1"; ws.Cells[2, 3].Value = "(R1 AND R2)";
                package.Save();
            }
            stream.Position = 0;

            var result = await _service.ImportECard(1, stream, "user");
            Assert.Contains("1 invalid rows", result);
        }

        [Fact]
        public async Task RemoveMultiple_RepoThrows_ShouldReturnErrorMessage()
        {
            _mockEcardRepo.Setup(u => u.GetById(It.IsAny<int>())).Throws(new Exception("DB Error"));
            var result = await _service.RemoveMultiple(1, [1]);
            Assert.Equal("DB Error", result);
        }

        [Fact]
        public async Task ImportECard_RepoThrows_ShouldReturnErrorMessage()
        {
            _mockEcardRepo.Setup(u => u.Query()).Returns(new List<Ecard>().AsQueryable().BuildMock());
            _mockUow.Setup(u => u.CompleteAsync()).Throws(new Exception("Completion Error"));

            using var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var ws = package.Workbook.Worksheets.Add("Cards");
                ws.Cells[1, 1].Value = "H1"; ws.Cells[1, 2].Value = "H2"; ws.Cells[1, 3].Value = "H3";
                ws.Cells[2, 1].Value = "C1"; ws.Cells[2, 2].Value = "D1"; ws.Cells[2, 3].Value = "R1";
                package.Save();
            }
            stream.Position = 0;

            // Mock successful rule lookup for R1
            var master = new EruleMaster { Id = 1, EruleName = "R1", TenantId = 1 };
            _mockEruleMasterRepo.Setup(u => u.Query()).Returns(new List<EruleMaster> { master }.AsQueryable().BuildMock());
            _mockEruleRepo.Setup(u => u.Query()).Returns(new List<Erule> { new Erule { EruleId = 1, EruleMasterId = 1, Version = 1 } }.AsQueryable().BuildMock());
            _mockMapper.Setup(m => m.Map<Ecard>(It.IsAny<EcardListModel>())).Returns(new Ecard());

            var result = await _service.ImportECard(1, stream, "user");
            Assert.Contains("Completion Error", result);
        }
        [Fact]
        public async Task RemoveMultiple_MixedCase_ShouldDeleteSomeAndReturnMessageForOthers()
        {
            var pcardData = new List<Pcard> { new() { TenantId = 1, Expression = "\b1\b" } }.BuildMock();
            var ecard1 = new Ecard { EcardId = 1, EcardName = "UsedCard", TenantId = 1 };
            var ecard2 = new Ecard { EcardId = 2, EcardName = "FreeCard", TenantId = 1 };

            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(pcardData);
            _mockEcardRepo.Setup(u => u.GetById(1)).Returns(ecard1);
            _mockEcardRepo.Setup(u => u.GetById(2)).Returns(ecard2);

            var result = await _service.RemoveMultiple(1, [1, 2]);

            Assert.Contains("UsedCard", result);
            Assert.Contains("1  ECards Deleted", result);
            _mockEcardRepo.Verify(u => u.Remove(ecard2), Times.Once);
            _mockEcardRepo.Verify(u => u.Remove(ecard1), Times.Never);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task ExportECard_WithSearch_ShouldFilterResults()
        {
            var request = new ExportRequestModel { SearchTerm = "Match" };
            var data = new List<Ecard> 
            { 
                new() { EcardId = 1, EcardName = "matchthis", TenantId = 1 },
                new() { EcardId = 2, EcardName = "other", TenantId = 1 }
            }.BuildMock();

            _mockEcardRepo.Setup(u => u.Query()).Returns(data);
            _mockMapper.Setup(m => m.Map<List<EcardModelDescription>>(It.IsAny<object>()))
                .Returns((List<Ecard> src) => src.Select(s => new EcardModelDescription { EcardId = s.EcardId }).ToList());
            _mockExportService.Setup(e => e.ExportToExcel(It.IsAny<List<EcardModelDescription>>(), "Ecards", It.IsAny<string[]>()))
                .ReturnsAsync(new MemoryStream());

            var result = await _service.ExportECard(1, request);

            Assert.NotNull(result);
            _mockExportService.Verify(e => e.ExportToExcel(It.Is<List<EcardModelDescription>>(l => l.Count == 1), "Ecards", It.IsAny<string[]>()), Times.Once);
        }

        [Fact]
        public async Task ExportECard_WithSelection_ShouldFilterResults()
        {
            var request = new ExportRequestModel { SelectedIds = [1] };
            var data = new List<Ecard> 
            { 
                new() { EcardId = 1, EcardName = "C1", TenantId = 1 },
                new() { EcardId = 2, EcardName = "C2", TenantId = 1 }
            }.BuildMock();

            _mockEcardRepo.Setup(u => u.Query()).Returns(data);
            _mockExportService.Setup(e => e.ExportToExcel(It.IsAny<List<EcardModelDescription>>(), "Ecards", It.IsAny<string[]>()))
                .ReturnsAsync(new MemoryStream());

            var result = await _service.ExportECard(1, request);

            Assert.NotNull(result);
            _mockExportService.Verify(e => e.ExportToExcel(It.Is<List<EcardModelDescription>>(l => l.Count == 1), "Ecards", It.IsAny<string[]>()), Times.Once);
        }

        [Fact]
        public async Task ImportECard_EmptyFile_ShouldReturnErrorMessage()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Cards");
            var stream = new MemoryStream(package.GetAsByteArray());

            var result = await _service.ImportECard(1, stream, "user");

            Assert.Equal("Uploaded File Is Empty", result);
        }

        [Fact]
        public async Task ImportECard_InvalidExpression_ShouldReturnErrorMessage()
        {
            // BuildExpressionFromShown will return null because EruleMaster doesn't exist
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster>().BuildMock());

            using var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var ws = package.Workbook.Worksheets.Add("Cards");
                ws.Cells[1, 1].Value = "CardName"; ws.Cells[1, 2].Value = "Desc"; ws.Cells[1, 3].Value = "Expr";
                ws.Cells[2, 1].Value = "C1"; ws.Cells[2, 2].Value = "D1"; ws.Cells[2, 3].Value = "NonExistentRule";
                package.Save();
            }
            stream.Position = 0;

            var result = await _service.ImportECard(1, stream, "user");

            Assert.Contains("Invalid ExpressionShown", result);
        }

        [Fact]
        public void PrivateHelpers_ShouldApplyDropdownAndPopulateEmpty()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Cards");
            sheet.Cells[1, 1].Value = "H";

            var populate = typeof(EcardService).GetMethod("PopulateColumn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var applyDropdown = typeof(EcardService).GetMethod("ApplyDropdown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            Assert.NotNull(populate);
            Assert.NotNull(applyDropdown);

            populate!.Invoke(null, new object[] { sheet, Array.Empty<string>(), 3 });
            applyDropdown!.Invoke(null, new object[] { sheet, "TestRange", "A", 1, 0 });

            Assert.Equal(string.Empty, sheet.Cells[2, 3].Text);
            Assert.NotNull(package.Workbook.Names["TestRange"]);
        }

        [Fact]
        public async Task BuildExpressionFromShown_EmptyExpression_ShouldReturnNull()
        {
            var method = typeof(EcardService).GetMethod("BuildExpressionFromShown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);

            var task = (Task<string?>)method!.Invoke(null, new object[] { "", _mockUow.Object })!;
            var result = await task;

            Assert.Null(result);
        }
    }
}
