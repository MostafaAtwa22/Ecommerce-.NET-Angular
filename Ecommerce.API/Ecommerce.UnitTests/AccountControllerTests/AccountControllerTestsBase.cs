
namespace Ecommerce.UnitTests.AccountControllerTests
{
    [Collection("Hangfire")]
    public class AccountControllerTestsBase
    {
        protected readonly Mock<UserManager<ApplicationUser>> _userManager;
        protected readonly Mock<SignInManager<ApplicationUser>> _signInManager;
        protected readonly Mock<ITokenService> _tokenService;
        protected readonly Mock<IEmailService> _emailService;
        protected readonly Mock<IConfiguration> _config;
        protected readonly Mock<IGoogleService> _googleService;
        protected readonly Mock<IMapper> _mapper;
        protected readonly Mock<IPermissionService> _permissionService;

        protected readonly AccountController _controller;

        public AccountControllerTestsBase()
        {
            _userManager = MockUserManager();
            _signInManager = MockSignInManager(_userManager.Object);
            _tokenService = new Mock<ITokenService>();
            _emailService = new Mock<IEmailService>();
            _config = new Mock<IConfiguration>();
            _googleService = new Mock<IGoogleService>();
            _mapper = new Mock<IMapper>();
            _permissionService = new Mock<IPermissionService>();

            _controller = new AccountController(
                _userManager.Object,
                _signInManager.Object,
                _tokenService.Object,
                _emailService.Object,
                _config.Object,
                _googleService.Object,
                _mapper.Object,
                _permissionService.Object
            );

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        private static Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private static Mock<SignInManager<ApplicationUser>> MockSignInManager(
            UserManager<ApplicationUser> userManager)
        {
            return new Mock<SignInManager<ApplicationUser>>(
                userManager,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null!, null!, null!, null!);
        }
    }
}
