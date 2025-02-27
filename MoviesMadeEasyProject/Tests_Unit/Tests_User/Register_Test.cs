using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;
using NUnit.Framework;
using MoviesMadeEasy.Areas.Identity.Pages.Account;
using MoviesMadeEasy.Models;
using MoviesMadeEasy.Data;

namespace MME_Tests
{
    [TestFixture]
    public class RegisterPageTests
    {
        private Mock<UserManager<IdentityUser>> _userManagerMock;
        private Mock<SignInManager<IdentityUser>> _signInManagerMock;
        private Mock<ILogger<RegisterModel>> _loggerMock;
        private Mock<IEmailSender> _emailSenderMock;
        private Mock<IdentityDbContext> _identityDbContextMock;
        private Mock<UserDbContext> _userDbContextMock;
        private RegisterModel _registerModel;

        [SetUp]
        public void Setup()
        {
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                new Mock<IUserStore<IdentityUser>>().Object,
                null, null, null, null, null, null, null, null
            );

            _signInManagerMock = new Mock<SignInManager<IdentityUser>>(
                _userManagerMock.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object,
                null, null, null, null
            );

            _loggerMock = new Mock<ILogger<RegisterModel>>();
            _emailSenderMock = new Mock<IEmailSender>();

            _registerModel = new RegisterModel(
                _userManagerMock.Object,
                new Mock<IUserStore<IdentityUser>>().Object,  
                _signInManagerMock.Object,
                _loggerMock.Object,
                _emailSenderMock.Object,
                _identityDbContextMock.Object,
                _userDbContextMock.Object
            );
        }
    }
}