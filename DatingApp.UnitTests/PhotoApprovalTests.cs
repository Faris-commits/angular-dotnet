using API.Data;
using API.Entities;
using API.Interfaces;
using API.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DatingApp.Unit.Tests
{
    public class PhotoApprovalTests
    {
        private readonly DataContext _context;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IPhotoService> _photoServiceMock = new();
        private readonly Mock<ILogger<AdminService>> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();

        private readonly AdminService _adminService;

        public PhotoApprovalTests()
        {
            // Create in-memory EF Core DataContext for testing
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new DataContext(options);

            _adminService = new AdminService(
                _unitOfWorkMock.Object,
                _photoServiceMock.Object,
                _loggerMock.Object,
                _mapperMock.Object,
                _context  // Pass real context here!
            );
        }

        [Fact]
        public async Task ApprovePhotoAsync_ShouldApprovePhoto_WhenValidPhotoExists()
        {
            // Arrange
            var photo = new Photo
            {
                Id = 1,
                Url = "http://example.com/photo.jpg",
                IsApproved = false,
                IsMain = false,
                AppUserId = 2
            };

            var user = new AppUser
            {
                Id = 2,
                KnownAs = "TestUser",
                Gender = "male",
                City = "TestCity",
                Country = "TestCountry",
                Photos = new List<Photo> { photo }
            };

            _unitOfWorkMock.Setup(u => u.PhotoRepository.GetPhotoById(1)).ReturnsAsync(photo);
            _unitOfWorkMock.Setup(u => u.UserRepository.GetUserByPhotoId(1)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.Complete()).ReturnsAsync(true);

            // Act
            var (success, message) = await _adminService.ApprovePhotoAsync(1);

            // Assert
            Assert.True(success);
            Assert.Equal("Photo approved successfully.", message);
            Assert.True(photo.IsApproved);
        }

        [Fact]
        public async Task ApprovePhotoAsync_ShouldSetAsMain_WhenUserHasNoMainPhoto()
        {
            // Arrange
            var photo = new Photo
            {
                Id = 1,
                Url = "http://example.com/photo.jpg",
                IsApproved = false,
                IsMain = false,
                AppUserId = 2
            };

            var user = new AppUser
            {
                Id = 2,
                KnownAs = "TestUser",
                Gender = "female",
                City = "TestCity",
                Country = "TestCountry",
                Photos = new List<Photo> { photo } // no main photo
            };

            _unitOfWorkMock.Setup(u => u.PhotoRepository.GetPhotoById(1)).ReturnsAsync(photo);
            _unitOfWorkMock.Setup(u => u.UserRepository.GetUserByPhotoId(1)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.Complete()).ReturnsAsync(true);

            // Act
            var (success, message) = await _adminService.ApprovePhotoAsync(1);

            // Assert
            Assert.True(success);
            Assert.True(photo.IsMain);
            Assert.Equal("Photo approved successfully.", message);
        }

        [Fact]
        public async Task ApprovePhotoAsync_ShouldReturnFalse_WhenPhotoNotFound()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.PhotoRepository.GetPhotoById(1)).ReturnsAsync((Photo?)null);

            // Act
            var (success, message) = await _adminService.ApprovePhotoAsync(1);

            // Assert
            Assert.False(success);
            Assert.Equal("Photo not found.", message);
        }

        [Fact]
        public async Task ApprovePhotoAsync_ShouldReturnFalse_WhenUserNotFound()
        {
            // Arrange
            var photo = new Photo
            {
                Id = 1,
                Url = "http://example.com/photo.jpg",
                IsApproved = false,
                IsMain = false,
                AppUserId = 2
            };

            _unitOfWorkMock.Setup(u => u.PhotoRepository.GetPhotoById(1)).ReturnsAsync(photo);
            _unitOfWorkMock.Setup(u => u.UserRepository.GetUserByPhotoId(1)).ReturnsAsync((AppUser?)null);

            // Act
            var (success, message) = await _adminService.ApprovePhotoAsync(1);

            // Assert
            Assert.False(success);
            Assert.Equal("User not found.", message);
        }

        [Fact]
        public async Task ApprovePhotoAsync_ShouldReturnFalse_WhenSaveFails()
        {
            // Arrange
            var photo = new Photo
            {
                Id = 1,
                Url = "http://example.com/photo.jpg",
                IsApproved = false,
                IsMain = false,
                AppUserId = 2
            };

            var user = new AppUser
            {
                Id = 2,
                KnownAs = "TestUser",
                Gender = "male",
                City = "TestCity",
                Country = "TestCountry",
                Photos = new List<Photo> { photo }
            };

            _unitOfWorkMock.Setup(u => u.PhotoRepository.GetPhotoById(1)).ReturnsAsync(photo);
            _unitOfWorkMock.Setup(u => u.UserRepository.GetUserByPhotoId(1)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.Complete()).ReturnsAsync(false);

            // Act
            var (success, message) = await _adminService.ApprovePhotoAsync(1);

            // Assert
            Assert.False(success);
            Assert.Equal("Failed to approve photo.", message);
        }
    }
}
