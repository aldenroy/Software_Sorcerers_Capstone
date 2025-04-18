using Microsoft.EntityFrameworkCore;
using Moq;
using MoviesMadeEasy.DAL.Concrete;
using MoviesMadeEasy.Models;
using MoviesMadeEasy.Data;


namespace MME_Tests.DAL
{
    [TestFixture]
    public class TitleRepositoryTests
    {
        private Mock<UserDbContext> _mockContext;
        private Mock<DbSet<Title>> _mockTitles;
        private TitleRepository _repo;
        private List<Title> _titleData;

        [SetUp]
        public void SetUp()
        {
            _titleData = new List<Title>();
            _mockTitles = MockHelper.GetMockDbSet(_titleData.AsQueryable());

            _mockContext = new Mock<UserDbContext>();
            _mockContext.Setup(c => c.Titles).Returns(_mockTitles.Object);

            _repo = new TitleRepository(_mockContext.Object);
        }

        [Test]
        public void CaptureOrUpdate_NewTitle_AddsToDbSet()
        {
            var newTitle = new Title { TitleName = "John Wick", Year = 2014 };
            _repo.CaptureOrUpdate(newTitle);

            _mockTitles.Verify(d => d.Add(It.Is<Title>(t => t.TitleName == "John Wick" && t.Year == 2014)), Times.Once);
            _mockTitles.Verify(d => d.Update(It.IsAny<Title>()), Times.Never);
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        [Test]
        public void CaptureOrUpdate_ExistingTitle_UpdatesLastUpdated()
        {
            _titleData.Add(new Title { Id = 1, TitleName = "John Wick", Year = 2014, LastUpdated = DateTime.UtcNow.AddDays(-10) });
            _mockTitles = MockHelper.GetMockDbSet(_titleData.AsQueryable());
            _mockContext.Setup(c => c.Titles).Returns(_mockTitles.Object);
            _repo = new TitleRepository(_mockContext.Object);
            var updatedTitle = new Title { TitleName = "John Wick", Year = 2014 };
            _repo.CaptureOrUpdate(updatedTitle);


            _mockTitles.Verify(d => d.Update(It.Is<Title>(t => t.TitleName == "John Wick" && t.Year == 2014)), Times.Once);
            _mockTitles.Verify(d => d.Add(It.IsAny<Title>()), Times.Never);
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        [Test]
        public void CaptureOrUpdate_NullTitle_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _repo.CaptureOrUpdate(null));
        }
    }
}
