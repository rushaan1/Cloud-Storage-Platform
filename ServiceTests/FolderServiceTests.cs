using AutoFixture;
using Castle.Core.Configuration;
using CloudStoragePlatform.Core.Domain.Entities;
using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.DTO;
using CloudStoragePlatform.Core.Exceptions;
using CloudStoragePlatform.Core.ServiceContracts;
using CloudStoragePlatform.Core.Services;
using FluentAssertions;
using Moq;
using Org.BouncyCastle.Asn1.X509;
using Xunit;
namespace ServiceTests
{
    public class FolderServiceTests
    {
        private readonly IFoldersModificationService _foldersModificationService;
        private readonly IFoldersRetrievalService _foldersRetrievalService;

        private readonly IFixture _fixture;
        private readonly Mock<IFoldersRepository> _foldersRepositoryMock;
        string initialPath = @"C:\CloudStoragePlatformUnitTests\home";

        public FolderServiceTests() 
        {
            _fixture = new Fixture();
            //Limiting every single string generated by _fixture to have only <=50 characters which are valid windows directory names
            _fixture.Customize<string>(c => c.FromFactory(() => string.Concat(new string(Enumerable.Range(0, 50).Select(x => _fixture.Create<char>()).ToArray()).Split(Path.GetInvalidFileNameChars()))));

            _foldersRepositoryMock = new Mock<IFoldersRepository>();
            _foldersModificationService = new FoldersModificationService(_foldersRepositoryMock.Object);
            _foldersRetrievalService = new FoldersRetrievalService(_foldersRepositoryMock.Object);
        }

        #region FolderModificationService

        #region AddFolder
        [Fact]
        public async Task AddFolder_DuplicateFolder()
        {
            string newFolderName = _fixture.Create<string>();
            FolderAddRequest? folderAddRequest = new FolderAddRequest() { FolderName = newFolderName, FolderPath = Path.Combine(initialPath, newFolderName) };
            
            Directory.CreateDirectory(folderAddRequest.FolderPath);

            try
            {
                Func<Task> action = async () =>
                {
                    await _foldersModificationService.AddFolder(folderAddRequest);
                };
                await action.Should().ThrowAsync<DuplicateFolderException>();
            }
            finally 
            {
                Directory.Delete(folderAddRequest.FolderPath);
            }
        }


        [Fact]
        public async Task AddFolder_InvalidAddRequestPath()
        {
            string newFolderName = _fixture.Create<string>();
            FolderAddRequest? folderAddRequest = new FolderAddRequest() { FolderName = newFolderName, FolderPath = Path.Combine(_fixture.Create<string>(), newFolderName) };

            Func<Task> action = async () =>
            {
                await _foldersModificationService.AddFolder(folderAddRequest);
            };

            await action.Should().ThrowAsync<ArgumentException>();
        }

        // PLEASE USE DIRECTORY IN THE SERVICE TO CHECK IF THE PATH IS VALID

        [Fact]
        public async Task AddFolder_CorrectDetails()
        {
            string newFolderName = _fixture.Create<string>();
            FolderAddRequest? folderAddRequest = new FolderAddRequest() { FolderName = newFolderName, FolderPath = Path.Combine(initialPath, newFolderName) };
            Folder folder = new Folder() { FolderName = folderAddRequest.FolderName, FolderPath = folderAddRequest.FolderPath };

            _foldersRepositoryMock.Setup(f => f.AddFolder(It.IsAny<Folder>()))
                .ReturnsAsync(folder);

            FolderResponse folderResponse = await _foldersModificationService.AddFolder(folderAddRequest);
            bool folderExists = Directory.Exists(folderResponse.FolderPath);

            folderResponse.FolderId.Should().NotBeEmpty();
            folderResponse.FolderName.Should().Be(folder.FolderName);
            folderResponse.FolderPath.Should().Be(folder.FolderPath);
            folderExists.Should().BeTrue();
        }
        #endregion



