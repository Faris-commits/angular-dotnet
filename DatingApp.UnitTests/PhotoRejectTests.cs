using API.Entities;
using API.Interfaces;
using API.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CloudinaryDotNet.Actions;

namespace DatingApp.Unit.Tests
{
    public class PhotoRejectTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IPhotoService> _photoServiceMock = new();
        private readonly Mock<ILogger<AdminService>> _loggerMock = new();

        private readonly AdminService _adminService;

        public PhotoRejectTests()
        {
            _adminService = new AdminService(
                _unitOfWorkMock.Object,
                _photoServiceMock.Object,
                _loggerMock.Object,
                null!,
                null!
            );
        }

        [Fact]
        public async Task RejectPhotoAsync_ShouldReturnFalse_WhenPhotoNotFound()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.PhotoRepository.GetPhotoById(It.IsAny<int>()))
                .ReturnsAsync((Photo?)null);

            // Act
            var result = await _adminService.RejectPhotoAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Photo not found.", result.Message);
        }

        [Fact]
        public async Task RejectPhotoAsync_ShouldDeletePhotoFromCloudinary_AndRemovePhoto_WhenPublicIdExists()
        {
            // Arrange
            var photo = new Photo
            {
                Id = 1,
                Url = "http://photo.jpg",
                PublicId = "publicId123",
                IsApproved = true,
                AppUser = new AppUser
                {
                    KnownAs = "Test",
                    Gender = "male",
                    City = "City",
                    Country = "Country"
                }
            };

            _unitOfWorkMock.Setup(u => u.PhotoRepository.GetPhotoById(1))
                .ReturnsAsync(photo);

            _photoServiceMock.Setup(p => p.DeletePhotoAsync("publicId123"))
                .ReturnsAsync(new DeletionResult { StatusCode = System.Net.HttpStatusCode.OK });

            _unitOfWorkMock.Setup(u => u.Complete())
                .ReturnsAsync(true);

            // Act
            var result = await _adminService.RejectPhotoAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Photo rejected and deleted successfully.", result.Message);
            _photoServiceMock.Verify(p => p.DeletePhotoAsync("publicId123"), Times.Once);
            _unitOfWorkMock.Verify(u => u.PhotoRepository.RemovePhoto(photo), Times.Once);
            _unitOfWorkMock.Verify(u => u.Complete(), Times.Once);
        }

        [Fact]
        public async Task RejectPhotoAsync_ShouldRemovePhotoAndComplete_WhenNoPublicId()
        {
            // Arrange
            var photo = new Photo
            {
                Id = 2,
                Url = "http://photo2.jpg",
                PublicId = null,
                IsApproved = false,
                AppUser = new AppUser
                {
                    KnownAs = "Test2",
                    Gender = "female",
                    City = "City2",
                    Country = "Country2"
                }
            };

            _unitOfWorkMock.Setup(u => u.PhotoRepository.GetPhotoById(2))
                .ReturnsAsync(photo);

            _unitOfWorkMock.Setup(u => u.Complete())
                .ReturnsAsync(true);

            // Act
            var result = await _adminService.RejectPhotoAsync(2);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Photo rejected and deleted successfully.", result.Message);
            _photoServiceMock.Verify(p => p.DeletePhotoAsync(It.IsAny<string>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.PhotoRepository.RemovePhoto(photo), Times.Once);
            _unitOfWorkMock.Verify(u => u.Complete(), Times.Once);
        }

        [Fact]
        public async Task RejectPhotoAsync_ShouldReturnFalse_WhenCompleteFails()
        {
            // Arrange
            var photo = new Photo
            {
                Id = 3,
                Url = "http://photo3.jpg",
                PublicId = null,
                IsApproved = false,
                AppUser = new AppUser
                {
                    KnownAs = "Test3",
                    Gender = "male",
                    City = "City3",
                    Country = "Country3"
                }
            };

            _unitOfWorkMock.Setup(u => u.PhotoRepository.GetPhotoById(3))
                .ReturnsAsync(photo);

            _unitOfWorkMock.Setup(u => u.Complete())
                .ReturnsAsync(false);

            // Act
            var result = await _adminService.RejectPhotoAsync(3);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to reject photo.", result.Message);
            _unitOfWorkMock.Verify(u => u.PhotoRepository.RemovePhoto(photo), Times.Once);
        }
    }
}
