using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using MoviesMadeEasy.DAL.Concrete;
using MoviesMadeEasy.Models;
using MoviesMadeEasy.Data;

namespace MME_Tests
{
    [TestFixture]
    public class TitleRepositoryTests
    {
        private Mock<UserDbContext> _mockContext;
        private Mock<DbSet<Title>> _mockTitles;
        private List<Title> _titleData;

        private Mock<DbSet<RecentlyViewedTitle>> _mockRvt;
        private List<RecentlyViewedTitle> _rvtData;

        private TitleRepository _repo;

        [SetUp]
        public void SetUp()
        {
            _titleData = new List<Title>();
            _mockTitles = MockHelper.GetMockDbSet(_titleData.AsQueryable());

            _rvtData = new List<RecentlyViewedTitle>();
            _mockRvt = MockHelper.GetMockDbSet(_rvtData.AsQueryable());

            _mockContext = new Mock<UserDbContext>(new DbContextOptions<UserDbContext>());
            _mockContext.Setup(c => c.Titles).Returns(_mockTitles.Object);
            _mockContext.Setup(c => c.RecentlyViewedTitles).Returns(_mockRvt.Object);

            _repo = new TitleRepository(_mockContext.Object);
        }

        [Test]
        public void RecordTitleView_NewTitle_AddsToTitlesAndRvt()
        {
            var newTitle = new Title { TitleName = "John Wick", Year = 2014 };

            _repo.RecordTitleView(newTitle, userId: 42);

            _mockTitles.Verify(d => d.Add(It.Is<Title>(t => t.TitleName == "John Wick" && t.Year == 2014)), Times.Once);
            _mockTitles.Verify(d => d.Update(It.IsAny<Title>()), Times.Never);

            _mockRvt.Verify(d => d.Add(It.Is<RecentlyViewedTitle>(rv => rv.UserId == 42)), Times.Once);

            _mockContext.Verify(c => c.SaveChanges(), Times.Exactly(2));
        }

        [Test]
        public void RecordTitleView_ExistingTitle_AddsOnlyRvt()
        {
            var existing = new Title { Id = 1, TitleName = "John Wick", Year = 2014, LastUpdated = DateTime.UtcNow.AddDays(-10) };
            _titleData.Add(existing);
            _mockTitles = MockHelper.GetMockDbSet(_titleData.AsQueryable());
            _mockContext.Setup(c => c.Titles).Returns(_mockTitles.Object);
            _repo = new TitleRepository(_mockContext.Object);

            var toRecord = new Title { TitleName = "John Wick", Year = 2014 };
            _repo.RecordTitleView(toRecord, userId: 99);


            _mockTitles.Verify(d => d.Add(It.IsAny<Title>()), Times.Never); 
            _mockTitles.Verify(d => d.Update(It.IsAny<Title>()), Times.Never);

            _mockRvt.Verify(d => d.Add(It.Is<RecentlyViewedTitle>(rv => rv.UserId == 99 && rv.TitleId == 1)), Times.Once);

            _mockContext.Verify(c => c.SaveChanges(), Times.Exactly(1));
        }

        [Test]
        public void RecordTitleView_ExistingRecentlyViewedTitle_UpdatesViewedAt()
        {
            var existingTitle = new Title { Id = 1, TitleName = "Inception", Year = 2010 };
            _titleData.Add(existingTitle);
            _rvtData.Add(new RecentlyViewedTitle { UserId = 7, TitleId = 1, ViewedAt = DateTime.UtcNow.AddDays(-10) });

            _mockTitles = MockHelper.GetMockDbSet(_titleData.AsQueryable());
            _mockRvt = MockHelper.GetMockDbSet(_rvtData.AsQueryable());
            _mockContext.Setup(c => c.Titles).Returns(_mockTitles.Object);
            _mockContext.Setup(c => c.RecentlyViewedTitles).Returns(_mockRvt.Object);
            _repo = new TitleRepository(_mockContext.Object);

            var updatedTitle = new Title { TitleName = "Inception", Year = 2010 };
            _repo.RecordTitleView(updatedTitle, userId: 7);

            _mockRvt.Verify(d => d.Update(It.Is<RecentlyViewedTitle>(rv => rv.UserId == 7 && rv.TitleId == 1)), Times.Once);
            _mockRvt.Verify(d => d.Add(It.IsAny<RecentlyViewedTitle>()), Times.Never);

            _mockContext.Verify(c => c.SaveChanges(), Times.Exactly(1));
        }


        [Test]
        public void GetRecentlyViewedByUser_ReturnsCorrectTitles()
        {
            var userId = 7;
            var title1 = new Title { Id = 1, TitleName = "Inception", Year = 2010 };
            var title2 = new Title { Id = 2, TitleName = "The Matrix", Year = 1999 };

            _titleData.AddRange(new[] { title1, title2 });
            _rvtData.AddRange(new[]
            {
                new RecentlyViewedTitle { UserId = userId, Title = title1, TitleId = 1, ViewedAt = DateTime.UtcNow.AddHours(-2) },
                new RecentlyViewedTitle { UserId = userId, Title = title2, TitleId = 2, ViewedAt = DateTime.UtcNow.AddHours(-1) },
            });

            _mockTitles = MockHelper.GetMockDbSet(_titleData.AsQueryable());
            _mockRvt = MockHelper.GetMockDbSet(_rvtData.AsQueryable());
            _mockContext.Setup(c => c.Titles).Returns(_mockTitles.Object);
            _mockContext.Setup(c => c.RecentlyViewedTitles).Returns(_mockRvt.Object);
            _repo = new TitleRepository(_mockContext.Object);

            var result = _repo.GetRecentlyViewedByUser(userId);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(t => t.TitleName == "Inception"));
            Assert.IsTrue(result.Any(t => t.TitleName == "The Matrix"));
        }

        [Test]
        public void GetRecentlyViewedByUser_ReturnsEmptyList_WhenNoViews()
        {
            var userId = 99;

            _mockContext.Setup(c => c.RecentlyViewedTitles).Returns(_mockRvt.Object);
            _repo = new TitleRepository(_mockContext.Object);

            var result = _repo.GetRecentlyViewedByUser(userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }


        [Test]
        public void RecordTitleView_NullTitle_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _repo.RecordTitleView(null, userId: 5));
        }
    }
}