        #region RenameFolder
        [Fact]
        public async Task RenameFolder_FolderDoesNotExists() 
        {
            FolderRenameRequest renameRequest = _fixture.Create<FolderRenameRequest>();

            _foldersRepositoryMock.Setup(f => f.GetFolderByFolderId(It.IsAny<Guid>()))
                .ReturnsAsync((Folder)null!);

            Func<Task> action = async () =>
            {
                await _foldersModificationService.RenameFolder(renameRequest);
            };

            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task RenameFolder_NewFolderNameAlreadyExists() 
        {
            string newFolderName = _fixture.Create<string>();
            string pathOfExistingFolderWithNewFolderName = Path.Combine(initialPath, newFolderName);



            string folderToBeRenamedsName = _fixture.Create<string>();
            string folderToBeRenamedsPath = Path.Combine(initialPath, folderToBeRenamedsName);

            FolderRenameRequest renameRequest = _fixture.Build<FolderRenameRequest>().With(frr=>frr.FolderNewName, newFolderName).Create(); 

            Folder folderToBeRenamed = new Folder() { FolderId = renameRequest.FolderId, FolderName = folderToBeRenamedsName, FolderPath = folderToBeRenamedsPath };

            Directory.CreateDirectory(pathOfExistingFolderWithNewFolderName);

            _foldersRepositoryMock.Setup(f => f.GetFolderByFolderId(It.IsAny<Guid>()))
                .ReturnsAsync(folderToBeRenamed);

            try
            {
                Func<Task> action = async () =>
                {
                    await _foldersModificationService.RenameFolder(renameRequest);
                };
                await action.Should().ThrowAsync<InvalidOperationException>();
            }
            finally 
            {
                Directory.Delete(pathOfExistingFolderWithNewFolderName);
            }
        }

        [Fact]
        public async Task RenameFolder_SuccessfulRename() 
        {
            //Arrange
            string folderName = _fixture.Create<string>();
            string folderPath = Path.Combine(initialPath, folderName);

            FolderRenameRequest renameRequest = _fixture.Create<FolderRenameRequest>();// IF ERROR COMES HERE REGARDING INVALID DIRECTORY NAME IT MIGHT BE BECAUSE FIXTURE ISN'T RESPECTING THE CUSTOMIZATION OF STRING GENERATION WHEN GENERATING STRING PROPERTIES OF OBJECT
            Folder folder = new Folder() { FolderId = renameRequest.FolderId, FolderName = folderName, FolderPath = folderPath };
            Directory.CreateDirectory(folderPath);

            _foldersRepositoryMock.Setup(f => f.GetFolderByFolderId(It.IsAny<Guid>()))
                .ReturnsAsync(folder);
            // The service will check using Directory to see if folder with new name already exists 

            //Act
            FolderResponse response = await _foldersModificationService.RenameFolder(renameRequest);

            //Assert
            bool folderExists = Directory.Exists(Path.Combine(initialPath, renameRequest.FolderNewName));
            response.FolderId.Should().Be(renameRequest.FolderId);
            response.FolderName.Should().Be(renameRequest.FolderNewName);
            folderExists.Should().BeTrue();
            Directory.Delete(response.FolderPath);
        }
        #endregion



        #region MoveFolder
        [Fact]
        public async Task MoveFolder_FolderDoesntExists() 
        {
            //Arrange
            Guid guid = _fixture.Create<Guid>();

            string newFolderPath = _fixture.Create<string>();

            _foldersRepositoryMock.Setup(f=>f.GetFolderByFolderId(It.IsAny<Guid>()))
                .ReturnsAsync((Folder) null!);

            //Act
            Func<Task> action = async () =>
            {
                await _foldersModificationService.MoveFolder(guid, newFolderPath);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }



        [Fact]
        public async Task MoveFolder_InvalidNewFolderPath() 
        {
            //Arrange
            Guid guid = _fixture.Create<Guid>();
            string folderPath = _fixture.Create<string>();
            Folder folder = new Folder() { FolderId = guid, FolderPath = folderPath };

            string newFolderPath = _fixture.Create<string>();

            _foldersRepositoryMock.Setup(f => f.GetFolderByFolderId(It.IsAny<Guid>()))
                .ReturnsAsync(folder);

            //Act
            Func<Task> action = async () =>
            {
                await _foldersModificationService.MoveFolder(guid, newFolderPath);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }



        [Fact]
        public async Task MoveFolder_SuccessfullyMoved() 
        {
            //Arrange
            Guid guid = _fixture.Create<Guid>();
            string folderName = _fixture.Create<string>();
            string folderPath = Path.Combine(initialPath, folderName);
            Folder folder = new Folder() { FolderId = guid, FolderName = folderName, FolderPath = folderPath };
            string destinationDirectoryPath = Path.Combine(initialPath, _fixture.Create<string>());
            string newPathForFOLDER = Path.Combine(destinationDirectoryPath, folderName);

            Directory.CreateDirectory(folderPath);
            Directory.CreateDirectory(destinationDirectoryPath);

            _foldersRepositoryMock.Setup(f => f.GetFolderByFolderId(It.IsAny<Guid>()))
                .ReturnsAsync(folder);

            try
            {
                //Act
                FolderResponse response = await _foldersModificationService.MoveFolder(guid, destinationDirectoryPath);


                //Assert
                bool dirExists = Directory.Exists(newPathForFOLDER);
                response.Should().NotBeNull();
                response.FolderId.Should().Be(guid);
                response.FolderName.Should().Be(folderName);
                response.FolderPath.Should().Be(newPathForFOLDER);
                dirExists.Should().BeTrue();
            }
            finally 
            {
                Directory.Delete(newPathForFOLDER);
            }
        }
        #endregion



        #region AddOrRemoveFavorite
        public async Task AddOrRemoveFavorite_FolderDoesntExists() 
        {
            //Arrange
            Guid guid = _fixture.Create<Guid>();

            _foldersRepositoryMock.Setup(f => f.GetFolderByFolderId(It.IsAny<Guid>()))
                .ReturnsAsync((Folder) null!);

            //Act
            Func<Task> action = async () =>
            {
                await _foldersModificationService.AddOrRemoveFavorite(guid);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }



        public async Task AddOrRemoveFavorite_AddedToFavSuccesfully() 
        {
            //Arrange
            Guid guid = _fixture.Create<Guid>();
            Folder folder = new Folder() { FolderId = guid, IsFavorite=false };
            _foldersRepositoryMock.Setup(f => f.GetFolderByFolderId(It.IsAny<Guid>()))
                .ReturnsAsync(folder);

            //Act
            FolderResponse fr = await _foldersModificationService.AddOrRemoveFavorite(guid);

            //Assert
            fr.FolderId.Should().Be(guid);
            fr.IsFavorite.Should().BeTrue();
        }



        public async Task AddOrRemoveFavorite_RemovedFromFavSuccesfully() 
        {
            //Arrange
            Guid guid = _fixture.Create<Guid>();
            Folder folder = new Folder() { FolderId = guid, IsFavorite = true };
            _foldersRepositoryMock.Setup(f => f.GetFolderByFolderId(It.IsAny<Guid>()))
                .ReturnsAsync(folder);


            //Act
            FolderResponse fr = await _foldersModificationService.AddOrRemoveFavorite(guid);

            //Assert
            fr.FolderId.Should().Be(guid);  
            fr.IsFavorite.Should().BeFalse();
        }
        #endregion



        #region AddOrRemoveTrash
        public async Task AddOrRemoveTrash_FolderDoesntExists() 
        {
            //Arrange
            Guid guid = _fixture.Create<Guid>();
            _foldersRepositoryMock.Setup(f => f.GetFolderByFolderId(It.IsAny<Guid>()))
                .ReturnsAsync((Folder) null!);

            //Act
            Func<Task> action = async () =>
            {
                await _foldersModificationService.AddOrRemoveTrash(guid);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }



        public async Task AddOrRemoveTrash_AddedToTrashSuccessfully() 
        {
            //Arrange
            Guid guid = _fixture.Create<Guid>();
            Folder folder = new Folder() { FolderId = guid, IsTrash = false };
            _foldersRepositoryMock.Setup(f => f.GetFolderByFolderId(It.IsAny<Guid>()))
                .ReturnsAsync(folder);

            //Act
            FolderResponse fr = await _foldersModificationService.AddOrRemoveTrash(guid);

            //Assert
            fr.FolderId.Should().Be(guid);
            fr.IsFavorite.Should().BeTrue();
        }



        public async Task AddOrRemoveTrash_RemovedFromTrashSuccessfully() 
        {
            //Arrange
            Guid guid = _fixture.Create<Guid>();
            Folder folder = new Folder() { FolderId = guid, IsTrash = true };
            _foldersRepositoryMock.Setup(f => f.GetFolderByFolderId(It.IsAny<Guid>()))
                .ReturnsAsync(folder);

            //Act
            FolderResponse fr = await _foldersModificationService.AddOrRemoveTrash(guid);

            //Assert
            fr.FolderId.Should().Be(guid);
            fr.IsFavorite.Should().BeFalse();
        }
        #endregion



        #region DeleteFolder
        public async Task DeleteFolder_FolderDoesntExists() 
        {
            //Arrange
            Guid guid = _fixture.Create<Guid>();
            _foldersRepositoryMock.Setup(f => f.GetFolderByFolderId(It.IsAny<Guid>()))
                .ReturnsAsync((Folder) null!);

            //Act
            Func<Task> action = async () =>
            {
                await _foldersModificationService.DeleteFolder(guid);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }



        public async Task DeleteFolder_FolderDeletedSuccessfully()
        {
            //Arrange
            Guid guid = _fixture.Create<Guid>();
            string folderName = _fixture.Create<string>();
            string folderPath = Path.Combine(initialPath, folderName);
            Folder folder = new Folder() { FolderId = guid, FolderName = folderName, FolderPath = folderPath};
            _foldersRepositoryMock.Setup(f => f.GetFolderByFolderId(It.IsAny<Guid>()))
                .ReturnsAsync(folder);

            Directory.CreateDirectory(folderPath);
            System.IO.File.Create(Path.Combine(folderPath, _fixture.Create<string>()+".txt"));

            try
            {
                //Act
                bool deleted = await _foldersModificationService.DeleteFolder(guid);

                //Assert
                deleted.Should().BeTrue();
                Directory.EnumerateDirectories(initialPath).Should().BeEmpty();
                Directory.EnumerateFiles(initialPath).Should().BeEmpty();
            }
            catch (Exception ex) 
            {
                Directory.Delete(folderPath, true);
            }
        }
        #endregion
        #endregion

        #region FoldersRetrievalService

        #region GetFoldersByFolderId
        public async Task GetFolderByFolderId_FolderNotFound(Guid id) 
        {
            //Arrange
            Guid guid = _fixture.Create<Guid>();
            _foldersRepositoryMock.Setup(f => f.GetFolderByFolderId(It.IsAny<Guid>()))
                .ReturnsAsync((Folder) null!);

            //Act
            Func<Task> action = async () => 
            {
                await _foldersRetrievalService.GetFolderByFolderId(guid);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        public async Task GetFolderByFolderId_Success(Guid id)
        {
            //Arrange
            Guid guid = _fixture.Create<Guid>();
            Folder folder = new Folder() { FolderId = guid };
            _foldersRepositoryMock.Setup(f => f.GetFolderByFolderId(It.IsAny<Guid>()))
                .ReturnsAsync(folder);

            //Act
            FolderResponse fr = await _foldersRetrievalService.GetFolderByFolderId(guid);

            //Assert
            fr.Should().NotBeNull();
            fr.FolderId.Should().Be(guid);
        }
        #endregion

        #endregion
    }
}